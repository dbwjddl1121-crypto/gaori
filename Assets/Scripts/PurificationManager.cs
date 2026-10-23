using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PurificationManager : MonoBehaviour
{
    [Header("스테이지 번호 (0=연안, 1=중층, 2=심해)")]
    [SerializeField] int stageIndex = 0;

    [Header("쓰레기")]
    [SerializeField] string trashTag = "Trash";

    [Header("바다 이미지 (겹쳐서 크로스페이드)")]
    [SerializeField] SpriteRenderer pollutedRenderer;
    [SerializeField] SpriteRenderer cleanRenderer;

    [Header("연출 시간")]
    [SerializeField] float cameraZoomDuration = 1.5f;  // 전체 화면으로 줌아웃하는 시간
    [SerializeField] float crossfadeDuration = 3.5f;   // 바다가 바뀌는 시간
    [SerializeField] float sparkleDuration = 2.5f;     // 전체 화면 샤라라 시간
    [SerializeField] float holdAfterSparkle = 0.5f;

    [Header("전체 화면 맞춤")]
    [SerializeField] float fitPadding = 1.0f;          // 1=배경에 딱 맞게, 1.1=여유

    [Header("샤라라 효과")]
    [SerializeField] ParticleSystem sparkle;
    [SerializeField] AudioSource sparkleSound;
    [SerializeField] int sparkleCount = 150;           // 초당 개수
    [SerializeField] float sparkleSizeRatio = 0.03f;   // 화면 높이 대비 파티클 크기

    [Header("UI (선택)")]
    [SerializeField] TMP_Text percentText;
    [SerializeField] Slider percentSlider;

    [Header("나가기")]
    [SerializeField] string returnSceneName = "GameScene";

    List<GameObject> trashList = new List<GameObject>();
    int totalTrash;
    bool completed;
    float timer;

    void Start()
    {
        bool isPreview = gameObject.scene != SceneManager.GetActiveScene();

        if (sparkle != null)
            sparkle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (StageProgress.IsCleaned(stageIndex))
        {
            foreach (var go in FindTrashInThisScene())
                go.SetActive(false);

            SetAlpha(cleanRenderer, 1f);
            SetAlpha(pollutedRenderer, 0f);
            UpdateUI(1f);
            completed = true;
            return;
        }

        SetAlpha(pollutedRenderer, 1f);
        SetAlpha(cleanRenderer, 0f);

        if (isPreview)
        {
            UpdateUI(0f);
            completed = true;
            return;
        }

        trashList = FindTrashInThisScene();
        totalTrash = trashList.Count;
        UpdateUI(0f);

        if (totalTrash == 0)
            Debug.LogWarning("Trash 태그가 붙은 쓰레기를 찾지 못했어요.");
    }

    void Update()
    {
        if (completed || totalTrash == 0) return;

        timer += Time.deltaTime;
        if (timer < 0.2f) return;
        timer = 0f;

        int remaining = 0;
        foreach (var go in trashList)
        {
            if (go != null && go.activeInHierarchy)
                remaining++;
        }

        float percent = 1f - (float)remaining / totalTrash;
        UpdateUI(percent);

        if (remaining == 0)
        {
            completed = true;
            StartCoroutine(CompleteRoutine());
        }
    }

    List<GameObject> FindTrashInThisScene()
    {
        var result = new List<GameObject>();
        foreach (var go in GameObject.FindGameObjectsWithTag(trashTag))
        {
            if (go.scene == gameObject.scene)
                result.Add(go);
        }
        return result;
    }

    IEnumerator CompleteRoutine()
    {
        StageProgress.SetCleaned(stageIndex);

        // 0) 카메라를 배경 전체가 보이게 줌아웃
        Camera cam = Camera.main;
        Bounds bounds = GetOceanBounds();
        float fitSize = 0f;
        Vector3 fitPos = Vector3.zero;

        if (cam != null && cam.orthographic)
        {
            // 따라다니기/줌 스크립트 끄기
            foreach (var mb in cam.GetComponents<MonoBehaviour>())
            {
                if (mb == null) continue;
                if (mb is Physics2DRaycasterMarker) continue;
                string n = mb.GetType().Name;
                if (n.Contains("Follow") || n.Contains("Zoom"))
                    mb.enabled = false;
            }

            float sizeByH = bounds.size.y / 2f;
            float sizeByW = bounds.size.x / (2f * cam.aspect);
            fitSize = Mathf.Max(sizeByH, sizeByW) * fitPadding;
            fitPos = new Vector3(bounds.center.x, bounds.center.y, cam.transform.position.z);

            Vector3 startPos = cam.transform.position;
            float startSize = cam.orthographicSize;
            float zt = 0f;
            while (zt < cameraZoomDuration)
            {
                zt += Time.deltaTime;
                float p = Mathf.SmoothStep(0f, 1f, zt / cameraZoomDuration);
                cam.transform.position = Vector3.Lerp(startPos, fitPos, p);
                cam.orthographicSize = Mathf.Lerp(startSize, fitSize, p);
                yield return null;
            }
            cam.transform.position = fitPos;
            cam.orthographicSize = fitSize;
        }

        // 1) 오염 -> 깨끗 서서히 변화
        float t = 0f;
        while (t < crossfadeDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / crossfadeDuration);
            SetAlpha(cleanRenderer, p);
            SetAlpha(pollutedRenderer, 1f - p);
            yield return null;
        }
        SetAlpha(cleanRenderer, 1f);
        SetAlpha(pollutedRenderer, 0f);

        // 2) 전체 화면 샤라라
        PlayFullScreenSparkle(bounds);
        if (sparkleSound != null) sparkleSound.Play();
        yield return new WaitForSeconds(sparkleDuration + holdAfterSparkle);

        // 3) 페이드 아웃하며 스테이지 선택창으로
        StageProgress.openStageSelect = true;
        StageProgress.returnStage = stageIndex;

        var fader = FindFirstObjectByType<SceneFader>();
        if (fader != null)
            fader.FadeToScene(returnSceneName);
        else
            SceneManager.LoadScene(returnSceneName);
    }

    Bounds GetOceanBounds()
    {
        if (cleanRenderer != null) return cleanRenderer.bounds;
        if (pollutedRenderer != null) return pollutedRenderer.bounds;
        return new Bounds(Vector3.zero, new Vector3(100, 100, 1));
    }

    void PlayFullScreenSparkle(Bounds bounds)
    {
        if (sparkle == null) return;

        sparkle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        float h = bounds.size.y;
        float w = bounds.size.x;

        sparkle.transform.position = new Vector3(bounds.center.x, bounds.center.y, 0f);
        sparkle.transform.rotation = Quaternion.identity;
        sparkle.transform.localScale = Vector3.one;

        var main = sparkle.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = sparkleDuration;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.6f);
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(h * sparkleSizeRatio * 0.5f, h * sparkleSizeRatio);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 1f, 1f, 1f),
            new Color(1f, 0.95f, 0.6f, 1f));
        main.maxParticles = 2000;

        var shape = sparkle.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.position = Vector3.zero;
        shape.rotation = Vector3.zero;
        shape.scale = new Vector3(w, h, 1f);

        var emission = sparkle.emission;
        emission.enabled = true;
        emission.rateOverTime = sparkleCount;

        // 시간에 따라 반짝이며 사라지게
        var col = sparkle.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.3f),
                new GradientAlphaKey(0f, 1f) });
        col.color = g;

        var renderer = sparkle.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = 200;
            if (renderer.sharedMaterial == null)
                renderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        sparkle.Play();
    }

    void UpdateUI(float percent)
    {
        int p = Mathf.RoundToInt(percent * 100f);
        if (percentText != null) percentText.text = "정화도 " + p + "%";
        if (percentSlider != null) percentSlider.value = percent;
    }

    void SetAlpha(SpriteRenderer sr, float a)
    {
        if (sr == null) return;
        var c = sr.color;
        c.a = a;
        sr.color = c;
    }
}

// 컴파일용 더미 (Physics2DRaycaster를 건너뛰기 위한 표시용)
class Physics2DRaycasterMarker : MonoBehaviour { }