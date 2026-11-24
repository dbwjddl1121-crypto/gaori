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

        [Tooltip("소리 앞부분을 이 시간(초)만큼 건너뜀. 0이면 처음부터")]
        public float trimStart = 0f;
        [Tooltip("소리를 이 시간(초)에서 끝냄. 0이면 끝까지")]
        public float trimEnd = 0f;
        [Tooltip("끝에서 서서히 작아지는 시간(초)")]
        public float trimFade = 0.1f;
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

    // 자르기(Trim) 효과음용: 동시에 여러 개 재생할 수 있게 소리 칸을 모아 둠
    private List<AudioSource> trimPool = new List<AudioSource>();
    private Dictionary<AudioSource, int> trimTokens = new Dictionary<AudioSource, int>();
    private int trimCounter = 0;

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
        PlayEntry(sfxName, 1f);
    }

    // 이름으로 재생하되 크기를 따로 곱해서 조절 (0~1, 작을수록 조용함)
    // 예: AudioManager.Instance.PlaySFXScaled("error", 0.3f);
    public void PlaySFXScaled(string sfxName, float scale)
    {
        PlayEntry(sfxName, scale);
    }

    void PlayEntry(string sfxName, float scale)
    {
        foreach (var entry in sfxList)
        {
            if (entry.name == sfxName)
            {
                if (entry.clip == null) return;

                float vol = entry.volume * SettingsManager.SFXVolume * scale;

                // 자르기 값이 있으면 잘라서 재생, 없으면 그대로 재생
                if (entry.trimStart > 0f || entry.trimEnd > 0f)
                    PlayTrimmed(entry, vol);
                else
                    sfxSource.PlayOneShot(entry.clip, vol);

                return;
            }
        }

        Debug.LogWarning("효과음을 찾을 수 없어요: " + sfxName);
    }

    // 소리 파일을 직접 넘겨서 재생
    public void PlaySFXClip(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, SettingsManager.SFXVolume);
    }

    // ───────── 자르기(Trim) 재생 ─────────

    void PlayTrimmed(SFXEntry entry, float vol)
    {
        AudioClip clip = entry.clip;

        float start = Mathf.Clamp(entry.trimStart, 0f, Mathf.Max(0f, clip.length - 0.05f));
        float end = entry.trimEnd > 0f ? Mathf.Min(entry.trimEnd, clip.length) : clip.length;
        float length = Mathf.Max(0.05f, end - start);
        float fade = Mathf.Clamp(entry.trimFade, 0.01f, length);

        AudioSource src = GetTrimSource();
        src.clip = clip;
        src.volume = vol;
        src.time = start;
        src.Play();

        trimCounter++;
        trimTokens[src] = trimCounter;
        StartCoroutine(TrimRoutine(src, trimCounter, vol, length, fade));
    }

    // 쉬고 있는 소리 칸을 찾고, 없으면 새로 만듦
    AudioSource GetTrimSource()
    {
        foreach (AudioSource s in trimPool)
        {
            if (s != null && !s.isPlaying)
                return s;
        }

        AudioSource created = gameObject.AddComponent<AudioSource>();
        created.loop = false;
        created.playOnAwake = false;
        trimPool.Add(created);
        return created;
    }

    IEnumerator TrimRoutine(AudioSource src, int token, float baseVolume, float length, float fade)
    {
        float hold = Mathf.Max(0f, length - fade);
        if (hold > 0f)
            yield return new WaitForSecondsRealtime(hold);

        float t = 0f;
        while (t < fade)
        {
            // 그 사이 같은 칸이 다른 소리에 쓰였으면 건드리지 않음
            if (src == null || trimTokens[src] != token) yield break;

            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(baseVolume, 0f, t / fade);
            yield return null;
        }

        if (src != null && trimTokens[src] == token)
            src.Stop();
    }

    // ───────── 정해진 시간만 재생 ─────────

    // 긴 효과음을 앞부분 seconds초만 재생하고, 마지막 fadeOut초는 서서히 작아지게 함
    // 등록된 Trim Start가 있으면 거기서부터 시작하고, Trim End가 있으면 그 안에서 끝남
    // 예: AudioManager.Instance.PlaySFXFor("paper_crumple", 4.5f);
    public void PlaySFXFor(string sfxName, float seconds, float fadeOut = 0.3f)
    {
        foreach (var entry in sfxList)
        {
            if (entry.name == sfxName)
            {
                if (entry.clip == null) return;

                if (limitedRoutine != null) StopCoroutine(limitedRoutine);

                float start = Mathf.Clamp(entry.trimStart, 0f, Mathf.Max(0f, entry.clip.length - 0.05f));

                // Trim End가 있으면 그 시간을 넘지 않게 함
                if (entry.trimEnd > 0f)
                    seconds = Mathf.Min(seconds, Mathf.Max(0.05f, entry.trimEnd - start));

                limitedSource.Stop();
                limitedSource.clip = entry.clip;
                limitedSource.volume = entry.volume * SettingsManager.SFXVolume;
                limitedSource.time = start;
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