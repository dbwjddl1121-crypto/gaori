using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// 심해 씬을 어둡게 덮고, 플레이어 주변만 동그랗게 보이게 합니다.
// 손전등을 장착하면 보이는 동그라미가 커집니다.
public class DeepDarkness : MonoBehaviour
{
    [Header("어둠")]
    [Range(0f, 1f)] public float darkness = 0.92f;   // 1이면 완전히 까맣게
    public int sortingOrder = 10;                    // 다른 UI와 겹칠 때 조절

    [Header("보이는 동그라미 (화면 세로 길이 기준 반지름)")]
    [Range(0.02f, 1f)] public float visibleRadius = 0.10f;     // 손전등 없을 때
    [Range(0.02f, 1f)] public float flashlightRadius = 0.28f;  // 손전등 장착했을 때
    [Range(0f, 0.95f)] public float softEdge = 0.6f; // 가장자리가 부드러운 정도
    public float smoothSpeed = 6f;                   // 크기가 바뀔 때 부드러운 정도

    [Header("테스트")]
    public bool darknessOn = true;                   // 끄면 어둠이 사라짐
    public bool forceFlashlight = false;             // 체크하면 손전등 없이도 켜진 것처럼 테스트

    private Canvas canvas;
    private RawImage center, left, right, top, bottom;
    private Texture2D circleTexture;

    private Transform player;
    private Camera cam;

    private float currentRadius;
    private float currentAlpha;
    private bool built = false;

    // 지금 손전등이 켜져 있는지
    bool FlashlightOn
    {
        get { return forceFlashlight || EquipEffects.HasFlashlight; }
    }

    float TargetRadius
    {
        get { return FlashlightOn ? flashlightRadius : visibleRadius; }
    }

    void Start()
    {
        // 작업장에서 미리보기로 같이 불러온 씬이면 어둠을 만들지 않음
        if (gameObject.scene != SceneManager.GetActiveScene())
        {
            enabled = false;
            return;
        }

        cam = Camera.main;
        FindPlayer();
        Build();

        currentRadius = TargetRadius;
        currentAlpha = darknessOn ? darkness : 0f;
        built = true;
    }

    void FindPlayer()
    {
        PlayerMovement pm = FindFirstObjectByType<PlayerMovement>();
        if (pm != null)
            player = pm.transform;
    }

    void Build()
    {
        GameObject canvasGo = new GameObject("DarknessCanvas", typeof(Canvas));
        canvasGo.transform.SetParent(transform, false);

        canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        circleTexture = MakeCircleTexture(256, softEdge);

        center = MakeImage("Center", canvasGo.transform, circleTexture);
        left = MakeImage("Left", canvasGo.transform, null);
        right = MakeImage("Right", canvasGo.transform, null);
        top = MakeImage("Top", canvasGo.transform, null);
        bottom = MakeImage("Bottom", canvasGo.transform, null);
    }

    RawImage MakeImage(string name, Transform parent, Texture tex)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(RawImage));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
        rt.pivot = Vector2.zero;

        RawImage img = go.GetComponent<RawImage>();
        img.texture = tex;
        img.raycastTarget = false; // 클릭을 막지 않게

        return img;
    }

    // 가운데는 투명하고 가장자리로 갈수록 까매지는 동그란 그림 만들기
    Texture2D MakeCircleTexture(int size, float soft)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Color32[] pixels = new Color32[size * size];
        float half = size / 2f;
        float inner = 1f - soft; // 이 안쪽은 완전히 투명

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f - half) / half;
                float dy = (y + 0.5f - half) / half;
                float d = Mathf.Sqrt(dx * dx + dy * dy);

                float a;
                if (d <= inner) a = 0f;
                else if (d >= 1f) a = 1f;
                else a = Mathf.SmoothStep(0f, 1f, (d - inner) / (1f - inner));

                pixels[y * size + x] = new Color32(0, 0, 0, (byte)Mathf.RoundToInt(a * 255f));
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
        return tex;
    }

    void LateUpdate()
    {
        if (!built) return;

        if (player == null) FindPlayer();
        if (cam == null) cam = Camera.main;

        // 어둠 세기와 동그라미 크기를 부드럽게 바꿈
        float t = 1f - Mathf.Exp(-smoothSpeed * Time.unscaledDeltaTime);
        currentRadius = Mathf.Lerp(currentRadius, TargetRadius, t);
        currentAlpha = Mathf.Lerp(currentAlpha, darknessOn ? darkness : 0f, t);

        float W = Screen.width;
        float H = Screen.height;

        Vector2 pos = GetPlayerScreenPos();
        float r = currentRadius * H;

        int x0 = Mathf.RoundToInt(pos.x - r);
        int x1 = Mathf.RoundToInt(pos.x + r);
        int y0 = Mathf.RoundToInt(pos.y - r);
        int y1 = Mathf.RoundToInt(pos.y + r);

        int w = Mathf.RoundToInt(W);
        int h = Mathf.RoundToInt(H);

        int midX0 = Mathf.Max(0, x0);
        int midX1 = Mathf.Min(w, x1);
        int midW = Mathf.Max(0, midX1 - midX0);

        // 가운데 동그라미
        SetRect(center, x0, y0, x1 - x0, y1 - y0);

        // 동그라미 밖을 네 방향의 까만 판으로 채움
        SetRect(left, 0, 0, Mathf.Max(0, x0), h);
        SetRect(right, x1, 0, Mathf.Max(0, w - x1), h);
        SetRect(bottom, midX0, 0, midW, Mathf.Max(0, y0));
        SetRect(top, midX0, y1, midW, Mathf.Max(0, h - y1));

        // 색 / 투명도 적용
        center.color = new Color(1f, 1f, 1f, currentAlpha);
        Color solid = new Color(0f, 0f, 0f, currentAlpha);
        left.color = solid;
        right.color = solid;
        top.color = solid;
        bottom.color = solid;
    }

    void SetRect(RawImage img, int x, int y, int width, int height)
    {
        RectTransform rt = img.rectTransform;
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(width, height);
    }

    // 플레이어의 화면 위치(픽셀) 구하기
    Vector2 GetPlayerScreenPos()
    {
        if (player == null)
            return new Vector2(Screen.width / 2f, Screen.height / 2f);

        Canvas playerCanvas = player.GetComponentInParent<Canvas>();

        if (playerCanvas != null && playerCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return player.position;

        if (playerCanvas != null && playerCanvas.worldCamera != null)
            return playerCanvas.worldCamera.WorldToScreenPoint(player.position);

        if (cam != null)
            return cam.WorldToScreenPoint(player.position);

        return new Vector2(Screen.width / 2f, Screen.height / 2f);
    }

    void OnDestroy()
    {
        if (circleTexture != null)
            Destroy(circleTexture);
    }
}