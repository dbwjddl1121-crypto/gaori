using UnityEngine;

public class BackgroundFloat : MonoBehaviour
{
    [Header("흔들림 설정")]
    public float speed = 1.5f;       // 흔들리는 속도
    public float amount = 5.0f;      // 흔들리는 범위(크기)

    private Vector2 startPos;

    void Start()
    {
        // 처음 시작했을 때의 UI 위치를 저장해둡니다.
        RectTransform rectTrans = GetComponent<RectTransform>();
        if (rectTrans != null)
        {
            startPos = rectTrans.anchoredPosition;
        }
        else
        {
            startPos = transform.position;
        }
    }

    void Update()
    {
        // 삼각함수(Sin, Cos)를 이용해 부드럽게 상하좌우로 미세하게 움직이게 만듭니다.
        float x = Mathf.Sin(Time.time * speed) * amount;
        float y = Mathf.Cos(Time.time * speed * 0.8f) * amount;

        RectTransform rectTrans = GetComponent<RectTransform>();
        if (rectTrans != null)
        {
            rectTrans.anchoredPosition = startPos + new Vector2(x, y);
        }
        else
        {
            transform.position = startPos + new Vector2(x, y);
        }
    }
}