using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Serializable]
    public class SceneBGM
    {
        public string sceneName;   // 씬 이름 (정확히 똑같이)
        public AudioClip clip;     // 그 씬에서 틀 음악
    }

    [Serializable]
    public class SFXEntry
    {
        public string name;        // 부를 이름 (예: coin, click)
        public AudioClip clip;     // 재생할 효과음
        [Range(0f, 1f)] public float volume = 1f;  // 이 소리만 따로 조절하는 크기

        [Tooltip("소리의 앞부분을 몇 초 건너뛰고 재생할지 (0이면 처음부터)")]
        public float startTime = 0f;

        [Tooltip("소리의 뒷부분을 몇 초 자를지 (0이면 끝까지)")]
        public float endTrim = 0f;
    }

    [Header("BGM")]
    public AudioSource bgmSource;
    public List<SceneBGM> sceneBgmList = new List<SceneBGM>();

    [Tooltip("음악 전체 최대 크기. 슬라이더를 끝까지 올려도 이 값을 넘지 않아요. 너무 크면 이 값을 줄이세요.")]
    [Range(0f, 1f)] public float bgmGain = 0.4f;

    [Header("SFX")]
    public AudioSource sfxSource;
    public List<SFXEntry> sfxList = new List<SFXEntry>();

    // 설정창 슬라이더 값 (0~1). 저장된 값을 시작할 때 바로 불러옴
    private float bgmVolume = 0.7f;

    // 정해진 시간만 재생하는 효과음용
    private AudioSource limitedSource;
    private Coroutine limitedRoutine;

    // 앞뒤를 자르고 재생하는 효과음용 (연타해도 겹쳐서 재생되도록 여러 개 돌려 씀)
    private const int PoolSize = 6;
    private const float TrimFade = 0.08f;   // 뒤를 자를 때 부드럽게 줄어드는 시간
    private AudioSource[] pool;
    private Coroutine[] poolRoutines;
    private int poolIndex = 0;

    void Awake()
    {
        // 이미 매니저가 있으면 새로 생긴 건 지움 (타이틀로 돌아와도 중복 방지)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 저장된 BGM 볼륨을 시작하자마자 적용 (처음부터 큰 소리가 나는 걸 막음)
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.7f);

        if (bgmSource == null)
            bgmSource = gameObject.AddComponent<AudioSource>();

        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        ApplyBGMVolume();

        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        limitedSource = gameObject.AddComponent<AudioSource>();
        limitedSource.loop = false;
        limitedSource.playOnAwake = false;

        pool = new AudioSource[PoolSize];
        poolRoutines = new Coroutine[PoolSize];
        for (int i = 0; i < PoolSize; i++)
        {
            pool[i] = gameObject.AddComponent<AudioSource>();
            pool[i].loop = false;
            pool[i].playOnAwake = false;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        // 첫 씬(타이틀)은 sceneLoaded가 이미 지나갔을 수 있어서 직접 한 번 실행
        PlayForScene(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayForScene(scene.name);
    }

    void PlayForScene(string sceneName)
    {
        foreach (var entry in sceneBgmList)
        {
            if (entry.sceneName == sceneName && entry.clip != null)
            {
                PlayBGM(entry.clip);
                return;
            }
        }
        // 목록에 없는 씬은 지금 나오는 음악을 그대로 유지
    }

    // ───────── BGM ─────────

    public void PlayBGM(AudioClip clip)
    {
        // 같은 곡이면 처음부터 다시 틀지 않음
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        ApplyBGMVolume();
        bgmSource.Play();
    }

    // 설정창 슬라이더에서 호출 (0~1)
    public void SetBGMVolume(float value)
    {
        bgmVolume = Mathf.Clamp01(value);
        ApplyBGMVolume();
    }

    // 실제 소리 = 슬라이더 값 × 최대 크기
    void ApplyBGMVolume()
    {
        if (bgmSource != null)
            bgmSource.volume = bgmVolume * bgmGain;
    }

    // ───────── SFX ─────────

    // 이름으로 재생: AudioManager.Instance.PlaySFX("coin");
    // 버튼 On Click에도 문자열을 적어서 쓸 수 있음
    public void PlaySFX(string sfxName)
    {
        foreach (var entry in sfxList)
        {
            if (entry.name == sfxName)
            {
                if (entry.clip == null) return;
                PlayEntry(entry);
                return;
            }
        }

        Debug.LogWarning("효과음을 찾을 수 없어요: " + sfxName);
    }

    // Start Time, End Trim이 있으면 앞뒤를 자르고, 없으면 그대로 재생
    void PlayEntry(SFXEntry entry)
    {
        float vol = entry.volume * SettingsManager.SFXVolume;

        float start = Mathf.Max(0f, entry.startTime);
        float trim = Mathf.Max(0f, entry.endTrim);

        // 앞 건너뛰기가 소리보다 길면 처음부터 재생
        if (start >= entry.clip.length)
            start = 0f;

        // 자를 게 없으면 기본 방식으로 재생
        if (start <= 0f && trim <= 0f)
        {
            sfxSource.PlayOneShot(entry.clip, vol);
            return;
        }

        // 실제로 재생할 길이 (너무 짧아지지 않게 최소 0.1초)
        float playLength = Mathf.Max(0.1f, entry.clip.length - start - trim);

        int i = poolIndex;
        poolIndex = (poolIndex + 1) % pool.Length;

        AudioSource src = pool[i];

        if (poolRoutines[i] != null)
        {
            StopCoroutine(poolRoutines[i]);
            poolRoutines[i] = null;
        }

        src.Stop();
        src.clip = entry.clip;
        src.volume = vol;
        src.time = start;
        src.Play();

        // 뒤를 자르는 경우에만 시간 맞춰 부드럽게 끝냄
        if (trim > 0f)
            poolRoutines[i] = StartCoroutine(StopAfter(i, playLength, vol));
    }

    IEnumerator StopAfter(int i, float playLength, float vol)
    {
        AudioSource src = pool[i];

        float fade = Mathf.Min(TrimFade, playLength * 0.5f);
        float hold = Mathf.Max(0f, playLength - fade);

        yield return new WaitForSecondsRealtime(hold);

        float t = 0f;
        while (t < fade)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(vol, 0f, t / Mathf.Max(0.01f, fade));
            yield return null;
        }

        src.Stop();
        src.volume = vol;
        poolRoutines[i] = null;
    }

    // 소리 파일을 직접 넘겨서 재생
    public void PlaySFXClip(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, SettingsManager.SFXVolume);
    }

    // 긴 효과음을 앞부분 seconds초만 재생하고, 마지막 fadeOut초는 서서히 작아지게 함
    // 예: AudioManager.Instance.PlaySFXFor("paper_crumple", 4.5f);
    public void PlaySFXFor(string sfxName, float seconds, float fadeOut = 0.3f)
    {
        foreach (var entry in sfxList)
        {
            if (entry.name == sfxName)
            {
                if (entry.clip == null) return;

                if (limitedRoutine != null) StopCoroutine(limitedRoutine);

                limitedSource.Stop();
                limitedSource.clip = entry.clip;
                limitedSource.volume = entry.volume * SettingsManager.SFXVolume;
                limitedSource.time = 0f;
                limitedSource.Play();

                float hold = Mathf.Max(0f, seconds - fadeOut);
                limitedRoutine = StartCoroutine(LimitedRoutine(hold, fadeOut));
                return;
            }
        }

        Debug.LogWarning("효과음을 찾을 수 없어요: " + sfxName);
    }

    // PlaySFXFor로 재생 중인 소리를 부드럽게 멈춤
    public void StopLimitedSFX(float fadeOut = 0.2f)
    {
        if (limitedSource == null || !limitedSource.isPlaying) return;

        if (limitedRoutine != null) StopCoroutine(limitedRoutine);
        limitedRoutine = StartCoroutine(LimitedRoutine(0f, fadeOut));
    }

    IEnumerator LimitedRoutine(float hold, float fadeOut)
    {
        float baseVolume = limitedSource.volume;

        if (hold > 0f)
            yield return new WaitForSecondsRealtime(hold);

        float duration = Mathf.Max(0.01f, fadeOut);
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            limitedSource.volume = Mathf.Lerp(baseVolume, 0f, t / duration);
            yield return null;
        }

        limitedSource.Stop();
        limitedSource.volume = baseVolume;
        limitedRoutine = null;
    }
}