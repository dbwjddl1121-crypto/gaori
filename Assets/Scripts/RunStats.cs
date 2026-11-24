using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public static class RunStats
{
    const string SaveKey = "SavedTrashByScene";

    // 이번 입장(바다에 들어온 뒤)에서 한 기록 → 사망 창에 표시
    // 값이 바뀔 때마다 정화율 저장도 같이 갱신됨 (기존처럼 RunStats.trashCount++ 로 쓰면 돼요)
    private static int _trashCount;
    public static int trashCount
    {
        get { return _trashCount; }
        set
        {
            _trashCount = value;
            LiveSave();
        }
    }

    public static int fishCaught;
    public static int cashEarned;

    // 이 바다에 있는 전체 쓰레기 수 → 정화율 계산용
    public static int totalTrash;

    // 이전 탐험들에서 이미 수거해서 저장해 둔 개수 (이 씬 기준)
    private static int previousTrash;

    // 지금 어느 바다를 기록 중인지
    private static bool tracking = false;
    private static string trackedScene = "";

    // 씬 이름별로 저장된 수거 개수 (작업장에 나갔다 와도, 게임을 껐다 켜도 유지)
    private static Dictionary<string, int> savedTrash = new Dictionary<string, int>();
    private static bool loaded = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        _trashCount = 0;
        fishCaught = 0;
        cashEarned = 0;
        totalTrash = 0;
        previousTrash = 0;
        tracking = false;
        trackedScene = "";
        savedTrash = new Dictionary<string, int>();
        loaded = false;
    }

    // 이전 탐험 + 이번 탐험을 합친 수거 개수
    public static int TotalCollected
    {
        get { return previousTrash + _trashCount; }
    }

    // 정화율 (0 ~ 100). 쓰레기를 다 주우면 100%
    public static float PurificationPercent
    {
        get
        {
            if (totalTrash <= 0) return 0f;
            return Mathf.Clamp01((float)TotalCollected / totalTrash) * 100f;
        }
    }

    // ───────── 저장 / 불러오기 ─────────

    static void EnsureLoaded()
    {
        if (loaded) return;
        loaded = true;

        savedTrash.Clear();

        string raw = PlayerPrefs.GetString(SaveKey, "");
        if (string.IsNullOrEmpty(raw)) return;

        foreach (string pair in raw.Split(';'))
        {
            int eq = pair.LastIndexOf('=');
            if (eq <= 0) continue;

            string scene = pair.Substring(0, eq);
            int count;

            if (int.TryParse(pair.Substring(eq + 1), out count))
                savedTrash[scene] = count;
        }
    }

    static void SaveToPrefs()
    {
        List<string> parts = new List<string>();

        foreach (var kv in savedTrash)
            parts.Add(kv.Key + "=" + kv.Value);

        PlayerPrefs.SetString(SaveKey, string.Join(";", parts.ToArray()));
        PlayerPrefs.Save();
    }

    // 쓰레기를 주울 때마다 지금까지의 개수를 바로 저장 (바다에서 게임을 꺼도 유지)
    static void LiveSave()
    {
        if (!tracking) return;

        EnsureLoaded();

        int total = previousTrash + _trashCount;

        if (totalTrash > 0)
            total = Mathf.Min(total, totalTrash);

        savedTrash[trackedScene] = total;
        SaveToPrefs();
    }

    // ───────── 기존 기능 ─────────

    // 바다 씬이 시작될 때 호출: 전체 쓰레기 수를 정하고, 저장해 둔 수거 개수를 불러옴
    public static void SetTotalTrash(int total)
    {
        EnsureLoaded();

        totalTrash = total;
        trackedScene = SceneManager.GetActiveScene().name;

        int saved;
        previousTrash = savedTrash.TryGetValue(trackedScene, out saved) ? saved : 0;

        tracking = true;
    }

    // 작업장으로 나갈 때 호출: 지금까지의 수거 개수를 이 씬에 저장
    public static void CommitRun()
    {
        if (!tracking) return;

        LiveSave();
    }

    // 새 게임 시작용: 모든 씬의 정화율 저장을 지움 (저장된 기록도 함께 지움)
    public static void ResetAllProgress()
    {
        EnsureLoaded();
        savedTrash.Clear();

        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();

        StartNewRun();
    }

    public static void StartNewRun()
    {
        _trashCount = 0;
        fishCaught = 0;
        cashEarned = 0;
        previousTrash = 0;
        tracking = false;
    }

    public static void Reset()
    {
        StartNewRun();
    }

    // 기존 코드 호환용
    public static void SaveCheckpoint() { }

    // 죽었을 때: 이번 탐험에서 모은 개수를 버리고, 입장했을 때의 정화율로 되돌림
    public static void RestoreCheckpoint()
    {
        if (tracking)
        {
            EnsureLoaded();
            savedTrash[trackedScene] = previousTrash;
            SaveToPrefs();
        }

        StartNewRun();
    }
}