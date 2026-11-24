using System.Collections.Generic;
using UnityEngine;

// 수거한 쓰레기 / 잡은 물고기 기록
// 게임을 완전히 껐다 켜도 유지됩니다 (PlayerPrefs에 저장)
public static class CollectedRecord
{
    const string SaveKey = "CollectedIDs";
    const string Separator = "\n";

    private static HashSet<string> collected = new HashSet<string>();
    private static HashSet<string> checkpoint = new HashSet<string>();
    private static bool loaded = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        collected = new HashSet<string>();
        checkpoint = new HashSet<string>();
        loaded = false;
    }

    // 처음 쓸 때 저장된 기록을 한 번만 불러옴
    static void EnsureLoaded()
    {
        if (loaded) return;
        loaded = true;

        collected.Clear();

        string raw = PlayerPrefs.GetString(SaveKey, "");
        if (string.IsNullOrEmpty(raw)) return;

        foreach (string id in raw.Split(new string[] { Separator }, System.StringSplitOptions.RemoveEmptyEntries))
        {
            collected.Add(id);
        }
    }

    static void Save()
    {
        PlayerPrefs.SetString(SaveKey, string.Join(Separator, collected));
        PlayerPrefs.Save();
    }

    public static void Add(string id)
    {
        EnsureLoaded();

        if (collected.Add(id))
            Save();
    }

    public static bool Has(string id)
    {
        EnsureLoaded();
        return collected.Contains(id);
    }

    // 바다에 들어온 순간 호출: 현재 기록을 저장점으로 복사
    public static void SaveCheckpoint()
    {
        EnsureLoaded();
        checkpoint = new HashSet<string>(collected);
    }

    // 죽었을 때 호출: 저장점 상태로 되돌리기
    public static void RestoreCheckpoint()
    {
        EnsureLoaded();
        collected = new HashSet<string>(checkpoint);
        Save();
    }

    // 완전히 처음부터 (저장된 기록도 함께 지움)
    public static void ResetAll()
    {
        EnsureLoaded();
        collected.Clear();
        checkpoint.Clear();

        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }
}