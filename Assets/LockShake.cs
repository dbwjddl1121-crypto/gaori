using UnityEngine;
using System.Collections;

// 자물쇠 이미지를 좌우로 까딱이게 하는 효과
public class LockShake : MonoBehaviour
{
    [Header("흔들림 설정")]
    public float duration = 0.4f;     // 흔들리는 시간
    public float angle = 12f;         // 좌우 회전 각도
    public float speed = 40f;         // 흔들리는 속도
    public float moveAmount = 6f;     // 좌우로 살짝 움직이는 거리 (0이면 회전만)

    private RectTransform rect;
    private Vector2 basePos;
    private Coroutine running;

    void Awake()
    {
        rect = transform as RectTransform;
        basePos = rect.anchoredPosition;
    }

    void OnDisable()
    {
        // 흔들리는 도중에 꺼져도 원래 자세로 복구
        if (rect != null)
        {
            rect.localRotation = Quaternion.identity;
            rect.anchoredPosition = basePos;
        }
        running = null;
    }

    public void Shake()
    {
        if (!gameObject.activeInHierarchy) return;

        if (running != null)
            StopCoroutine(running);

        running = StartCoroutine(ShakeRoutine());
    }

    IEnumerator ShakeRoutine()
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            // 시간이 지날수록 흔들림이 약해짐
            float fade = 1f - (timer / duration);
            float wave = Mathf.Sin(timer * speed) * fade;

            rect.localRotation = Quaternion.Euler(0f, 0f, wave * angle);
            rect.anchoredPosition = basePos + new Vector2(wave * moveAmount, 0f);

            yield return null;
        }

        rect.localRotation = Quaternion.identity;
        rect.anchoredPosition = basePos;
        running = null;
    }
}