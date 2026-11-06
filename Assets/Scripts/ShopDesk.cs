using UnityEngine;
using UnityEngine.UI;

public class ShopDesk : MonoBehaviour
{
    public GameObject shopPanel; // Inspector에서 연결

    [Header("거리 설정")]
    public float interactDistance = 200f;  // 이 거리 안에 들어와야 상점이 열림
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

            if (dist > interactDistance)
            {
                NoticeUI.Show(
                    "상점이 너무 멀리 있습니다.",
                    "The shop is too far away."
                );
                return;
            }
        }

        shopPanel.SetActive(true);
    }
}