using UnityEngine;

public class PixelFishSmooth : MonoBehaviour
{
    [Header("처음 오른쪽으로 이동")]
    public bool startRight = true;

    [Header("원본 물고기가 오른쪽을 보고 있는지")]
    public bool spriteFacesRight = true;

    [Header("속도")]
    public float minSpeed = 1f;
    public float maxSpeed = 3f;

    [Header("이동 범위")]
    public float moveRange = 8f;

    [Header("둥실거림")]
    public float swimHeight = 0.2f;
    public float swimSpeed = 2f;

    private float speed;
    private int direction;

    private Vector3 startPos;
    private float swimOffset;

    void Start()
    {
        startPos = transform.position;

        // 랜덤 속도
        speed = Random.Range(minSpeed, maxSpeed);

        // 시작 방향
        direction = startRight ? 1 : -1;

        // 둥실 타이밍 랜덤
        swimOffset = Random.Range(0f, 10f);

        // 랜덤 크기
        float size = Random.Range(0.8f, 1.3f);

        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x) * size,
            transform.localScale.y * size,
            transform.localScale.z * size
        );

        Flip();
    }

    void Update()
    {
        // 이동
        transform.Translate(
            Vector3.right *
            direction *
            speed *
            Time.deltaTime
        );

        // 둥실거림
        float y =
            Mathf.Sin(Time.time * swimSpeed + swimOffset)
            * swimHeight;

        transform.position = new Vector3(
            transform.position.x,
            startPos.y + y,
            transform.position.z
        );

        // 방향 전환
        if (transform.position.x > startPos.x + moveRange)
        {
            direction = -1;
            Flip();
        }

        if (transform.position.x < startPos.x - moveRange)
        {
            direction = 1;
            Flip();
        }
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;

        if (spriteFacesRight)
        {
            scale.x =
                Mathf.Abs(scale.x) * direction;
        }
        else
        {
            scale.x =
                -Mathf.Abs(scale.x) * direction;
        }

        transform.localScale = scale;
    }
}