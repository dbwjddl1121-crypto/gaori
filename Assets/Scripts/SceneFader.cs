using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    public Image fadeImage; // 아까 만든 패널의 Image 연결
    public float fadeDuration = 1.0f; // 페이드 되는 시간 (1초)

    void Start()
    {
        // 게임 시작할 때 화면이 밝아지는 효과 (선택사항)
        StartCoroutine(FadeIn());
    }

    // 외부(버튼)에서 호출할 함수
    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOut(sceneName));
    }

    IEnumerator FadeIn()
    {
        float timer = 0f;
        Color color = fadeImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
        fadeImage.gameObject.SetActive(false); // 끝나면 패널 끄기
    }

    IEnumerator FadeOut(string sceneName)
    {
        fadeImage.gameObject.SetActive(true);
        float timer = 0f;
        Color color = fadeImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        // 완전히 어두워진 후 다음 씬으로 이동
        SceneManager.LoadScene(sceneName);
    }
}