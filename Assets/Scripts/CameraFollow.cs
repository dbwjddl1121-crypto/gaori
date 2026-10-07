using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("따라갈 대상")]
    public Transform target;          // Player 연결

    [Header("따라가는 부드러움 (클수록 빠르게 따라감)")]
    public float smoothSpeed = 5f;

    [Header("카메라 이동 제한")]
    public bool useBounds = true;
    public SpriteRenderer mapBackground; // 맵 배경 이미지 연결

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // 플레이어가 먼저 움직인 뒤 카메라가 움직이도록 LateUpdate 사용
    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = new Vector3(
            target.position.x,
            target.position.y,
            transform.position.z   // 카메라 Z값은 유지
        );

        if (useBounds && mapBackground != null)
        {
            desired = ClampToMap(desired);
        }

        transform.position = Vector3.Lerp(
            transform.position,
            desired,
            smoothSpeed * Time.deltaTime
        );
    }

    // 카메라가 맵 밖(빈 공간)을 비추지 않게 제한
    Vector3 ClampToMap(Vector3 pos)
    {
        Bounds b = mapBackground.bounds;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float minX = b.min.x + halfWidth;
        float maxX = b.max.x - halfWidth;
        float minY = b.min.y + halfHeight;
        float maxY = b.max.y - halfHeight;

        // 맵이 화면보다 작으면 가운데에 고정
        pos.x = (minX > maxX) ? b.center.x : Mathf.Clamp(pos.x, minX, maxX);
        pos.y = (minY > maxY) ? b.center.y : Mathf.Clamp(pos.y, minY, maxY);

        return pos;
    }
}