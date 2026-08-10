using UnityEngine;

public class TrashFloat : MonoBehaviour
{
    [Header("위아래 움직임")]
    public float floatAmount = 0.3f;

    [Header("좌우 움직임")]
    public float sideAmount = 0.1f;

    [Header("움직임 속도")]
    public float floatSpeed = 1.5f;

    private Vector3 startPos;
    private float randomOffset;

    void Start()
    {
        startPos = transform.position;

        // 쓰레기마다 다른 타이밍
        randomOffset = Random.Range(0f, 10f);
    }

    void Update()
    {
        float y =
            Mathf.Sin(
                Time.time * floatSpeed + randomOffset
            ) * floatAmount;

        float x =
            Mathf.Cos(
                Time.time * floatSpeed + randomOffset
            ) * sideAmount;

        transform.position = new Vector3(
            startPos.x + x,
            startPos.y + y,
            startPos.z
        );
    }
}