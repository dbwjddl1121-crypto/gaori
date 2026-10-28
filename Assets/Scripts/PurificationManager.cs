
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
    [SerializeField] float cameraZoomDuration = 1.5f;
    [SerializeField] float crossfadeDuration = 3.5f;
    [SerializeField] float sparkleDuration = 2.5f;
    [SerializeField] float holdAfterSparkle = 0.5f;

    [Header("전체 화면 맞춤")]
    [SerializeField] float fitPadding = 1.0f;

    [Header("샤라라 효과")]
    [SerializeField] ParticleSystem sparkle;
    [SerializeField] AudioSource sparkleSound;
    [SerializeField] int sparkleCount = 100;

    [Tooltip("파티클 크기 비율. 작을수록 입자가 작아집니다.")]
    [SerializeField] float sparkleSizeRatio = 0.008f;

    [Tooltip("입자가 퍼지는 속도")]
    [SerializeField] float sparkleSpreadSpeed = 0.5f;

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
        if (completed || totalTrash == 0)
            return;

        timer += Time.deltaTime;

        if (timer < 0.2f)
            return;

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

        Camera cam = Camera.main;
        Bounds bounds = GetOceanBounds();

        float fitSize = 0f;
        Vector3 fitPos = Vector3.zero;

        if (cam != null && cam.orthographic)
        {
            foreach (var mb in cam.GetComponents<MonoBehaviour>())
            {
                if (mb == null)
                    continue;

                if (mb is Physics2DRaycasterMarker)
                    continue;

                string n = mb.GetType().Name;

                if (n.Contains("Follow") || n.Contains("Zoom"))
                    mb.enabled = false;
            }

            float sizeByH = bounds.size.y / 2f;
            float sizeByW = bounds.size.x / (2f * cam.aspect);

            fitSize = Mathf.Max(sizeByH, sizeByW) * fitPadding;

            fitPos = new Vector3(
                bounds.center.x,
                bounds.center.y,
                cam.transform.position.z
            );

            Vector3 startPos = cam.transform.position;
            float startSize = cam.orthographicSize;
            float zt = 0f;

            while (zt < cameraZoomDuration)
            {
                zt += Time.deltaTime;

                float p = Mathf.SmoothStep(
                    0f, 1f, zt / cameraZoomDuration
                );

                cam.transform.position = Vector3.Lerp(startPos, fitPos, p);
                cam.orthographicSize = Mathf.Lerp(startSize, fitSize, p);

                yield return null;
            }

            cam.transform.position = fitPos;
            cam.orthographicSize = fitSize;
        }

        // 오염된 바다에서 깨끗한 바다로 부드럽게 전환
        float t = 0f;

        while (t < crossfadeDuration)
        {
            t += Time.deltaTime;

            float p = Mathf.SmoothStep(
                0f, 1f, t / crossfadeDuration
            );

            SetAlpha(cleanRenderer, p);
            SetAlpha(pollutedRenderer, 1f - p);

            yield return null;
        }

        SetAlpha(cleanRenderer, 1f);
        SetAlpha(pollutedRenderer, 0f);

        // 바다가 깨끗해진 뒤 화면 전체에 반짝이 표시
        PlayFullScreenSparkle(bounds);

        if (sparkleSound != null)
            sparkleSound.Play();

        yield return new WaitForSeconds(
            sparkleDuration + holdAfterSparkle
        );

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
        if (cleanRenderer != null)
            return cleanRenderer.bounds;

        if (pollutedRenderer != null)
            return pollutedRenderer.bounds;

        return new Bounds(
            Vector3.zero,
            new Vector3(100, 100, 1)
        );
    }

    void PlayFullScreenSparkle(Bounds bounds)
    {
        if (sparkle == null)
        {
            Debug.LogError(
                "Sparkle가 연결되지 않았습니다. " +
                "Inspector의 PurificationManager를 확인하세요."
            );
            return;
        }

        sparkle.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );

        float h = bounds.size.y;
        float w = bounds.size.x;

        sparkle.transform.position = new Vector3(
            bounds.center.x,
            bounds.center.y,
            0f
        );

        sparkle.transform.rotation = Quaternion.identity;
        sparkle.transform.localScale = Vector3.one;

        var main = sparkle.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = sparkleDuration;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.4f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(
            0.05f, sparkleSpreadSpeed
        );

        // 작은 빛 입자
        main.startSize = new ParticleSystem.MinMaxCurve(
            Mathf.Max(0.01f, h * sparkleSizeRatio * 0.5f),
            Mathf.Max(0.02f, h * sparkleSizeRatio)
        );

        // 흰빛과 은은한 금빛
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 1f, 1f, 0.9f),
            new Color(1f, 0.94f, 0.72f, 0.85f)
        );

        main.maxParticles = 2000;

        // 화면 전체에 고르게 생성
        var shape = sparkle.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.position = Vector3.zero;
        shape.rotation = Vector3.zero;
        shape.scale = new Vector3(w, h, 1f);

        var emission = sparkle.emission;
        emission.enabled = true;
        emission.rateOverTime = sparkleCount;

        // 처음과 끝은 투명하고 중간에 은은하게 빛남
        var col = sparkle.colorOverLifetime;
        col.enabled = true;

        Gradient g = new Gradient();

        g.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(
                    new Color(1f, 0.97f, 0.82f), 0.5f
                ),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.8f, 0.2f),
                new GradientAlphaKey(0.65f, 0.65f),
                new GradientAlphaKey(0f, 1f)
            }
        );

        col.color = g;

        var renderer = sparkle.GetComponent<ParticleSystemRenderer>();

        if (renderer != null)
        {
            renderer.sortingOrder = 200;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            // 검은 사각형을 만드는 사용자 지정 머티리얼 대신
            // 기본 스프라이트 셰이더를 사용해 표시를 복구
            Shader shader = Shader.Find("Sprites/Default");

            if (shader != null)
            {
                Material material = new Material(shader);
                material.color = Color.white;
                renderer.material = material;
            }
            else
            {
                Debug.LogError(
                    "Sprites/Default 셰이더를 찾을 수 없습니다."
                );
            }
        }

        sparkle.Play(true);

        Debug.Log("정화 완료 반짝이 효과 실행!");
    }

    void UpdateUI(float percent)
    {
        int p = Mathf.RoundToInt(percent * 100f);

        if (percentText != null)
            percentText.text = "정화도 " + p + "%";

        if (percentSlider != null)
            percentSlider.value = percent;
    }

    void SetAlpha(SpriteRenderer sr, float a)
    {
        if (sr == null)
            return;

        var c = sr.color;
        c.a = a;
        sr.color = c;
    }
}

// 컴파일용 더미
class Physics2DRaycasterMarker : MonoBehaviour { }
