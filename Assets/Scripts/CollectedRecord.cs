using System.Collections.Generic;

public static class CollectedRecord
{
    private static HashSet<string> collected = new HashSet<string>();
    private static HashSet<string> checkpoint = new HashSet<string>();

    public static void Add(string id)
    {
        collected.Add(id);
    }

    public static bool Has(string id)
    {
        return collected.Contains(id);
    }

    // 바다에 들어온 순간 호출: 현재 기록을 저장점으로 복사
    public static void SaveCheckpoint()
    {
        checkpoint = new HashSet<string>(collected);
    }

    // 죽었을 때 호출: 저장점 상태로 되돌리기
    public static void RestoreCheckpoint()
    {
        collected = new HashSet<string>(checkpoint);
    }

    // 완전히 처음부터 (타이틀로 갈 때)
    public static void ResetAll()
    {
        collected.Clear();
        checkpoint.Clear();
    }
}