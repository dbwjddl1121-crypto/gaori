using UnityEngine;

public class TrashClickPickup : MonoBehaviour
{
    [Header("수거 가능한 거리")]
    public float pickupRange = 1.5f;

    private Transform player;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("TrashClickPickup: Player 태그를 찾을 수 없습니다.");
        }
    }

    private void OnMouseDown()
    {
        if (player == null)
        {
            Debug.LogWarning("TrashClickPickup: Player를 찾을 수 없습니다.");
            return;
        }

        float distance = Vector2.Distance(
            player.position,
            transform.position
        );

        if (distance > pickupRange)
        {
            Debug.Log("쓰레기가 너무 멀리 있습니다.");
            return;
        }

        Debug.Log("쓰레기 수거!");

        Destroy(gameObject);
    }
}