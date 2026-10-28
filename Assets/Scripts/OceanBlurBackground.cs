using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class OceanBlurBackground : MonoBehaviour
{
    [Header("연결")]
    [Tooltip("움직이는 바다가 나오는 RawImage (OceanPreviewImage)")]
    [SerializeField] RawImage source;

    [Header("설정")]
    [Tooltip("작을수록 더 흐려져요 (예: 32, 64, 128)")]
    [SerializeField] int blurWidth = 64;

    [Tooltip("몇 프레임마다 갱신할지 (1 = 매 프레임)")]
    [SerializeField] int updateEvery = 2;

    RawImage target;
    RenderTexture blurRT;
    int frameCount;

    void Awake()
    {
        target = GetComponent<RawImage>();
    }

    void LateUpdate()
    {
        if (source == null || source.texture == null)
            return;

        frameCount++;
        if (updateEvery > 1 && frameCount % updateEvery != 0)
            return;

        Texture src = source.texture;

        int finalW = Mathf.Max(8, blurWidth);
        int finalH = Mathf.Max(8, Mathf.RoundToInt(finalW * (float)src.height / src.width));

        if (blurRT == null || blurRT.width != finalW || blurRT.height != finalH)
        {
            if (blurRT != null)
                blurRT.Release();

            blurRT = new RenderTexture(finalW, finalH, 0);
            blurRT.filterMode = FilterMode.Bilinear;
            target.texture = blurRT;
        }

        // 한 번에 확 줄이면 깜빡이니까, 반씩 여러 번 줄여요
        Texture current = src;
        int w = src.width;
        int h = src.height;
        RenderTexture lastTemp = null;

        while (w / 2 >= finalW)
        {
            w /= 2;
            h /= 2;

            RenderTexture tmp = RenderTexture.GetTemporary(w, h, 0);
            tmp.filterMode = FilterMode.Bilinear;
            Graphics.Blit(current, tmp);

            if (lastTemp != null)
                RenderTexture.ReleaseTemporary(lastTemp);

            lastTemp = tmp;
            current = tmp;
        }

        Graphics.Blit(current, blurRT);

        if (lastTemp != null)
            RenderTexture.ReleaseTemporary(lastTemp);
    }

    void OnDestroy()
    {
        if (blurRT != null)
            blurRT.Release();
    }
}