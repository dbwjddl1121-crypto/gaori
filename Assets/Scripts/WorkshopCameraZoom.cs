using UnityEngine;

public class WorkshopCameraZoom : MonoBehaviour
{
    [Header("대상")]
    public Transform target;                 // 따라갈 플레이어
    public PlayerMove playerMove;            // 입장 연출 끝 감지용 (Player의 PlayerMove)

    [Header("줌 설정")]
    public float wideSize = 260f;            // 전체화면 Size
    public float zoomedSize = 150f;          // 확대했을 때 Size (작을수록 확대)
    public float zoomSpeed = 2f;             // 확대/이동 속도
    public float moveThreshold = 0.5f;       // 이 거리 이상 움직이면 '움직였다'고 판단

    private Vector3 wideCenter;
    private Vector3 playerRefPos;
    private bool entryDone = false;
    private bool zoomStarted = false;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        wideCenter = transform.position;
        cam.orthographicSize = wideSize;
    }

    void LateUpdate()
    {
        if (target == null || playerMove == null) return;

        // 입장 연출이 끝날 때까지(PlayerMove가 켜질 때까지) 대기
        if (!entryDone)
        {
            if (!playerMove.enabled) return;
            entryDone = true;
            playerRefPos = target.position; // 연출 끝난 위치를 기준으로 삼음
        }

        // 연출 이후 처음 움직이면 줌 시작
        if (!zoomStarted)
        {
            if (Vector3.Distance(target.position, playerRefPos) > moveThreshold)
                zoomStarted = true;
            else
                return;
        }

        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, zoomedSize, zoomSpeed * Time.deltaTime);

        Vector3 desired = new Vector3(target.position.x, target.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, desired, zoomSpeed * Time.deltaTime);
    }
}