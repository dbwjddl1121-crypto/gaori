using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LanguageButton : MonoBehaviour
{
    [Header("버튼 글자")]
    public TMP_Text label;

    void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
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

        label.text = Localization.Current == Language.Korean
            ? "언어: 한국어"
            : "Language: English";
    }
}