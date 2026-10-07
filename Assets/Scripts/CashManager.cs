using UnityEngine;
using System;

public class CashManager : MonoBehaviour
{
    public static CashManager Instance;

    public int startCash = 100;   // 새 게임 시작 금액
    public int cash;                  // 현재 금액

    public event Action<int> OnCashChanged;

    const string SaveKey = "PlayerCash";

    private int checkpointCash = -1;  // 바다에 들어온 순간의 금액

    void Awake()
    {
        // 이미 다른 CashManager가 있으면 나는 삭제 (중복 방지)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    void Start()
    {
        OnCashChanged?.Invoke(cash);
    }

    void Load()
    {
        // 저장된 값이 있으면 불러오고, 없으면 시작 금액 사용
        if (PlayerPrefs.HasKey(SaveKey))
            cash = PlayerPrefs.GetInt(SaveKey);
        else
            cash = startCash;
    }

    void Save()
    {
        PlayerPrefs.SetInt(SaveKey, cash);
        PlayerPrefs.Save();
    }

    public bool CanAfford(int amount)
    {
        return cash >= amount;
    }

    public bool SpendCash(int amount)
    {
        if (cash < amount) return false;

        cash -= amount;
        Save();
        OnCashChanged?.Invoke(cash);
        return true;
    }

    public void AddCash(int amount)
    {
        cash += amount;
        Save();
        OnCashChanged?.Invoke(cash);
    }

    // 바다에 들어온 순간 호출: 현재 금액을 저장점으로 기록
    public void SaveCheckpoint()
    {
        checkpointCash = cash;
    }

    // 죽고 다시 시작할 때 호출: 저장점 금액으로 되돌리기
    public void RestoreCheckpoint()
    {
        if (checkpointCash < 0) checkpointCash = startCash;
        cash = checkpointCash;
        Save();
        OnCashChanged?.Invoke(cash);
    }

    // 테스트용: 새 게임처럼 초기화
    [ContextMenu("Reset Save")]
    public void ResetSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        cash = startCash;
        checkpointCash = startCash;
        OnCashChanged?.Invoke(cash);
    }

    // 타이틀로 돌아갈 때 호출: 시작 금액으로 완전 초기화
    public void ResetCash()
    {
        cash = startCash;
        checkpointCash = startCash;
        Save();
        OnCashChanged?.Invoke(cash);
    }
}