using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// UI 위에서 조각이 사방으로 터지는 효과
public class UIBurstEffect : MonoBehaviour
{
    [Header("조각 설정")]
    public Sprite pieceSprite;                       // 비워 두면 단색 사각형
    public Vector2 pieceSize = new Vector2(24, 24);
    public Color[] colors =
    {
        new Color(1f, 0.85f, 0.2f),   // 노랑
        new Color(0.4f, 0.9f, 0.5f),  // 초록
        new Color(0.4f, 0.75f, 1f),   // 하늘
        new Color(1f, 0.5f, 0.6f)     // 분홍
    };

    [Header("움직임 설정")]
    public int count = 24;            // 조각 개수
    public float minSpeed = 300f;     // 터지는 최소 속도
    public float maxSpeed = 700f;     // 터지는 최대 속도
    public float gravity = 900f;      // 아래로 떨어지는 힘
    public float lifetime = 0.7f;     // 효과가 유지되는 시간

    public void Play()
    {
        StartCoroutine(BurstRoutine());
    }

    private IEnumerator BurstRoutine()
    {
        int n = count;
        RectTransform[] rects = new RectTransform[n];
        Image[] images = new Image[n];
        Vector2[] positions = new Vector2[n];
        Vector2[] velocities = new Vector2[n];
        float[] spins = new float[n];
        Color[] baseColors = new Color[n];

        // 조각 만들기
        for (int i = 0; i < n; i++)
        {
            GameObject go = new GameObject("Piece", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);

            rects[i] = go.GetComponent<RectTransform>();
            rects[i].sizeDelta = pieceSize * Random.Range(0.6f, 1.2f);

            images[i] = go.GetComponent<Image>();
            images[i].sprite = pieceSprite;
            images[i].raycastTarget = false;   // 클릭을 막지 않게

            baseColors[i] = colors[Random.Range(0, colors.Length)];
            images[i].color = baseColors[i];

            float angle = Random.Range(0f, Mathf.PI * 2f);
            float speed = Random.Range(minSpeed, maxSpeed);
            velocities[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
            spins[i] = Random.Range(-540f, 540f);
        }

        // 움직이기
        float t = 0f;
        while (t < lifetime)
        {
            float dt = Time.unscaledDeltaTime;
            t += dt;
            float p = Mathf.Clamp01(t / lifetime);

            for (int i = 0; i < n; i++)
            {
                if (rects[i] == null) continue;

                velocities[i].y -= gravity * dt;
                positions[i] += velocities[i] * dt;

                rects[i].anchoredPosition = positions[i];
                rects[i].Rotate(0f, 0f, spins[i] * dt);
                rects[i].localScale = Vector3.one * (1f - p * 0.5f);

                Color c = baseColors[i];
                c.a = 1f - p * p;   // 끝으로 갈수록 흐려짐
                images[i].color = c;
            }

            yield return null;
        }

        ClearPieces();
    }

    // 창이 닫혀도 조각이 남지 않게 정리
    private void OnDisable()
    {
        ClearPieces();
    }

    private void ClearPieces()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }
}