using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    [Header("이 창들 중 하나라도 켜져 있으면 산소/체력이 멈춤 (예: 인벤토리 창)")]
    public GameObject[] freezeWhileOpen;

    private float healthTimer = 0f;
    private bool isPreview;
    private bool lastFrozen = false;
    private string freezeReason = "";

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
        // 작업장에서 미리보기로 로드된 씬이면 산소/체력을 줄이지 않음
        isPreview = gameObject.scene != SceneManager.GetActiveScene();

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

        Debug.Log("[산소/체력] 시작 / 미리보기 = " + isPreview +
                  " / 멈춤 창 개수 = " + (freezeWhileOpen == null ? 0 : freezeWhileOpen.Length));
    }

    // 정화가 끝났거나 지정한 창이 열려 있으면 true
    bool IsFrozen()
    {
        if (PurificationManager.PurificationDone)
        {
            freezeReason = "정화 완료";
            return true;
        }

        if (freezeWhileOpen == null || freezeWhileOpen.Length == 0)
        {
            freezeReason = "Freeze While Open 목록이 비어 있음";
            return false;
        }

        foreach (var go in freezeWhileOpen)
        {
            if (go == null)
            {
                freezeReason = "목록에 빈 칸(None)이 있음";
                continue;
            }

            if (go.activeInHierarchy)
            {
                freezeReason = go.name + " 켜져 있음";
                return true;
            }

            freezeReason = go.name + " 꺼져 있음";
        }

        return false;
    }

    void Update()
    {
        if (isPreview) return;

        // 멈춰 있는 동안에는 산소도 체력도 줄지 않음
        bool frozen = IsFrozen();

        if (frozen != lastFrozen)
        {
            Debug.Log("[산소/체력] 멈춤 = " + frozen + " (" + freezeReason + ")");
            lastFrozen = frozen;
        }

        if (frozen) return;

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