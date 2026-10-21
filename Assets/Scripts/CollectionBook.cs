using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CollectionBook : MonoBehaviour
{
    [Header("연결")]
    public GameObject bookPanel;          // CollectionBookPanel
    public Transform gridContainer;       // GridContainer
    public GameObject entryPrefab;        // BookEntry 프리팹
    public Button openButton;             // BookButton
    public Button closeButton;            // CloseButton

    [Header("페이드 연동 (이 화면들이 덮이면 도감도 같이 사라짐)")]
    public Image fadeImage;               // SceneFader의 Fade Image
    public Image transitionImage;         // SceneFader의 Transition Image
    public GameObject blackScreenPanel;   // Canvas 안의 BlackScreenPanel (작업장 페이드)

    [Header("설정")]
    public string lockedName = "?";       // 못 얻은 쓰레기 칸에 표시할 글자

    private class EntryUI
    {
        public TrashEntry data;
        public Image icon;
        public TextMeshProUGUI nameText;
    }

    private List<EntryUI> entries = new List<EntryUI>();
    private CanvasGroup group;

    private Image blackImage;
    private CanvasGroup blackGroup;

    void Awake()
    {
        // 도감 전체를 한 번에 투명하게 하기 위한 CanvasGroup
        group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();

        if (blackScreenPanel != null)
        {
            blackImage = blackScreenPanel.GetComponent<Image>();
            blackGroup = blackScreenPanel.GetComponent<CanvasGroup>();
        }
    }

    void Start()
    {
        Build();

        if (openButton != null) openButton.onClick.AddListener(Toggle);
        if (closeButton != null) closeButton.onClick.AddListener(Close);

        if (bookPanel != null) bookPanel.SetActive(false);
    }

    void OnEnable()
    {
        TrashCollection.OnChanged += Refresh;
    }

    void OnDisable()
    {
        TrashCollection.OnChanged -= Refresh;
    }

    void Update()
    {
        // 페이드 화면이 진해질수록 도감도 같이 흐려짐
        float a = 1f;

        if (fadeImage != null && fadeImage.gameObject.activeInHierarchy)
            a = Mathf.Min(a, 1f - fadeImage.color.a);

        if (transitionImage != null && transitionImage.gameObject.activeInHierarchy)
            a = Mathf.Min(a, 1f - transitionImage.color.a);

        // 작업장 페이드용 검은 패널
        if (blackScreenPanel != null && blackScreenPanel.activeInHierarchy)
        {
            float cover = 1f;
            if (blackGroup != null) cover *= blackGroup.alpha;
            if (blackImage != null) cover *= blackImage.color.a;
            a = Mathf.Min(a, 1f - cover);
        }

        group.alpha = a;
        group.interactable = a > 0.99f;
        group.blocksRaycasts = a > 0.99f;
    }

    // TrashDatabase의 항목 수만큼 칸 만들기
    void Build()
    {
        TrashDatabase db = TrashDatabase.Instance;
        if (db == null || gridContainer == null || entryPrefab == null)
        {
            Debug.LogWarning("CollectionBook: TrashDatabase나 연결 칸이 비어 있습니다.");
            return;
        }

        // 미리 놓아둔 칸이 남아 있으면 정리
        foreach (Transform child in gridContainer)
            Destroy(child.gameObject);

        entries.Clear();

        foreach (TrashEntry data in db.entries)
        {
            GameObject go = Instantiate(entryPrefab, gridContainer);

            EntryUI e = new EntryUI();
            e.data = data;

            Transform iconT = go.transform.Find("Icon");
            Transform nameT = go.transform.Find("NameText");
            if (iconT != null) e.icon = iconT.GetComponent<Image>();
            if (nameT != null) e.nameText = nameT.GetComponent<TextMeshProUGUI>();

            entries.Add(e);
        }

        Refresh();
    }

    // 얻은 쓰레기는 그림과 이름, 못 얻은 쓰레기는 물음표만
    public void Refresh()
    {
        foreach (EntryUI e in entries)
        {
            bool has = TrashCollection.Has(e.data.itemName);

            if (e.icon != null)
            {
                e.icon.sprite = e.data.icon;
                e.icon.preserveAspect = true;
                e.icon.color = Color.white;
                e.icon.enabled = has && e.data.icon != null;   // 못 얻었으면 그림 숨김
            }

            if (e.nameText != null)
                e.nameText.text = has ? e.data.itemName : lockedName;
        }
    }

    public void Toggle()
    {
        if (bookPanel == null) return;
        if (bookPanel.activeSelf) Close();
        else Open();
    }

    public void Open()
    {
        if (bookPanel == null) return;
        Refresh();
        bookPanel.SetActive(true);
    }

    public void Close()
    {
        if (bookPanel != null) bookPanel.SetActive(false);
    }
}