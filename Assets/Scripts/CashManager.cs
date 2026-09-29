using UnityEngine;
using TMPro; // TextMeshPro 쓸 경우. Legacy Text면 using UnityEngine.UI;

public class CashManager : MonoBehaviour
{
    public static CashManager Instance;

    public int cash = 0;
    public TextMeshProUGUI coinText; // Legacy Text면 public Text coinText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
            // GameManager와 Canvas를 같은 부모로 묶거나,
            // Canvas도 따로 DontDestroyOnLoad 해줘야 함 (아래 참고)
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
