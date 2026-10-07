using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ExitByKey : MonoBehaviour
{
    [Header("이동할 씬 이름 (Build Profiles에 등록된 이름과 똑같이)")]
    public string workshopSceneName = "GameScene";

    [Header("나가기 키")]
    public KeyCode exitKey = KeyCode.B;

    [Header("페이드")]
    public CanvasGroup fadePanel;        // ExitFadePanel 연결
    public float fadeInDuration = 0.5f;  // 씬 시작 시 밝아지는 시간
    public float fadeOutDuration = 0.5f; // 나갈 때 어두워지는 시간

    private bool isLeaving = false;

    void Start()
    {
        if (fadePanel != null)
        {
            // 씬 시작 시: 켜서 밝아지는 연출 → 끝나면 다시 끔
            fadePanel.gameObject.SetActive(true);
            fadePanel.alpha = 1f;
            fadePanel.blocksRaycasts = false;
            StartCoroutine(Fade(1f, 0f, fadeInDuration, () =>
            {
                fadePanel.gameObject.SetActive(false);
            }));
        }
    }

    void Update()
    {
        if (isLeaving) return;

        if (Input.GetKeyDown(exitKey))
        {
            isLeaving = true;

            if (fadePanel != null)
            {
                // 나갈 때: 패널을 켜고 어두워지게 함
                fadePanel.gameObject.SetActive(true);
                fadePanel.blocksRaycasts = true; // 나가는 중 클릭 막기
                StartCoroutine(Fade(0f, 1f, fadeOutDuration, () =>
                {
                    SceneManager.LoadScene(workshopSceneName);
                }));
            }
            else
            {
                SceneManager.LoadScene(workshopSceneName);
            }
        }
    }

    IEnumerator Fade(float from, float to, float duration, System.Action onDone)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            fadePanel.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }

        fadePanel.alpha = to;
        onDone?.Invoke();
    }
}