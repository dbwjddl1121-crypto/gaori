public static class RunStats
{
    // 이번 입장(바다에 들어온 뒤)에서 한 기록 → 사망 창에 표시
    public static int trashCount;
    public static int fishCaught;
    public static int cashEarned;

    public static void StartNewRun()
    {
        trashCount = 0;
        fishCaught = 0;
        cashEarned = 0;
    }

    public static void Reset()
    {
        StartNewRun();
    }

    // 기존 코드 호환용 (비워둬도 됨)
    public static void SaveCheckpoint() { }
    public static void RestoreCheckpoint() { StartNewRun(); }
}