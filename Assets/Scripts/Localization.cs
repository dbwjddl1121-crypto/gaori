using System;
using UnityEngine;

public enum Language
{
    Korean = 0,
    English = 1
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

    // 한국어 ↔ English 전환
    public static void Toggle()
    {
        SetLanguage(current == Language.Korean ? Language.English : Language.Korean);
    }
}