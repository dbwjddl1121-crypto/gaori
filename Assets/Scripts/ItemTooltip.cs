using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemTooltip : MonoBehaviour
{
    public static ItemTooltip Instance;

    [Header("툴팁 UI")]
    public GameObject tooltipPanel;        // TooltipPanel
    public Image iconImage;                // TooltipIcon
    public TextMeshProUGUI nameText;       // TooltipName
    public TextMeshProUGUI descText;       // TooltipDesc

    [Header("설정")]
    public Vector2 offset = new Vector2(20f, -20f);      // 마우스에서 떨어진 거리
    public Vector2 iconMaxSize = new Vector2(64f, 64f);  // 아이콘이 차지할 수 있는 최대 칸 크기

    private RectTransform panelRect;
    private RectTransform parentRect;
    private RectTransform iconRect;
    private Canvas canvas;

    void Awake()
    {
        Instance = this;

        if (iconImage != null)
        {
            iconRect = iconImage.rectTransform;
            iconImage.type = Image.Type.Simple;   // Sliced면 비율 유지가 안 되므로 강제
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
        }

        if (tooltipPanel != null)
        {
            panelRect = tooltipPanel.GetComponent<RectTransform>();
            parentRect = tooltipPanel.transform.parent as RectTransform;
            canvas = tooltipPanel.GetComponentInParent<Canvas>(true);
            tooltipPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (tooltipPanel == null || !tooltipPanel.activeSelf) return;

        // 인벤토리가 닫혔는데 툴팁만 켜져 있으면 끔
        if (!parentRect.gameObject.activeInHierarchy)
        {
            tooltipPanel.SetActive(false);
            return;
        }

        FollowMouse();
    }

    public void Show(Item item)
    {
        if (tooltipPanel == null || item == null) return;

        if (iconImage != null)
        {
            iconImage.sprite = item.itemIcon;
            FitIconToSprite(item.itemIcon);
        }
        if (nameText != null) nameText.text = item.itemName;
        if (descText != null) descText.text = ItemDescriptions.Get(item.itemName);

        tooltipPanel.SetActive(true);
        tooltipPanel.transform.SetAsLastSibling(); // 다른 칸 위에 그림
        FollowMouse();
    }

    public void Hide()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    // 스프라이트의 원래 가로세로 비율대로 아이콘 크기를 맞춤
    void FitIconToSprite(Sprite sprite)
    {
        if (iconRect == null || sprite == null) return;

        float w = sprite.rect.width;
        float h = sprite.rect.height;
        if (w <= 0f || h <= 0f) return;

        // 최대 칸 안에 비율을 유지한 채로 가장 크게 들어가는 크기
        float scale = Mathf.Min(iconMaxSize.x / w, iconMaxSize.y / h);
        float width = w * scale;
        float height = h * scale;

        // 부모 Scale이 가로세로 다르면 그만큼 보정 (화면에 보이는 비율을 원본과 같게)
        Vector3 ls = iconRect.lossyScale;
        if (ls.x > 0.0001f && ls.y > 0.0001f)
            width *= ls.y / ls.x;

        // 앵커가 어떤 모양이든 크기를 정확히 지정
        iconRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        iconRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }

    void FollowMouse()
    {
        if (panelRect == null || parentRect == null || canvas == null) return;

        // Overlay면 카메라 없이, 아니면 캔버스의 카메라로 계산
        Canvas root = canvas.rootCanvas;
        Camera cam = root.renderMode == RenderMode.ScreenSpaceOverlay ? null : root.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, Input.mousePosition, cam, out Vector2 local))
        {
            panelRect.localPosition = local + offset;
        }
    }
}