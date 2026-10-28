
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class stageSelectUI : MonoBehaviour
{
    [Header("스테이지 데이터 (3개)")]
    public stageData[] stages;

    [Header("스테이지별 씬 이름")]
    public string[] sceneNames =
    {
        "GameScene_Team",
        "GameScene_Mid",
        "GameScene_Deep"
    };

    [Header("UI 연결")]
    public TMP_Text stageNameText;
    public TMP_Text stageTitleText;
    public TMP_Text stageDescText;
    public Image stagePreview;

    public Button prevButton;
    public Button nextButton;
    public Button enterButton;
    public Button closeButton;

    [Header("UI 연동")]
    public GameObject bookButton;

    [Header("씬 전환")]
    public SceneFader sceneFader;

    [Header("바다 미리보기")]
    public OceanPreview oceanPreview;

    public static int selectedStageIndex = 0;

    private int currentIndex = 0;
    private bool initialized = false;

    void Awake()
    {
        if (sceneFader == null)
            sceneFader = FindFirstObjectByType<SceneFader>();
    }

    void Start()
    {
        Init();
        Refresh();
    }

    void OnEnable()
    {
        if (bookButton != null)
            bookButton.SetActive(false);

        // 정화 후 돌아온 경우 해당 스테이지 선택
        currentIndex = 0;

        if (StageProgress.returnStage >= 0 &&
            stages != null &&
            stages.Length > 0)
        {
            currentIndex = Mathf.Clamp(
                StageProgress.returnStage,
                0,
                stages.Length - 1
            );
        }

        Init();
        Refresh();
    }

    void OnDisable()
    {
        if (bookButton != null)
            bookButton.SetActive(true);

        if (oceanPreview != null)
            oceanPreview.ClosePreview();
    }

    void Update()
    {
        if (BPressed())
            CloseSelect();
    }

    bool BPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null &&
               Keyboard.current.bKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.B);
#endif
    }

    void Init()
    {
        if (initialized)
            return;

        initialized = true;

        if (prevButton != null)
        {
            prevButton.onClick.RemoveListener(Prev);
            prevButton.onClick.AddListener(Prev);
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(Next);
            nextButton.onClick.AddListener(Next);
        }

        if (enterButton != null)
        {
            enterButton.onClick.RemoveListener(EnterStage);
            enterButton.onClick.AddListener(EnterStage);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseSelect);
            closeButton.onClick.AddListener(CloseSelect);
        }
    }

    void Prev()
    {
        if (stages == null || stages.Length == 0)
            return;

        currentIndex = Mathf.Max(0, currentIndex - 1);
        Refresh();
    }

    void Next()
    {
        if (stages == null || stages.Length == 0)
            return;

        currentIndex = Mathf.Min(
            stages.Length - 1,
            currentIndex + 1
        );

        Refresh();
    }

    void Refresh()
    {
        if (stages == null ||
            stages.Length == 0 ||
            currentIndex < 0 ||
            currentIndex >= stages.Length)
        {
            Debug.LogError("스테이지 데이터를 확인해 주세요.");
            return;
        }

        stageData data = stages[currentIndex];

        if (data == null)
        {
            Debug.LogError("선택한 스테이지 데이터가 비어 있습니다.");
            return;
        }

        if (stageNameText != null)
            stageNameText.text = data.stageLabel;

        if (stageTitleText != null)
            stageTitleText.text = data.stageTitle;

        if (stageDescText != null)
            stageDescText.text = data.stageDescription;

        if (stagePreview != null && data.previewImage != null)
            stagePreview.sprite = data.previewImage;

        if (prevButton != null)
            prevButton.interactable = currentIndex > 0;

        if (nextButton != null)
        {
            nextButton.interactable =
                currentIndex < stages.Length - 1;
        }

        UpdateOceanPreview();
    }

    void UpdateOceanPreview()
    {
        if (oceanPreview == null)
            return;

        if (stages == null ||
            currentIndex < 0 ||
            currentIndex >= stages.Length)
            return;

        oceanPreview.SelectStage(currentIndex);
    }

    void EnterStage()
    {
        if (stages == null ||
            currentIndex < 0 ||
            currentIndex >= stages.Length ||
            stages[currentIndex] == null)
        {
            Debug.LogError("선택한 스테이지 데이터가 없습니다.");
            return;
        }

        if (sceneFader == null)
        {
            sceneFader = FindFirstObjectByType<SceneFader>();
        }

        if (sceneFader == null)
        {
            Debug.LogError("SceneFader가 연결되지 않았습니다.");
            return;
        }

        if (sceneNames == null ||
            currentIndex >= sceneNames.Length ||
            string.IsNullOrEmpty(sceneNames[currentIndex]))
        {
            Debug.LogError("해당 스테이지의 씬 이름을 확인해 주세요.");
            return;
        }

        selectedStageIndex = stages[currentIndex].stageIndex;

        string targetScene = sceneNames[currentIndex];

        Debug.Log(
            "선택한 스테이지: " +
            stages[currentIndex].stageTitle +
            " / 이동할 씬: " +
            targetScene
        );

        if (oceanPreview != null)
            oceanPreview.ClosePreview();

        sceneFader.FadeToScene(targetScene);
    }

    void CloseSelect()
    {
        if (sceneFader == null)
            sceneFader = FindFirstObjectByType<SceneFader>();

        if (sceneFader == null)
        {
            Debug.LogError("SceneFader가 연결되지 않았습니다.");
            return;
        }

        if (oceanPreview != null)
            oceanPreview.ClosePreview();

        sceneFader.CloseStageSelect();
    }
}
