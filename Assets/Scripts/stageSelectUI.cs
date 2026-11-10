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

        Localization.OnLanguageChanged += Refresh;
    }

    void OnDisable()
    {
        if (bookButton != null)
            bookButton.SetActive(true);

        if (oceanPreview != null)
            oceanPreview.ClosePreview();

        Localization.OnLanguageChanged -= Refresh;
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

        // Localization.Pick 순서: 한국어, 영어, 베트남어
        if (stageNameText != null)
            stageNameText.text = Localization.Pick(
                data.stageLabel,
                data.stageLabelEn,
                ViLabel(data));

        if (stageTitleText != null)
            stageTitleText.text = Localization.Pick(
                data.stageTitle,
                data.stageTitleEn,
                ViTitle(data));

        if (stageDescText != null)
            stageDescText.text = Localization.Pick(
                data.stageDescription,
                data.stageDescriptionEn,
                ViDescription(data));

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

    // 스테이지 이름 (Inspector에 적은 게 있으면 그걸 우선 사용)
    string ViLabel(stageData d)
    {
        if (!string.IsNullOrEmpty(d.stageLabelVi)) return d.stageLabelVi;
        return "Màn " + (d.stageIndex + 1);
    }

    // 스테이지 제목
    string ViTitle(stageData d)
    {
        if (!string.IsNullOrEmpty(d.stageTitleVi)) return d.stageTitleVi;

        switch (d.stageIndex)
        {
            case 0: return "Vùng ven biển";   // 연안
            case 1: return "Tầng nước giữa";  // 중층
            case 2: return "Biển sâu";        // 심해
        }
        return d.stageTitleEn;
    }

    // 스테이지 설명 (아직 베트남어 번역을 안 넣어서 영어로 대신 보여줘요)
    string ViDescription(stageData d)
    {
        if (!string.IsNullOrEmpty(d.stageDescriptionVi)) return d.stageDescriptionVi;

        // 설명 번역을 받으면 여기에 case 0, 1, 2로 넣을 거예요
        return d.stageDescriptionEn;
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