using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class stageSelectUI : MonoBehaviour
{
    [Header("스테이지 데이터 (3개)")]
    public stageData[] stages;

    [Header("UI 연결")]
    public TMP_Text stageNameText;
    public TMP_Text stageTitleText;
    public Image stagePreview;
    public Button prevButton;
    public Button nextButton;
    public Button enterButton;

    // 선택된 스테이지 번호 (다른 스크립트에서 읽을 수 있어요)
    public static int selectedStageIndex = 0;

    int currentIndex = 0;

    void Start()
    {
        prevButton.onClick.AddListener(Prev);
        nextButton.onClick.AddListener(Next);
        enterButton.onClick.AddListener(EnterStage);
        Refresh();
    }

    void OnEnable()
    {
        currentIndex = 0;
        if (stageNameText != null) Refresh();
    }

    void Prev()
    {
        currentIndex = Mathf.Max(0, currentIndex - 1);
        Refresh();
    }

    void Next()
    {
        currentIndex = Mathf.Min(stages.Length - 1, currentIndex + 1);
        Refresh();
    }

    void Refresh()
    {
        stageData data = stages[currentIndex];
        stageNameText.text = data.stageLabel;
        stageTitleText.text = data.stageTitle;

        if (data.previewImage != null)
            stagePreview.sprite = data.previewImage;

        prevButton.interactable = currentIndex > 0;
        nextButton.interactable = currentIndex < stages.Length - 1;
    }

    void EnterStage()
    {
        selectedStageIndex = stages[currentIndex].stageIndex;
        Debug.Log("선택한 스테이지: " + selectedStageIndex);
        // 바다 화면으로 이동하는 코드는 다음 단계에서 추가해요
    }
}