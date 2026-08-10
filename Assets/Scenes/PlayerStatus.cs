using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour
{
    // 산소바
    public Slider oxygenBar;

    // 체력바
    public Slider healthBar;

    // 산소 최대값
    public float maxOxygen = 30f;

    // 현재 산소
    private float currentOxygen;

    // 체력 최대값
    public float maxHealth = 50f;

    // 현재 체력
    private float currentHealth;

    // 체력 감소 타이머
    private float healthTimer = 0f;

    void Start()
    {
        // 시작 값 설정
        currentOxygen = maxOxygen;
        currentHealth = maxHealth;

        // 산소바 설정
        oxygenBar.maxValue = maxOxygen;
        oxygenBar.value = currentOxygen;

        // 체력바 설정
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
    }

    void Update()
    {
        // 산소가 남아있으면 감소
        if (currentOxygen > 0)
        {
            currentOxygen -= Time.deltaTime;

            // 0 이하 방지
            if (currentOxygen < 0)
            {
                currentOxygen = 0;
            }

            // 산소바 업데이트
            oxygenBar.value = currentOxygen;
        }
        else
        {
            // 산소가 없으면 체력 감소 타이머 시작
            healthTimer += Time.deltaTime;

            // 5초마다 체력 감소
            if (healthTimer >= 5f)
            {
                currentHealth -= 5f;

                // 체력바 업데이트
                healthBar.value = currentHealth;

                // 타이머 초기화
                healthTimer = 0f;
            }
        }

        // 체력 0이면 게임오버
        if (currentHealth <= 0)
        {
            currentHealth = 0;

            Debug.Log("GAME OVER");
        }
    }
}