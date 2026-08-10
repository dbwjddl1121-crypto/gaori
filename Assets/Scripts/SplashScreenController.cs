using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashController : MonoBehaviour
{
    public Image backgroundImage;
    public Image textImage;
    public Image[] sparkles;  // 반짝이 이미지들

    public float fadeInTime = 1f;
    public float delayBeforeBounce = 2f;
    public float displayTime = 3f;
    public float fadeOutTime = 1f;
    public string nextSceneName = "MainMenu";

    public float bounceHeight = 15f;
    public float bounceSpeed = 2f;

    private Vector3 textOriginalPosition;
    private bool shouldAnimate = false;

    void Start()
    {
        textOriginalPosition = textImage.rectTransform.localPosition;

        SetAlpha(backgroundImage, 0f);
        SetAlpha(textImage, 0f);

        // 반짝이들 숨기기
        foreach (var sparkle in sparkles)
        {
            SetAlpha(sparkle, 0f);
        }

        StartCoroutine(SplashSequence());
    }

    IEnumerator SplashSequence()
    {
        yield return StartCoroutine(FadeIn());
        yield return new WaitForSeconds(delayBeforeBounce);

        shouldAnimate = true;
        StartCoroutine(BounceText());
        StartCoroutine(AnimateSparkles());

        yield return new WaitForSeconds(displayTime);

        shouldAnimate = false;
        textImage.rectTransform.localPosition = textOriginalPosition;

        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator BounceText()
    {
        float time = 0f;

        while (shouldAnimate)
        {
            time += Time.deltaTime * bounceSpeed;
            float offsetY = Mathf.Sin(time) * bounceHeight;
            textImage.rectTransform.localPosition = textOriginalPosition + new Vector3(0, offsetY, 0);
            yield return null;
        }
    }

    IEnumerator AnimateSparkles()
    {
        while (shouldAnimate)
        {
            for (int i = 0; i < sparkles.Length; i++)
            {
                StartCoroutine(SparkleFlash(sparkles[i], i * 0.2f));
            }
            yield return new WaitForSeconds(1f);
        }

        // 반짝이 끄기
        foreach (var sparkle in sparkles)
        {
            SetAlpha(sparkle, 0f);
        }
    }

    IEnumerator SparkleFlash(Image sparkle, float delay)
    {
        yield return new WaitForSeconds(delay);

        float time = 0f;
        float duration = 0.5f;

        // 나타났다가
        while (time < duration / 2)
        {
            time += Time.deltaTime;
            float alpha = time / (duration / 2);
            SetAlpha(sparkle, alpha);
            yield return null;
        }

        // 사라짐
        time = 0f;
        while (time < duration / 2)
        {
            time += Time.deltaTime;
            float alpha = 1f - (time / (duration / 2));
            SetAlpha(sparkle, alpha);
            yield return null;
        }

        SetAlpha(sparkle, 0f);
    }

    IEnumerator FadeIn()
    {
        float time = 0f;

        while (time < fadeInTime)
        {
            time += Time.deltaTime;
            float alpha = time / fadeInTime;

            SetAlpha(backgroundImage, alpha);
            SetAlpha(textImage, alpha);

            yield return null;
        }

        SetAlpha(backgroundImage, 1f);
        SetAlpha(textImage, 1f);
    }

    IEnumerator FadeOut()
    {
        float time = 0f;

        while (time < fadeOutTime)
        {
            time += Time.deltaTime;
            float alpha = 1f - (time / fadeOutTime);

            SetAlpha(backgroundImage, alpha);
            SetAlpha(textImage, alpha);

            yield return null;
        }
    }

    void SetAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}
