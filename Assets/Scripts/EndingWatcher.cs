using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// 분리수거통이 있는 씬에 두는 스크립트.
// "심해 정화 완료 + 분리수거 전부 완료"가 되면 흰색 페이드아웃 후 해피엔딩으로 이동한다.
public class EndingWatcher : MonoBehaviour
{
    [Header("이동할 씬")]
    [SerializeField] string endingSceneName = "EndingScene";

    [Header("마지막 분리수거 후 기다릴 시간(초)")]
    [Tooltip("뚜껑 애니메이션과 캐시 지급 연출이 끝나도록 잠깐 기다려요.")]
    [SerializeField] float endDelay = 1.5f;

    [Header("확인 주기(초)")]
    [SerializeField] float checkInterval = 0.3f;

    // TrashCanController의 분류 이름과 같아야 해요. 새 쓰레기를 추가하면 여기에도 추가하세요.
    // 띄어쓰기는 자동으로 무시해요.
    [Header("쓰레기 이름 목록 (이 이름만 쓰레기로 셈)")]
    [SerializeField]
    string[] trashNames = new string[]
    {
        "사이다병", "유리공병", "맥주병", "음료수병",
        "생수병",
        "비닐봉지", "과자봉지", "라면봉지",
        "참치캔", "통조림캔", "콜라캔",
        "종이컵", "피자박스", "깨진유리"
    };

    InventoryManager inventory;
    bool triggered;
    float timer;

    void Start()
    {
        // 미리보기로 겹쳐 로드된 씬에서는 동작하지 않게 함
        if (gameObject.scene != SceneManager.GetActiveScene())
        {
            enabled = false;
            return;
        }

        inventory = FindFirstObjectByType<InventoryManager>();
    }

    void Update()
    {
        if (triggered)
            return;

        timer += Time.deltaTime;

        if (timer < checkInterval)
            return;

        timer = 0f;

        if (inventory == null)
        {
            inventory = FindFirstObjectByType<InventoryManager>();
            if (inventory == null)
                return;
        }

        // 조건 1: 심해 정화 완료
        if (!StageProgress.IsCleaned(2))
            return;

        // 조건 3: 분리수거 미니게임 진행 중이면 기다림
        if (MiniGameManager.Instance != null && MiniGameManager.Instance.IsPlaying)
            return;

        // 조건 2: 인벤토리에 쓰레기가 남아 있으면 아직 안 끝남
        if (HasTrashInInventory())
            return;

        triggered = true;
        StartCoroutine(GoHappyEnding());
    }

    bool HasTrashInInventory()
    {
        int count = inventory.SlotCount;

        for (int i = 0; i < count; i++)
        {
            Item item = inventory.GetItemAt(i);

            if (item == null)
                continue;

            if (IsTrashName(item.itemName))
                return true;
        }

        return false;
    }

    bool IsTrashName(string itemName)
    {
        string n = RemoveSpaces(itemName);

        foreach (string t in trashNames)
        {
            if (n == RemoveSpaces(t))
                return true;
        }

        return false;
    }

    static string RemoveSpaces(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace(" ", "").Replace("\u00A0", "").Trim();
    }

    IEnumerator GoHappyEnding()
    {
        Debug.Log("[EndingWatcher] 심해 정화 + 분리수거 완료 → 해피엔딩");

        if (endDelay > 0f)
            yield return new WaitForSeconds(endDelay);

        // 흰색으로 페이드아웃 후 엔딩 씬으로 이동
        EndingFader.FadeOutAndLoad(this, EndingController.EndingType.Happy, endingSceneName);
    }
}