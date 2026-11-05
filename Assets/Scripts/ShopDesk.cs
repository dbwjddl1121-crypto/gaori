using UnityEngine;
using UnityEngine.UI;

public class ShopDesk : MonoBehaviour
{
    public GameObject shopPanel; // Inspector에서 연결

    [Header("거리 설정")]
    public float interactDistance = 40f;   // 이 거리 안에 들어와야 상점이 열림
    public Transform distanceTarget;       // 거리 기준점 (비워두면 이 버튼 위치)

    private Transform player;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OpenShop);

        PlayerMove playerMove = FindFirstObjectByType<PlayerMove>();
        if (playerMove != null)
            player = playerMove.transform;
    }

    void OpenShop()
    {
        if (player == null)
        {
            PlayerMove playerMove = FindFirstObjectByType<PlayerMove>();
            if (playerMove != null) player = playerMove.transform;
        }

        if (player != null)
        {
            Transform target = distanceTarget != null ? distanceTarget : transform;
            float dist = Vector2.Distance(target.position, player.position);
            Debug.Log("[상점 거리 확인] 현재 거리 = " + dist + " / 허용 거리 = " + interactDistance);

            if (dist > interactDistance)
            {
                Debug.Log("[실패] 상점에 너무 멀리 있어요. 가까이 다가가세요.");
                return;
            }
        }
        else
        {
            Debug.LogWarning("[상점 거리 확인] player를 찾지 못했어요!");
        }

        shopPanel.SetActive(true);
    }
}