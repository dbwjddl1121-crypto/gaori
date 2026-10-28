
using UnityEngine;
using System.Collections;

public class SpriteFadeGroup : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] sprites;

    // 스프라이트를 서서히 투명하게 만들기
    public IEnumerator FadeOut(float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            SetAlpha(1f - Mathf.Clamp01(t / duration));
            yield return null;
        }

        SetAlpha(0f);
    }

    // 스프라이트를 서서히 다시 보이게 만들기
    public IEnumerator FadeIn(float duration)
    {
        float t = 0f;

        // 페이드 인 시작 시 투명하게 설정
        SetAlpha(0f);

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Clamp01(t / duration));
            yield return null;
        }

        SetAlpha(1f);
    }

    // 모든 스프라이트를 완전히 보이게 만들기
    public void ResetAlpha()
    {
        SetAlpha(1f);
    }

    // 모든 스프라이트의 투명도 설정
    public void SetAlpha(float a)
    {
        if (sprites == null)
            return;

        foreach (var s in sprites)
        {
            if (s == null)
                continue;

            Color c = s.color;
            c.a = a;
            s.color = c;
        }
    }
}
