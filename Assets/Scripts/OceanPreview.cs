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

    [Header("전체 화면 맞춤")]
    [SerializeField] bool autoFit = true;          // 바다 전체가 보이게 자동 조절
    [SerializeField] float padding = 1.0f;         // 1.0 = 딱 맞게, 1.1 = 10% 여유
    [SerializeField] float manualSizeMultiplier = 1f; // 결과가 마음에 안 들 때 확대/축소 (작을수록 확대)

    [Header("연결")]
    [SerializeField] Camera mainCamera;
    [SerializeField] Camera previewCamera;
    [SerializeField] RawImage previewImage;

    RenderTexture rt;
    int layer;
    int selectedStage = -1;
    string loadedScene;
    Coroutine loadRoutine;
    bool initialized = false;

    void Start()
    {
        Init();
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

    // 스테이지가 바뀔 때 호출 (stageSelectUI에서 호출)
    public void SelectStage(int index)
    {
        Init();
        if (!initialized) return;
        if (index < 0 || index >= stageScenes.Length) return;
        if (index == selectedStage) return;

        selectedStage = index;
        if (loadRoutine != null) StopCoroutine(loadRoutine);
        loadRoutine = StartCoroutine(SwitchPreview(stageScenes[index]));
    }

    IEnumerator SwitchPreview(string sceneName)
    {
        // 이전 미리보기 씬 언로드
        if (!string.IsNullOrEmpty(loadedScene))
        {
            var scene = SceneManager.GetSceneByName(loadedScene);
            if (scene.isLoaded)
                yield return SceneManager.UnloadSceneAsync(scene);
            loadedScene = null;
        }

        // 새 씬 Additive 로드
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        loadedScene = sceneName;
        Scene oceanScene = SceneManager.GetSceneByName(sceneName);

        // 레이어 지정, 플레이어 숨김, 카메라/UI/오디오 끄기
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
        }

        // 바다 씬의 Start()들이 실행될 시간을 한 프레임 준다
        yield return null;

        // 기본값: 바다 카메라 설정 복사
        if (oceanCam != null)
        {
            previewCamera.transform.position = oceanCam.transform.position;
            previewCamera.orthographicSize = oceanCam.orthographicSize;
            previewCamera.backgroundColor = oceanCam.backgroundColor;
        }

        // 바다 전체가 보이도록 카메라 맞춤
        if (autoFit)
            FitCameraToScene(oceanScene);
    }

    void FitCameraToScene(Scene scene)
    {
        bool hasBounds = false;
        Bounds bounds = new Bounds();

        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var sr in root.GetComponentsInChildren<SpriteRenderer>(false))
            {
                if (!sr.enabled) continue;              // 숨긴 플레이어 등 제외
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

        float sizeByHeight = bounds.size.y / 2f;
        float sizeByWidth = bounds.size.x / (2f * aspect);
        float size = Mathf.Max(sizeByHeight, sizeByWidth) * padding * manualSizeMultiplier;

        float z = previewCamera.transform.position.z;
        previewCamera.transform.position = new Vector3(bounds.center.x, bounds.center.y, z);
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

    // 미리보기 끄기 (입장하거나 패널이 닫힐 때 호출)
    public void ClosePreview()
    {
        if (loadRoutine != null) StopCoroutine(loadRoutine);
        selectedStage = -1;

        if (!string.IsNullOrEmpty(loadedScene))
        {
            var scene = SceneManager.GetSceneByName(loadedScene);
            if (scene.isLoaded)
                SceneManager.UnloadSceneAsync(scene);
            loadedScene = null;
        }
    }

    void OnDestroy()
    {
        if (rt != null) rt.Release();
    }
}