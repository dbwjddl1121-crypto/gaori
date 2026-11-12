using UnityEngine;

public class SeaHeldItem : MonoBehaviour
{
    [System.Serializable]
    public class ItemScaleOverride
    {
        public string spriteName;          // 스프라이트(아이템) 이름
        public float multiplier = 1f;      // 1보다 작으면 줄이고, 크면 키움
    }

    [Header("연결")]
    public QuickSlotBar quickSlotBar;
    public SpriteRenderer heldItemRenderer;   // HeldItem의 Sprite Renderer
    public SpriteRenderer playerRenderer;     // 비워 두면 자동
    public Animator animator;                 // 비워 두면 자동

    [Header("아이템 크기 (면적 기준 자동 맞춤)")]
    public float referenceSize = 1.0f;
    public float referenceScale = 3.5f;
    [Tooltip("가장 긴 변이 이 비율(기준 크기 대비)을 넘지 않게 제한")]
    public float maxLongSideRatio = 1.3f;

    [Header("아이템별 크기 보정 (너무 큰 아이템만 등록)")]
    public ItemScaleOverride[] overrides;

    [Header("손 위치 (Player 기준 로컬 좌표)")]
    public Vector3 idlePos = new Vector3(0.2f, -0.1f, 0f);   // 가만히 있을 때
    public Vector3 swimPos = new Vector3(0.2f, -0.1f, 0f);   // 수영 중일 때
    public float rotationZ = 0f;

    [Header("옵션")]
    [Tooltip("켜면 캐릭터가 왼쪽을 볼 때도 아이템 그림이 뒤집히지 않음")]
    public bool keepItemUnflipped = true;
    [Tooltip("켜면 아이템이 몸 뒤에 그려짐")]
    public bool behindBody = false;

    private Sprite currentSprite;
    private float baseItemScale = 1f;

    void Start()
    {
        if (heldItemRenderer != null)
            heldItemRenderer.enabled = false;

        if (animator == null) animator = GetComponent<Animator>();
        if (playerRenderer == null) playerRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (heldItemRenderer == null) return;

        UpdateSprite();
        ApplyPose();
    }

    void UpdateSprite()
    {
        if (quickSlotBar == null) return;

        Item item = quickSlotBar.GetSelectedItem();
        Sprite newSprite = (item != null) ? item.itemIcon : null;

        if (newSprite == currentSprite) return;

        currentSprite = newSprite;
        heldItemRenderer.sprite = newSprite;
        heldItemRenderer.enabled = (newSprite != null);

        CalcSize(newSprite);
    }

    void CalcSize(Sprite sprite)
    {
        if (sprite == null) return;

        Vector2 size = sprite.bounds.size;
        if (size.x <= 0f || size.y <= 0f) return;

        float longest = Mathf.Max(size.x, size.y);
        float areaSide = Mathf.Sqrt(size.x * size.y);

        float targetSide = referenceSize * referenceScale;
        float scale = targetSide / areaSide;

        float maxLong = targetSide * maxLongSideRatio;
        if (longest * scale > maxLong)
            scale = maxLong / longest;

        scale *= GetMultiplier(sprite.name);

        baseItemScale = scale;
    }

    float GetMultiplier(string spriteName)
    {
        if (overrides == null) return 1f;

        foreach (ItemScaleOverride o in overrides)
        {
            if (o != null && o.spriteName == spriteName)
                return o.multiplier;
        }

        return 1f;
    }

    void ApplyPose()
    {
        Transform hand = heldItemRenderer.transform.parent; // HandAnchor

        bool moving = animator != null && animator.GetBool("IsMoving");
        hand.localPosition = moving ? swimPos : idlePos;
        hand.localEulerAngles = new Vector3(0, 0, rotationZ);

        // Player가 왼쪽을 볼 때(Scale X가 음수) 아이템 그림이 뒤집히지 않게 보정
        float flip = (keepItemUnflipped && transform.localScale.x < 0f) ? -1f : 1f;
        heldItemRenderer.transform.localScale =
            new Vector3(baseItemScale * flip, baseItemScale, 1f);

        int baseOrder = playerRenderer != null ? playerRenderer.sortingOrder : 0;
        heldItemRenderer.sortingOrder = behindBody ? baseOrder - 1 : baseOrder + 1;
    }
}