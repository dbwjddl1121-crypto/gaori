using UnityEngine;

// 배드엔딩 후 게임을 완전히 처음 상태로 되돌림
// (언어, 볼륨, 튜토리얼 본 기록은 건드리지 않음)
public static class GameReset
{
    public static void ResetAll()
    {
        // 정화 기록 + 스테이지 잠금 (같은 저장소)
        StageProgress.ResetAll();
        StageProgress.openStageSelect = false;
        StageProgress.returnStage = -1;

        // 도감
        TrashCollection.ResetAll();

        // 인벤토리
        InventoryStore.ResetAll();

        // 잡은 물고기, 수거 기록, 씬별 정화율
        CollectedRecord.ResetAll();
        RunStats.ResetAllProgress();

        // 캐시 (DontDestroyOnLoad라서 직접 되돌려야 함)
        if (CashManager.Instance != null)
            CashManager.Instance.ResetCash();

        // 체력 (-1 = 처음 입장 상태)
        PlayerVitals.savedHealth = -1f;

        Debug.Log("[GameReset] 전체 초기화 완료");
    }
}