using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OceanPreview : MonoBehaviour
{
    [Header("스테이지 씬 (0=연안, 1=중층, 2=심해)")]
    [SerializeField] string[] stageScenes = { "GameScene_Team", "GameScene_Mid", "GameScene_Deep" };

    [Header("설정")]
    [SerializeField] string previewLayerName = "OceanPreview";
    [SerializeField] string playerTag = "Player";
    [SerializeField] Vector2Int textureSize = new Vector2Int(1280, 720);

    [Header("화면 맞춤 (칸을 꽉 채우고 긴 쪽은 잘라냄)")]
    [SerializeField] bool autoFit = true;
    [Range(0f, 1f)]
    [SerializeField] float focusX = 0.5f;   // 0=왼쪽, 0.5=가운데, 1=오른쪽
    [Range(0f, 1f)]
    [SerializeField] float focusY = 0.5f;   // 0=아래, 0.5=가운데, 1=위
    [SerializeField] float zoom = 1f;       // 1=칸 꽉 채움, 1보다 크면 확대, 작으면 축소

    [Header("연결")]
    [SerializeField] Camera mainCamera;
    [SerializeField] Camera previewCamera;
    [SerializeField] RawImage previewImage;

    RenderTexture rt;
    int layer;
    bool initialized = false;

    // 보여 주고 싶은 스테이지 (-1이면 미리보기 끔)
    int wantedStage = -1;
    string loadedScene;
    bool working = false;

    // 미리보기 씬이 바꿔 놓는 체력 값을 되돌리기 위한 백업
    bool vitalsBackedUp = false;
    float backupHealth;
    float backupMaxHealth;

    void Start()
    {
        Init();
    }

    void OnDisable()
    {
        // 오브젝트가 꺼지면 코루틴도 멈추므로 작업 중 표시를 풀어 둠
        working = false;
    }

    void Init()
    {
        if (initialized) return;

        layer = LayerMask.NameToLayer(previewLayerName);
        if (layer < 0)
        {
            Debug.LogError($"'{previewLayerName}' 레이어를 먼저 만들어주세요!");
            return;
        }

        rt = new RenderTexture(textureSize.x, textureSize.y, 16);
        previewCamera.orthographic = true;
        previewCamera.targetTexture = rt;
        previewCamera.cullingMask = 1 << layer;
        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.cullingMask &= ~(1 << layer);
        previewImage.texture = rt;

        initialized = true;
    }

    // ───── 체력 백업/복원 ─────
    void BackupVitals()
    {
        if (vitalsBackedUp) return;

        backupHealth = PlayerVitals.savedHealth;
        backupMaxHealth = PlayerVitals.maxHealth;
        vitalsBackedUp = true;
    }

    void RestoreVitals()
    {
        if (!vitalsBackedUp) return;

        PlayerVitals.savedHealth = backupHealth;
        PlayerVitals.maxHealth = backupMaxHealth;
    }

    // 스테이지가 바뀔 때 호출 (stageSelectUI에서 호출)
    public void SelectStage(int index)
    {
        Init();
        if (!initialized) return;
        if (index < 0 || index >= stageScenes.Length) return;
        if (index == wantedStage) return;

        BackupVitals();
        wantedStage = index;

        // 이미 작업 중이면 반복문이 새 목표를 알아서 처리함
        if (!working)
            StartCoroutine(PreviewWorker());
    }

    // 미리보기 끄기 (입장하거나 패널이 닫힐 때 호출)
    public void ClosePreview()
    {
        wantedStage = -1;

        if (!working && !string.IsNullOrEmpty(loadedScene) && isActiveAndEnabled)
            StartCoroutine(PreviewWorker());
    }

    // 마지막으로 선택된 스테이지만 순서대로 처리 (중간에 끊지 않음)
    IEnumerator PreviewWorker()
    {
        working = true;

        while (true)
        {
            int target = wantedStage;
            string targetScene =
                (target >= 0 && target < stageScenes.Length) ? stageScenes[target] : null;

            // 원하는 상태가 이미 만들어져 있으면 끝
            if (loadedScene == targetScene) break;

            // 1) 떠 있는 미리보기 씬 내리기
            if (!string.IsNullOrEmpty(loadedScene))
            {
                Scene old = SceneManager.GetSceneByName(loadedScene);
                if (old.isLoaded)
                    yield return SceneManager.UnloadSceneAsync(old);

                loadedScene = null;
                RestoreVitals();   // 미리보기 씬이 바꾼 체력을 원래대로
            }

            // 내리는 동안 목표가 바뀌었으면 처음부터 다시 판단
            if (wantedStage != target) continue;
            if (targetScene == null) break;

            // 2) 새 씬 Additive 로드
            yield return SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);
            loadedScene = targetScene;

            Scene oceanScene = SceneManager.GetSceneByName(targetScene);
            Camera oceanCam = PreparePreviewScene(oceanScene);

            // 바다 씬의 Start()들이 실행될 시간을 한 프레임 준다
            yield return null;

            // 불러오는 동안 목표가 바뀌었으면 이 씬은 다음 반복에서 내려감
            if (wantedStage != target) continue;

            // 기본값: 바다 카메라 설정 복사
            if (oceanCam != null)
            {
                previewCamera.transform.position = oceanCam.transform.position;
                previewCamera.orthographicSize = oceanCam.orthographicSize;
                previewCamera.backgroundColor = oceanCam.backgroundColor;
            }

            if (autoFit)
                FitCameraToScene(oceanScene);
        }

        // 미리보기를 완전히 껐으면 체력을 원래대로 돌려 두고 백업 해제
        if (wantedStage < 0)
        {
            RestoreVitals();
            vitalsBackedUp = false;
        }

        working = false;
    }

    // 레이어 지정, 플레이어 숨김, 카메라/UI/오디오/사망 처리 끄기. 바다 카메라를 돌려줌
    Camera PreparePreviewScene(Scene oceanScene)
    {
        Camera oceanCam = null;

        foreach (var root in oceanScene.GetRootGameObjects())
        {
            SetLayerExceptPlayer(root.transform, layer);

            foreach (var cam in root.GetComponentsInChildren<Camera>(true))
            {
                if (oceanCam == null) oceanCam = cam;
                cam.enabled = false;
            }
            foreach (var al in root.GetComponentsInChildren<AudioListener>(true))
                al.enabled = false;
            foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
                canvas.enabled = false;
            foreach (var es in root.GetComponentsInChildren<UnityEngine.EventSystems.EventSystem>(true))
                es.gameObject.SetActive(false);

            // 미리보기에서는 체크포인트 저장, 시간 정지, 사망 연출이 돌지 않게 끔
            foreach (var gos in root.GetComponentsInChildren<GameOverByStatus>(true))
                gos.enabled = false;
        }

        return oceanCam;
    }

    void FitCameraToScene(Scene scene)
    {
        bool hasBounds = false;
        Bounds bounds = new Bounds();

        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var sr in root.GetComponentsInChildren<SpriteRenderer>(false))
            {
                if (!sr.enabled) continue;   // 숨긴 플레이어 제외
                if (sr.sprite == null) continue;

                if (!hasBounds)
                {
                    bounds = sr.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(sr.bounds);
                }
            }
        }

        if (!hasBounds)
        {
            Debug.LogWarning("미리보기: 맞출 스프라이트를 찾지 못했어요.");
            return;
        }

        float aspect = (float)textureSize.x / textureSize.y;

        // cover 방식: 두 값 중 작은 쪽을 써서 칸을 꽉 채우고 긴 쪽은 잘라냄
        float sizeByHeight = bounds.size.y / 2f;
        float sizeByWidth = bounds.size.x / (2f * aspect);
        float size = Mathf.Min(sizeByHeight, sizeByWidth) / Mathf.Max(0.01f, zoom);

        float halfH = size;
        float halfW = size * aspect;

        // 카메라 중심 위치: 잘리는 쪽에서만 focus 값으로 이동
        float x = bounds.center.x;
        float y = bounds.center.y;

        float minX = bounds.min.x + halfW;
        float maxX = bounds.max.x - halfW;
        if (maxX > minX) x = Mathf.Lerp(minX, maxX, focusX);

        float minY = bounds.min.y + halfH;
        float maxY = bounds.max.y - halfH;
        if (maxY > minY) y = Mathf.Lerp(minY, maxY, focusY);

        float z = previewCamera.transform.position.z;
        previewCamera.transform.position = new Vector3(x, y, z);
        previewCamera.orthographicSize = size;
    }

    void SetLayerExceptPlayer(Transform t, int layer)
    {
        if (t.CompareTag(playerTag))
        {
            foreach (var sr in t.GetComponentsInChildren<SpriteRenderer>(true))
                sr.enabled = false;
            return;
        }

        t.gameObject.layer = layer;
        foreach (Transform child in t)
            SetLayerExceptPlayer(child, layer);
    }

    void OnDestroy()
    {
        if (rt != null) rt.Release();
    }
}