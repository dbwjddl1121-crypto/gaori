using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LocalizedText : MonoBehaviour
{
    [TextArea(1, 6)]
    public string korean;

    [TextArea(1, 6)]
    public string english;

    [TextArea(1, 6)]
    public string vietnamese;

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

        string value;
        switch (Localization.Current)
        {
            case Language.English:
                value = english;
                break;
            case Language.Vietnamese:
                // 베트남어를 아직 안 적었으면 영어로 대신 보여줌
                value = string.IsNullOrEmpty(vietnamese) ? english : vietnamese;
                break;
            default:
                value = korean;
                break;
        }

        if (tmpText != null) tmpText.text = value;
        else if (legacyText != null) legacyText.text = value;
    }
}