using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    public Image stagePreview;
    public Button prevButton;
    public Button nextButton;
    public Button enterButton;

    [Header("바다 미리보기")]
    public OceanPreview oceanPreview;

    [Header("UI 연동")]
    public GameObject bookButton;

    public static int selectedStageIndex = 0;

    private int currentIndex = 0;
    private SceneFader sceneFader;

    void Start()
    {
        sceneFader = FindFirstObjectByType<SceneFader>();

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

        Refresh();
    }

    void OnEnable()
    {
        // 북버튼 숨기기
        if (bookButton != null)
            bookButton.SetActive(false);

        // 정화 후 돌아온 경우 그 스테이지를 선택, 아니면 첫 번째
        currentIndex = 0;
        if (StageProgress.returnStage >= 0 && stages != null && stages.Length > 0)
            currentIndex = Mathf.Min(StageProgress.returnStage, stages.Length - 1);

        if (stages != null &&
            stages.Length > 0 &&
            stageNameText != null)
        {
            Refresh();
        }
    }

    void OnDisable()
    {
        // 북버튼 다시 보이기
        if (bookButton != null)
            bookButton.SetActive(true);

        if (oceanPreview != null)
            oceanPreview.ClosePreview();
    }

    void Prev()
    {
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
            currentIndex >= stages.Length)
        {
            Debug.LogError("스테이지 데이터가 연결되지 않았습니다.");
            return;
        }

        stageData data = stages[currentIndex];

        if (data == null)
            return;

        if (stageNameText != null)
            stageNameText.text = data.stageLabel;

        if (stageTitleText != null)
            stageTitleText.text = data.stageTitle;

        if (stagePreview != null && data.previewImage != null)
            stagePreview.sprite = data.previewImage;

        if (oceanPreview != null)
            oceanPreview.SelectStage(currentIndex);

        if (prevButton != null)
            prevButton.interactable = currentIndex > 0;

        if (nextButton != null)
        {
            nextButton.interactable =
                currentIndex < stages.Length - 1;
        }
    }

    void EnterStage()
    {
        if (stages == null ||
            currentIndex >= stages.Length ||
            stages[currentIndex] == null)
        {
            Debug.LogError("선택한 스테이지 데이터가 없습니다.");
            return;
        }

        if (sceneFader == null)
        {
            Debug.LogError("SceneFader를 찾을 수 없습니다.");
            return;
        }

        if (sceneNames == null ||
            currentIndex >= sceneNames.Length ||
            string.IsNullOrEmpty(sceneNames[currentIndex]))
        {
            Debug.LogError("해당 스테이지의 씬 이름을 설정해 주세요.");
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
}
