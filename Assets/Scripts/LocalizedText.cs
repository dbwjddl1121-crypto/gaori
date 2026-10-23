using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LocalizedText : MonoBehaviour
{
    [TextArea(1, 6)]
    public string korean;

    [TextArea(1, 6)]
    public string english;

    private TMP_Text tmpText;
    private Text legacyText;

    void Awake()
    {
        FindText();
    }

    // 꺼져 있던 패널이 켜질 때도 현재 언어로 갱신됨
    void OnEnable()
    {
        Localization.OnLanguageChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        Localization.OnLanguageChanged -= Refresh;
    }

    void FindText()
    {
        if (tmpText == null) tmpText = GetComponent<TMP_Text>();
        if (tmpText == null && legacyText == null) legacyText = GetComponent<Text>();
    }

    void Refresh()
    {
        FindText();

        string value = Localization.Current == Language.Korean ? korean : english;

        if (tmpText != null) tmpText.text = value;
        else if (legacyText != null) legacyText.text = value;
    }
}