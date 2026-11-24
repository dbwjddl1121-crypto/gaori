using System.Collections.Generic;
using UnityEngine;

// 바다 씬의 Trash 태그 오브젝트를 배경 범위 안의 랜덤 위치로 흩뿌린다.
// Awake에서 실행되므로 TrashFloat / TrashClickPickup의 Start()보다 먼저 적용된다.
[DefaultExecutionOrder(-100)]
public class TrashRandomizer : MonoBehaviour
{
    [Header("스테이지 번호 (0=연안, 1=중층, 2=심해)")]
    [SerializeField] int stageIndex = 0;

    [Header("랜덤 범위 (이 배경 안에 배치)")]
    [SerializeField] SpriteRenderer areaRenderer;   // 오염 바다 배경 오브젝트

    [Header("설정")]
    [SerializeField] string trashTag = "Trash";
    [SerializeField] float edgeMargin = 30f;         // 가장자리에서 띄울 거리
    [SerializeField] float minDistance = 40f;        // 쓰레기끼리 최소 간격
    [SerializeField] int triesPerTrash = 30;         // 간격 맞추기 재시도 횟수
    [SerializeField] bool keepPlayerAreaClear = true;
    [SerializeField] float playerClearRadius = 60f;  // 플레이어 시작 위치 주변 비우기

    [Header("테스트용")]
    [SerializeField] bool rerollEveryTime = false;   // 켜면 입장할 때마다 새로 랜덤 (먹은 기록은 어긋날 수 있음)

    void Awake()
    {
        if (areaRenderer == null)
        {
            Debug.LogWarning("TrashRandomizer: Area Renderer가 연결되지 않았어요.");
            return;
        }

        // 미리보기(스테이지 선택창)로 로드된 경우엔 건드리지 않는다
        if (gameObject.scene != UnityEngine.SceneManagement.SceneManager.GetActiveScene())
            return;

        var trashList = new List<GameObject>();
        foreach (var go in GameObject.FindGameObjectsWithTag(trashTag))
        {
            if (go.scene == gameObject.scene)
                trashList.Add(go);
        }
        if (trashList.Count == 0) return;

        // 같은 순서로 저장/복원할 수 있게 이름+원래 위치로 정렬
        trashList.Sort((a, b) =>
        {
            int c = string.CompareOrdinal(a.name, b.name);
            if (c != 0) return c;
            c = a.transform.position.x.CompareTo(b.transform.position.x);
            if (c != 0) return c;
            return a.transform.position.y.CompareTo(b.transform.position.y);
        });

        string sceneName = gameObject.scene.name;
        bool hasSaved = !rerollEveryTime && PlayerPrefs.HasKey(SaveKey(sceneName, 0, 'x'));

        Bounds b = areaRenderer.bounds;
        float minX = b.min.x + edgeMargin;
        float maxX = b.max.x - edgeMargin;
        float minY = b.min.y + edgeMargin;
        float maxY = b.max.y - edgeMargin;

        Vector3 playerPos = Vector3.zero;
        bool hasPlayer = false;
        if (keepPlayerAreaClear)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null && p.scene == gameObject.scene)
            {
                playerPos = p.transform.position;
                hasPlayer = true;
            }
        }

        var placed = new List<Vector2>();

        for (int i = 0; i < trashList.Count; i++)
        {
            Transform t = trashList[i].transform;
            Vector2 pos;

            if (hasSaved)
            {
                // 저장된 랜덤 위치 복원
                pos = new Vector2(
                    PlayerPrefs.GetFloat(SaveKey(sceneName, i, 'x'), t.position.x),
                    PlayerPrefs.GetFloat(SaveKey(sceneName, i, 'y'), t.position.y));
            }
            else
            {
                pos = PickPosition(minX, maxX, minY, maxY, placed, hasPlayer, playerPos);
                PlayerPrefs.SetFloat(SaveKey(sceneName, i, 'x'), pos.x);
                PlayerPrefs.SetFloat(SaveKey(sceneName, i, 'y'), pos.y);
            }

            placed.Add(pos);
            t.position = new Vector3(pos.x, pos.y, t.position.z);
        }

        if (!hasSaved) PlayerPrefs.Save();
    }

    Vector2 PickPosition(float minX, float maxX, float minY, float maxY,
                         List<Vector2> placed, bool hasPlayer, Vector3 playerPos)
    {
        Vector2 best = Vector2.zero;
        for (int attempt = 0; attempt < triesPerTrash; attempt++)
        {
            Vector2 c = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
            best = c;

            if (hasPlayer && Vector2.Distance(c, playerPos) < playerClearRadius) continue;

            bool ok = true;
            foreach (var other in placed)
            {
                if (Vector2.Distance(c, other) < minDistance) { ok = false; break; }
            }
            if (ok) return c;
        }
        return best; // 못 찾으면 마지막 후보 사용
    }

    string SaveKey(string sceneName, int index, char axis)
    {
        return "TrashPos_" + sceneName + "_" + index + "_" + axis;
    }

    // 테스트용: 저장된 랜덤 위치 지우기
    public static void ResetSaved()
    {
        // PlayerPrefs는 키 목록 조회가 안 되므로 Edit > Clear All PlayerPrefs를 쓰세요.
    }
}