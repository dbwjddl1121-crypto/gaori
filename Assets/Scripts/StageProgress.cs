using UnityEngine;

// 스테이지별 정화 여부를 저장/조회
public static class StageProgress
{
    // 정화 후 GameScene으로 돌아왔을 때 스테이지 선택창을 열기 위한 표시
    public static bool openStageSelect = false;
    public static int returnStage = -1;

    static string Key(int stageIndex) => "StageCleaned_" + stageIndex;

    public static bool IsCleaned(int stageIndex)
    {
        return PlayerPrefs.GetInt(Key(stageIndex), 0) == 1;
    }

    public static void SetCleaned(int stageIndex)
    {
        PlayerPrefs.SetInt(Key(stageIndex), 1);
        PlayerPrefs.Save();
    }

    // 테스트용: 모든 스테이지를 오염 상태로 되돌림
    public static void ResetAll()
    {
        for (int i = 0; i < 3; i++)
            PlayerPrefs.DeleteKey(Key(i));
        PlayerPrefs.Save();
    }
}