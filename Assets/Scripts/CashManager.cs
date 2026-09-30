using UnityEngine;
using System;

public class CashManager : MonoBehaviour
{
    public static CashManager Instance;

    public int startCash = 100;   // 새 게임 시작 금액
    public int cash;                  // 현재 금액

    public event Action<int> OnCashChanged;

    const string SaveKey = "PlayerCash";

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

    // 테스트용: 새 게임처럼 초기화
    [ContextMenu("Reset Save")]
    public void ResetSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        cash = startCash;
        OnCashChanged?.Invoke(cash);
    }
}