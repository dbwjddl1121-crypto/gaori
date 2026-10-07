public static class RunStats
{
    public static int trashCount;   // 수거한 쓰레기 수
    public static int fishCaught;   // 잡은 물고기 수
    public static int cashEarned;   // 이번 판에서 번 캐시

    public static void Reset()
    {
        trashCount = 0;
        fishCaught = 0;
        cashEarned = 0;
    }
}