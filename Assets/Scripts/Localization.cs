using System;
using UnityEngine;

public enum Language
{
    Korean = 0,
    English = 1,
    Vietnamese = 2
}

public static class Localization
{
    const string LANGUAGE_KEY = "Language";

    static Language current = (Language)PlayerPrefs.GetInt(LANGUAGE_KEY, 0);

    // 언어가 바뀔 때마다 호출되는 이벤트
    public static event Action OnLanguageChanged;

    public static Language Current
    {
        get { return current; }
    }

    public static void SetLanguage(Language language)
    {
        if (current == language) return;

        current = language;
        PlayerPrefs.SetInt(LANGUAGE_KEY, (int)language);
        PlayerPrefs.Save();
        Debug.Log("언어 변경: " + language);

        if (OnLanguageChanged != null) OnLanguageChanged();
    }

    // 한국어 → English → Tiếng Việt → 한국어 순서로 전환
    public static void Toggle()
    {
        int next = ((int)current + 1) % Enum.GetValues(typeof(Language)).Length;
        SetLanguage((Language)next);
    }

    // 코드 안에서 문구를 고를 때 쓰는 도우미
    // 사용 예: Localization.Pick("상점이 너무 멀리있습니다.", "The shop is too far.", "Cửa hàng ở quá xa.")
    public static string Pick(string korean, string english, string vietnamese)
    {
        switch (current)
        {
            case Language.English: return english;
            case Language.Vietnamese: return vietnamese;
            default: return korean;
        }
    }
}