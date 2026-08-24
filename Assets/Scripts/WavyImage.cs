using UnityEngine;

public class WavyImage : MonoBehaviour
{
    [Header("배경 찰랑임 설정")]
    public float waveSpeed = 2f;      // 흔들리는 속도
    public float waveAmountX = 5f;    // 좌우로 흔들리는 폭
    public float waveAmountY = 3f;    // 위아래로 찰랑이는 폭

    private RectTransform rectTransform;
    private Vector2 startPos;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;
    }

    void Update()
    {
        // 삼각함수를 이용해 X축과 Y축으로 아주 미세하게 엇갈려 움직이게 만듭니다.
        float offsetX = Mathf.Sin(Time.time * waveSpeed) * waveAmountX;
        float offsetY = Mathf.Cos(Time.time * waveSpeed * 1.2f) * waveAmountY;

        rectTransform.anchoredPosition = new Vector2(startPos.x + offsetX, startPos.y + offsetY);
    }
}