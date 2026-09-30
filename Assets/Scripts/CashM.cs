using UnityEngine;
using TMPro;

public class CashM : MonoBehaviour
{
    public static CashM Instance;

    public int cash = 0;
    public TextMeshProUGUI coinText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        UpdateUI();
    }

    public void AddCash(int amount)
    {
        cash += amount;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (coinText != null)
            coinText.text = cash.ToString();
    }
}