using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public static class RunStats
{
    // 이번 입장(바다에 들어온 뒤)에서 한 기록 → 사망 창에 표시
    public static int trashCount;
    public static int fishCaught;
    public static int cashEarned;

    // 이 바다에 있는 전체 쓰레기 수 → 정화율 계산용
    public static int totalTrash;

    // 이전 탐험들에서 이미 수거해서 저장해 둔 개수 (이 씬 기준)
    private static int previousTrash;

    // 씬 이름별로 저장된 수거 개수 (작업장에 나갔다 와도 유지)
    private static Dictionary<string, int> savedTrash = new Dictionary<string, int>();

    // 이전 탐험 + 이번 탐험을 합친 수거 개수
    public static int TotalCollected
    {
        get { return previousTrash + trashCount; }
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

    // 바다 씬이 시작될 때 호출: 전체 쓰레기 수를 정하고, 저장해 둔 수거 개수를 불러옴
    public static void SetTotalTrash(int total)
    {
        totalTrash = total;

        string sceneName = SceneManager.GetActiveScene().name;
        int saved;
        previousTrash = savedTrash.TryGetValue(sceneName, out saved) ? saved : 0;
    }

    // 작업장으로 나갈 때 호출: 지금까지의 수거 개수를 이 씬에 저장
    public static void CommitRun()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        int total = previousTrash + trashCount;

        if (totalTrash > 0)
            total = Mathf.Min(total, totalTrash);

        savedTrash[sceneName] = total;
    }

    // 새 게임 시작용: 모든 씬의 정화율 저장을 지움
    public static void ResetAllProgress()
    {
        savedTrash.Clear();
        previousTrash = 0;
        StartNewRun();
    }

    public static void StartNewRun()
    {
        trashCount = 0;
        fishCaught = 0;
        cashEarned = 0;
        previousTrash = 0;
    }

    public static void Reset()
    {
        StartNewRun();
    }

    // 기존 코드 호환용 (비워둬도 됨)
    public static void SaveCheckpoint() { }
    public static void RestoreCheckpoint() { StartNewRun(); }
}