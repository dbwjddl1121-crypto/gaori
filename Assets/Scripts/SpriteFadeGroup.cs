using UnityEngine;
using System.Collections;

public class SpriteFadeGroup : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] sprites;

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

    public void ResetAlpha()
    {
        SetAlpha(1f);
    }

    public void SetAlpha(float a)
    {
        foreach (var s in sprites)
        {
            if (s == null) continue;
            var c = s.color;
            c.a = a;
            s.color = c;
        }
    }
}