using UnityEngine;

// 창 모드에서 마우스로 크기를 바꿔도 게임 비율(기본 16:9)로 맞춰주는 스크립트
public class AspectRatioLock : MonoBehaviour
{
    [Header("게임 비율 (가로 : 세로)")]
    public float ratioWidth = 16f;
    public float ratioHeight = 9f;

    [Header("크기 조절을 멈추고 이 시간(초)이 지나면 비율로 보정")]
    public float settleDelay = 0.3f;

    [Header("창이 모니터를 넘지 않게 빼 둘 세로 여유 (작업표시줄, 제목줄)")]
    public int verticalMargin = 100;

    static AspectRatioLock instance;

    int lastW, lastH;
    float stableTimer = 0f;

    void Awake()
    {
        // 씬이 바뀌어도 하나만 유지
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        lastW = Screen.width;
        lastH = Screen.height;
    }

    void Update()
    {
        // 전체화면이거나 창이 최소화된 상태면 건드리지 않음
        if (Screen.fullScreen || Screen.width <= 0 || Screen.height <= 0)
        {
            lastW = Screen.width;
            lastH = Screen.height;
            stableTimer = 0f;
            return;
        }

        // 크기가 아직 바뀌는 중이면(드래그 중) 기다림
        if (Screen.width != lastW || Screen.height != lastH)
        {
            lastW = Screen.width;
            lastH = Screen.height;
            stableTimer = 0f;
            return;
        }

        stableTimer += Time.unscaledDeltaTime;
        if (stableTimer < settleDelay) return;

        float targetAspect = ratioWidth / ratioHeight;
        float currentAspect = (float)Screen.width / Screen.height;

        // 이미 비율이 맞으면 끝
        if (Mathf.Abs(currentAspect - targetAspect) < 0.01f) return;

        // 가로 기준으로 세로를 맞춤
        int w = Screen.width;
        int h = Mathf.RoundToInt(w / targetAspect);

        // 모니터 세로를 넘으면 세로 기준으로 가로를 맞춤
        int maxH = Display.main.systemHeight - verticalMargin;
        if (h > maxH)
        {
            h = maxH;
            w = Mathf.RoundToInt(h * targetAspect);
        }

        Screen.SetResolution(w, h, FullScreenMode.Windowed);

        lastW = w;
        lastH = h;
        stableTimer = 0f;
    }
}