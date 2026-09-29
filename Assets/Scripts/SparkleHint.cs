using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 이 스크립트는 BoatHintGroup 오브젝트에 붙이세요.
public class SparkleHint : MonoBehaviour
{
    [Header("반짝임 유도 타이밍")]
    public float hintDelay = 3f;      // 게임 시작 후 처음 반짝이기까지 대기 시간(초)
    public float hintInterval = 5f;   // 한 번 반짝인 뒤 다음 반짝임까지 간격(초)
    public float singleSparkleDuration = 0.5f; // 반짝이 하나가 나타났다 사라지는데 걸리는 시간
    public float staggerDelay = 0.15f; // 반짝이들 사이의 시차 (0.15초씩 순서대로 시작)

    [Header("반짝이 이미지 3개 연결 (Sparkle1, Sparkle2, Sparkle3)")]
    public Image[] sparkles;

    private float timer;
    private bool isHinting = false;

    void Start()
    {
        timer = hintDelay;

        // 처음엔 전부 안 보이게
        SetAllAlpha(0f);

        // 부모 쪽에 Button이 있으면(보트를 눌렀을 때) 반짝임을 멈추도록 자동 연결
        Button btn = GetComponentInParent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() => { StopHinting(); });
        }
    }

    void Update()
    {
        if (isHinting) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            StartCoroutine(HintSequence());
        }
    }

    IEnumerator HintSequence()
    {
        isHinting = true;

        // 각 반짝이를 살짝 다른 타이밍에 시작시켜서 트윙클 느낌을 냄
        for (int i = 0; i < sparkles.Length; i++)
        {
            StartCoroutine(SingleSparkle(sparkles[i], i * staggerDelay));
        }

        // 모든 반짝이가 끝날 시간만큼 대기 (마지막 반짝이 시작 시차 + 반짝이 지속시간)
        float totalTime = (sparkles.Length - 1) * staggerDelay + singleSparkleDuration;
        yield return new WaitForSeconds(totalTime);

        isHinting = false;
        timer = hintInterval; // 다음 반짝임까지 다시 대기
    }

    IEnumerator SingleSparkle(Image img, float delay)
    {
        if (img == null) yield break;

        yield return new WaitForSeconds(delay);

        float t = 0f;
        while (t < singleSparkleDuration)
        {
            t += Time.deltaTime;
            // 0 -> 1 -> 0 곡선으로 나타났다가 사라짐
            float p = Mathf.Sin((t / singleSparkleDuration) * Mathf.PI);
            SetAlpha(img, p);
            yield return null;
        }

        SetAlpha(img, 0f);
    }

    void SetAllAlpha(float a)
    {
        foreach (var img in sparkles)
        {
            SetAlpha(img, a);
        }
    }

    void SetAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = a;
        img.color = c;
    }

    // 배를 클릭하면 더 이상 반짝이지 않도록 외부에서도 호출 가능
    public void StopHinting()
    {
        StopAllCoroutines();
        SetAllAlpha(0f);
        enabled = false; // Update 정지
    }
}