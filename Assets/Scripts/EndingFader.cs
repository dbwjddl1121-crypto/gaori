using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class EndingFader
{
    class Runner : MonoBehaviour { }

    // 색으로 페이드아웃한 뒤 엔딩 씬 로드
    public static void FadeOutAndLoad(MonoBehaviour owner, EndingController.EndingType type,
                                      string sceneName, float duration = 1.5f)
    {
        EndingController.pending = type;
        Color color = (type == EndingController.EndingType.Happy) ? Color.white : Color.black;

        var go = new GameObject("[EndingFade]");
        Object.DontDestroyOnLoad(go);
        var runner = go.AddComponent<Runner>();
        runner.StartCoroutine(Routine(go, color, sceneName, duration));
    }

    static IEnumerator Routine(GameObject go, Color color, string sceneName, float duration)
    {
        var canvasGo = new GameObject("Canvas");
        canvasGo.transform.SetParent(go.transform, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000;

        var imgGo = new GameObject("Fade");
        imgGo.transform.SetParent(canvasGo.transform, false);
        var img = imgGo.AddComponent<Image>();
        img.raycastTarget = true;
        var rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            img.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(t / duration));
            yield return null;
        }
        img.color = new Color(color.r, color.g, color.b, 1f);

        SceneManager.LoadScene(sceneName);
        yield return null;
        yield return null;

        // 엔딩 씬이 뜬 뒤 정리 (엔딩 씬 쪽에서 화면을 이어받음)
        Object.Destroy(go);
    }
}