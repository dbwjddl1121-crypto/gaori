using UnityEngine;

public class BoatFloat : MonoBehaviour
{
    [Header("흔들림 설정 (제자리)")]
    public float floatSpeed = 2f;      // 흔들리는 속도
    public float moveAmount = 10f;     // 위아래로 움직이는 거리
    public float rotateAmount = 2f;    // 살짝 기울어지는 각도

    [Header("아주 미세한 이동 설정 (둥실둥실 떠다님)")]
    public float driftSpeed = 0.5f;    // 아주 느리게 이동하는 속도
    public float driftRange = 30f;     // 이동하는 최대 범위 (넓지 않고 아담하게 움직임)

    private Vector3 startPos;

    void Start()
    {
        // 배의 원래 위치를 기억해 둡니다.
        startPos = transform.localPosition;
    }

    void Update()
    {
        // 1. 삼각함수를 이용해 아주 느린 미세 이동(드리프트) 위치 계산
        float offsetX = Mathf.Cos(Time.time * driftSpeed * 0.7f) * driftRange;
        float offsetY = Mathf.Sin(Time.time * driftSpeed) * (driftRange * 0.5f); // Y축은 폭을 절반으로 줄여 자연스럽게

        // 2. 기존의 출렁이는(Sin) 운동 계산
        float waveY = Mathf.Sin(Time.time * floatSpeed) * moveAmount;
        float newRotZ = Mathf.Sin(Time.time * (floatSpeed * 0.8f)) * rotateAmount;

        // 3. 기준 위치에 미세 이동과 파도 출렁임을 합쳐서 적용
        float finalX = startPos.x + offsetX;
        float finalY = startPos.y + offsetY + waveY;

        transform.localPosition = new Vector3(finalX, finalY, startPos.z);
        transform.localRotation = Quaternion.Euler(0, 0, newRotZ);
    }
}