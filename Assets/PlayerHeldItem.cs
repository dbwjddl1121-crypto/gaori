using UnityEngine;
using System;

public class PlayerHeldItem : MonoBehaviour
{
    [System.Serializable]
    public class ItemScaleOverride
    {
        public string spriteName;          // 스프라이트(아이템) 이름
        public float multiplier = 1f;      // 1보다 작으면 줄이고, 크면 키움
    }

    [Header("연결")]
    public QuickSlotBar quickSlotBar;
    public SpriteRenderer heldItemRenderer;
    public SpriteRenderer playerRenderer;   // Player 자신의 Sprite Renderer
    public Animator animator;

    [Header("아이템 크기 (면적 기준 자동 맞춤)")]
    public float referenceSize = 1.0f;    // 기준 크기 (유닛)
    public float referenceScale = 3.5f;   // 기준 배율
    [Tooltip("가장 긴 변이 이 비율(기준 크기 대비)을 넘지 않게 제한")]
    public float maxLongSideRatio = 1.3f;

    [Header("아이템별 크기 보정 (너무 큰 아이템만 등록)")]
    public ItemScaleOverride[] overrides;

    [Header("방향별 손 위치 (Player 기준 로컬 좌표)")]
    public Vector3 downPos = new Vector3(0.2f, -0.1f, 0f);
    public Vector3 upPos = new Vector3(0.1f, -0.1f, 0f);
    public Vector3 leftPos = new Vector3(-0.2f, -0.1f, 0f);
    public Vector3 rightPos = new Vector3(0.2f, -0.1f, 0f);

    [Header("정지 화면(Idle, 정면)일 때 손 위치")]
    public bool useSeparateIdlePos = false;   // 켜면 아래 Idle Pos를 사용, 끄면 Down Pos 사용
    public Vector3 idlePos = new Vector3(0.2f, -0.1f, 0f);

    [Header("방향별 회전 (Z)")]
    public float downRot = 0f;
    public float upRot = 0f;
    public float leftRot = 0f;
    public float rightRot = 0f;

    private Sprite currentSprite;

    // 현재 방향: 0=아래(정면), 1=위, 2=왼쪽, 3=오른쪽
    private int facing = 0;

    // 정지 화면(정면 Idle) 상태인지
    private bool isIdleFront = true;

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
        UpdateFacing();
        ApplyDirection();
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

        FitSize(newSprite);
    }

    void FitSize(Sprite sprite)
    {
        if (sprite == null) return;

        // 스프라이트의 크기 (Scale 1 기준, 단위: 유닛)
        Vector2 size = sprite.bounds.size;
        if (size.x <= 0f || size.y <= 0f) return;

        float longest = Mathf.Max(size.x, size.y);
        float areaSide = Mathf.Sqrt(size.x * size.y); // 면적 기준 한 변 길이

        // 목표 크기: 면적 기준으로 referenceSize * referenceScale
        float targetSide = referenceSize * referenceScale;
        float scale = targetSide / areaSide;

        // 가장 긴 변이 너무 길어지지 않게 제한
        float maxLong = targetSide * maxLongSideRatio;
        if (longest * scale > maxLong)
            scale = maxLong / longest;

        // 아이템별 보정
        scale *= GetMultiplier(sprite.name);

        heldItemRenderer.transform.localScale = new Vector3(scale, scale, 1f);
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

    void UpdateFacing()
    {
        if (animator == null) return;

        float x = animator.GetFloat("MoveX");
        float y = animator.GetFloat("MoveY");
        bool moving = animator.GetBool("IsMoving") || x != 0f || y != 0f;

        if (!moving)
        {
            // 위로 가다 멈춰서 뒷모습 정지 자세(IsCtrl)면 위쪽 처리, 아니면 정면
            if (animator.GetBool("IsCtrl"))
            {
                facing = 1;
                isIdleFront = false;
            }
            else
            {
                facing = 0;
                isIdleFront = true;
            }
            return;
        }

        isIdleFront = false;

        // 가로/세로 중 큰 쪽을 방향으로 사용
        if (Mathf.Abs(x) > Mathf.Abs(y))
            facing = (x > 0f) ? 3 : 2;
        else
            facing = (y > 0f) ? 1 : 0;
    }

    void ApplyDirection()
    {
        Transform t = heldItemRenderer.transform.parent; // HandAnchor
        int baseOrder = playerRenderer != null ? playerRenderer.sortingOrder : 0;

        switch (facing)
        {
            case 0: // 아래 (걷는 중이거나 정지 화면)
                t.localPosition = (isIdleFront && useSeparateIdlePos) ? idlePos : downPos;
                t.localEulerAngles = new Vector3(0, 0, downRot);
                heldItemRenderer.sortingOrder = baseOrder + 1;
                break;
            case 1: // 위 (걷는 중이거나 뒷모습 정지)
                t.localPosition = upPos;
                t.localEulerAngles = new Vector3(0, 0, upRot);
                heldItemRenderer.sortingOrder = baseOrder - 1; // 몸 뒤로
                break;
            case 2: // 왼쪽
                t.localPosition = leftPos;
                t.localEulerAngles = new Vector3(0, 0, leftRot);
                heldItemRenderer.sortingOrder = baseOrder - 1; // 몸 뒤로
                break;
            case 3: // 오른쪽
                t.localPosition = rightPos;
                t.localEulerAngles = new Vector3(0, 0, rightRot);
                heldItemRenderer.sortingOrder = baseOrder + 1;
                break;
        }
    }
}