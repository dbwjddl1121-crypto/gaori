using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public Image fadeImage;

    public void LoadGameScene()
    {
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        float time = 0;

        while (time < 1)
        {
            time += Time.deltaTime;

            Color color = fadeImage.color;
            color.a = time;

            fadeImage.color = color;

            yield return null;
        }

        SceneManager.LoadScene("GameScene");
    }
}