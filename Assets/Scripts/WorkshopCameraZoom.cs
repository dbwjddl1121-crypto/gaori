using UnityEngine;

public class WorkshopCameraZoom : MonoBehaviour
{
    [Header("대상")]
    public Transform target;                 // 따라갈 플레이어
    public PlayerMove playerMove;            // 입장 연출 끝 감지용
    public GameObject workshopPanel;         // WorkshopInteriorPanel (꺼지면 카메라 복구, 범위 기준)

    [Header("줌 설정")]
    public float wideSize = 260f;            // 전체화면 Size
    public float zoomedSize = 150f;          // 확대했을 때 Size (작을수록 확대)
    public float zoomSpeed = 2f;             // 확대/이동 속도
    public float moveThreshold = 0.5f;       // 이 거리 이상 움직이면 '움직였다'고 판단

    [Header("범위 제한")]
    public bool useLimit = true;             // 맵 밖이 안 보이게 제한

    private Vector3 wideCenter;
    private Vector3 playerRefPos;
    private bool entryDone = false;
    private bool zoomStarted = false;
    private Camera cam;
    private RectTransform panelRect;

    void Start()
    {
        cam = GetComponent<Camera>();
        wideCenter = transform.position;
        cam.orthographicSize = wideSize;

        if (workshopPanel != null)
            panelRect = workshopPanel.GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        // 작업장이 꺼져 있으면 카메라를 원래대로 되돌리고 상태 초기화
        if (workshopPanel != null && !workshopPanel.activeInHierarchy)
        {
            ResetCamera();
            return;
        }

        if (target == null || playerMove == null) return;

        // 입장 연출이 끝날 때까지(PlayerMove가 켜질 때까지) 대기
        if (!entryDone)
        {
            if (!playerMove.enabled) return;
            entryDone = true;
            playerRefPos = target.position;
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
        Vector3 next = Vector3.Lerp(transform.position, desired, zoomSpeed * Time.deltaTime);

        transform.position = useLimit ? ClampToPanel(next) : next;
    }

    // 카메라 화면이 배경 패널 밖으로 나가지 않게 위치를 제한
    Vector3 ClampToPanel(Vector3 pos)
    {
        if (panelRect == null) return pos;

        Vector3[] c = new Vector3[4];
        panelRect.GetWorldCorners(c); // 0:좌하, 2:우상

        float minX = c[0].x, maxX = c[2].x;
        float minY = c[0].y, maxY = c[2].y;

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        // 화면이 패널보다 크면 가운데에 고정, 아니면 범위 안으로 제한
        pos.x = (maxX - minX <= halfW * 2f)
            ? (minX + maxX) * 0.5f
            : Mathf.Clamp(pos.x, minX + halfW, maxX - halfW);

        pos.y = (maxY - minY <= halfH * 2f)
            ? (minY + maxY) * 0.5f
            : Mathf.Clamp(pos.y, minY + halfH, maxY - halfH);

        return pos;
    }

    void ResetCamera()
    {
        cam.orthographicSize = wideSize;
        transform.position = wideCenter;
        entryDone = false;
        zoomStarted = false;
    }
}