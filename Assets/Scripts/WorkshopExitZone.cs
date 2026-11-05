using UnityEngine;

public class WorkshopExitZone : MonoBehaviour
{
    [Header("연결")]
    public WorkshopController workshop;   // WorkshopController가 붙은 오브젝트

    [Header("나가기 위치 / 범위")]
    public Vector2 exitPosition = new Vector2(459.4f, -27.39f);
    public float exitDistance = 60f;      // 이 거리 안에 들어오면 나감

    private Transform player;
    private bool armed = false;     // 입구에서 한 번 멀어진 뒤에야 작동
    private bool exiting = false;   // 나가는 연출 중복 방지

    void Update()
    {
        if (workshop == null || workshop.workshopInteriorPanel == null)
            return;

        // 작업장 안에 있을 때만 작동
        if (!workshop.workshopInteriorPanel.activeInHierarchy)
        {
            armed = false;
            exiting = false;
            return;
        }

        if (player == null)
        {
            PlayerMove pm = FindFirstObjectByType<PlayerMove>();
            if (pm == null) return;
            player = pm.transform;
        }

        float dist = Vector2.Distance(player.position, exitPosition);

        // 들어오자마자 바로 나가지는 것 방지: 입구에서 한 번 벗어나야 활성화
        if (!armed)
        {
            if (dist > exitDistance) armed = true;
            return;
        }

        if (!exiting && dist <= exitDistance)
        {
            exiting = true;
            workshop.ExitWorkshop();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(exitPosition, exitDistance);
    }
}