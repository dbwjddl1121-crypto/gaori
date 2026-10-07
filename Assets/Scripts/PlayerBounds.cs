using UnityEngine;

public class PlayerBounds : MonoBehaviour
{
    [Header("맵 배경 (SpriteRenderer)")]
    public SpriteRenderer mapBackground;

    [Header("가장자리 여백 (캐릭터 크기만큼)")]
    public float paddingX = 0.5f;
    public float paddingY = 0.5f;

    // 플레이어가 움직인 뒤에 위치를 보정하려고 LateUpdate 사용
    void LateUpdate()
    {
        if (mapBackground == null) return;

        Bounds b = mapBackground.bounds;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, b.min.x + paddingX, b.max.x - paddingX);
        pos.y = Mathf.Clamp(pos.y, b.min.y + paddingY, b.max.y - paddingY);
        transform.position = pos;
    }
}