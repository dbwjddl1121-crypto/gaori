using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LanguageButton : MonoBehaviour
{
    [Header("버튼 글자")]
    public TMP_Text label;

    [Header("특정 언어 버튼으로 쓰려면 체크")]
    public bool selectDirectly = false;
    public Language targetLanguage = Language.Korean;

    void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            if (selectDirectly)
                button.onClick.AddListener(() => Localization.SetLanguage(targetLanguage));
            else
                button.onClick.AddListener(Localization.Toggle);
        }

        Localization.OnLanguageChanged += RefreshLabel;
        RefreshLabel();
    }

    void OnDestroy()
    {
        Localization.OnLanguageChanged -= RefreshLabel;
    }

    void RefreshLabel()
    {
        if (label == null) return;

        // 직접 선택 버튼은 항상 자기 언어 이름을 표시
        Language shown = selectDirectly ? targetLanguage : Localization.Current;

        switch (shown)
        {
            case Language.English:
                label.text = selectDirectly ? "English" : "Language: English";
                break;
            case Language.Vietnamese:
                label.text = selectDirectly ? "Tiếng Việt" : "Ngôn ngữ: Tiếng Việt";
                break;
            default:
                label.text = selectDirectly ? "한국어" : "언어: 한국어";
                break;
        }
    }
}