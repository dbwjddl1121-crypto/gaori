using UnityEngine;

// 스테이지 잠금 상태 (0=연안, 1=중층, 2=심해)
// 별도 저장 없이 StageProgress의 "정화 완료" 기록을 그대로 사용
public static class StageLock
{
    public const int TotalStages = 3;

    // 연안은 항상 열림, 나머지는 바로 앞 스테이지를 정화했을 때 열림
    public static bool IsUnlocked(int stageIndex)
    {
        if (stageIndex <= 0) return true;
        return StageProgress.IsCleaned(stageIndex - 1);
    }

    public static void MarkCleared(int stageIndex)
    {
        StageProgress.SetCleaned(stageIndex);
    }

    // 테스트용 (F5)
    public static void UnlockAll()
    {
        for (int i = 0; i < TotalStages - 1; i++)
            StageProgress.SetCleaned(i);
    }

    // 테스트용 (F6)
    public static void ResetAll()
    {
        StageProgress.ResetAll();
    }
}