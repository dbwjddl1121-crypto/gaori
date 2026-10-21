using System;
using System.Collections.Generic;
using UnityEngine;

// 도감에서 "얻은 쓰레기"를 기록하고 저장
public static class TrashCollection
{
    private const string Key = "TrashBook_Collected";
    private static HashSet<string> collected;

    // 새로 얻어서 기록이 바뀔 때 호출됨
    public static event Action OnChanged;

    static void Load()
    {
        if (collected != null) return;

        collected = new HashSet<string>();
        string saved = PlayerPrefs.GetString(Key, "");
        if (string.IsNullOrEmpty(saved)) return;

        foreach (string n in saved.Split('|'))
            if (!string.IsNullOrEmpty(n)) collected.Add(n);
    }

    public static bool Has(string itemName)
    {
        Load();
        return !string.IsNullOrEmpty(itemName) && collected.Contains(itemName);
    }

    // 아이템을 얻을 때 호출 (이미 얻은 것이거나 도감에 없는 이름이면 무시)
    public static void Record(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return;

        TrashDatabase db = TrashDatabase.Instance;
        if (db != null && db.Find(itemName) == null) return;

        Load();
        if (!collected.Add(itemName)) return;

        PlayerPrefs.SetString(Key, string.Join("|", collected));
        PlayerPrefs.Save();
        OnChanged?.Invoke();
    }

    // 테스트나 새 게임 시작용 초기화
    public static void ResetAll()
    {
        collected = new HashSet<string>();
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
        OnChanged?.Invoke();
    }
}