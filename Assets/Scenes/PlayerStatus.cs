using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour
{
    public Slider oxygenBar;
    public Slider healthBar;

    public float maxOxygen = 30f;
    private float currentOxygen;

    public float maxHealth = 50f;
    private float currentHealth;

    [Header("체력 감소 설정 (산소가 0일 때만 적용)")]
    public float healthTickInterval = 3f;
    public float healthDamagePerTick = 3f;

    private float healthTimer = 0f;

    void OnEnable()
    {
        PlayerVitals.OnHealthChanged += SyncHealth;
    }

    void OnDisable()
    {
        PlayerVitals.OnHealthChanged -= SyncHealth;
    }

    // 포션으로 체력이 회복되면 호출됨
    void SyncHealth()
    {
        currentHealth = PlayerVitals.savedHealth;
        if (healthBar != null)
            healthBar.value = currentHealth;
    }

    void Start()
    {
        currentOxygen = maxOxygen;

        // 작업장에서도 최대 체력을 알 수 있게 기록
        PlayerVitals.maxHealth = maxHealth;

        if (PlayerVitals.savedHealth > 0f)
            currentHealth = Mathf.Min(PlayerVitals.savedHealth, maxHealth);
        else
            currentHealth = maxHealth;

        if (oxygenBar != null)
        {
            oxygenBar.maxValue = maxOxygen;
            oxygenBar.value = currentOxygen;
        }

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        PlayerVitals.savedHealth = currentHealth;
    }

    void Update()
    {
        if (currentOxygen > 0)
        {
            currentOxygen -= Time.deltaTime;
            if (currentOxygen < 0) currentOxygen = 0;

            if (oxygenBar != null)
                oxygenBar.value = currentOxygen;
        }
        else
        {
            healthTimer += Time.deltaTime;

            if (healthTimer >= healthTickInterval)
            {
                currentHealth -= healthDamagePerTick;
                if (currentHealth < 0) currentHealth = 0;

                if (healthBar != null)
                    healthBar.value = currentHealth;

                PlayerVitals.savedHealth = currentHealth;
                healthTimer = 0f;
            }
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log("GAME OVER");
        }
    }
}