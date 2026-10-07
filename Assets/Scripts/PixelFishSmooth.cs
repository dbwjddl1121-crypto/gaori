using UnityEngine;
using System.Collections;

public class PixelFishSmooth : MonoBehaviour
{
    [Header("처음 오른쪽으로 이동")]
    public bool startRight = true;

    [Header("원본 물고기가 오른쪽을 보고 있는지")]
    public bool spriteFacesRight = true;

    [Header("속도")]
    public float minSpeed = 1f;
    public float maxSpeed = 3f;

    [Header("이동 범위")]
    public float moveRange = 8f;

    [Header("둥실거림")]
    public float swimHeight = 0.2f;
    public float swimSpeed = 2f;

    [Header("잡기 설정")]
    public int hitsToCatch = 3;
    public int rewardCash = 50;
    public float fleeSpeedMultiplier = 4f; // 터치 시 도망 속도 배율
    public float fleeDuration = 1.2f;      // 도망치는 시간
    public Color hitColor = Color.red;

    [Header("잡을 수 있는 거리 (화면 픽셀 기준)")]
    public float catchDistancePixels = 300f;

    private float speed;
    private int direction;
    private Vector3 startPos;
    private float swimOffset;

    private int hitCount = 0;
    private bool isFleeing = false;
    private bool isCaught = false;
    private SpriteRenderer sr;
    private Color originalColor;
    private Transform player;
    private Camera cam;
    private Collider2D col;

    void Start()
    {
        cam = Camera.main;
        col = GetComponent<Collider2D>();

        PlayerMovement pm = FindFirstObjectByType<PlayerMovement>();
        if (pm != null)
            player = pm.transform;
        else
            Debug.Log("PlayerMovement를 찾지 못했어요!");

        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;

        startPos = transform.position;
        speed = Random.Range(minSpeed, maxSpeed);
        direction = startRight ? 1 : -1;
        swimOffset = Random.Range(0f, 10f);

        float size = Random.Range(0.8f, 1.3f);
        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x) * size,
            transform.localScale.y * size,
            transform.localScale.z * size
        );

        Flip();
    }

    void Update()
    {
        if (isCaught) return;

        // 우클릭 감지 (1 = 우클릭)
        if (Input.GetMouseButtonDown(1))
        {
            CheckRightClick();
        }

        float currentSpeed = isFleeing ? speed * fleeSpeedMultiplier : speed;

        transform.Translate(Vector3.right * direction * currentSpeed * Time.deltaTime);

        float y = Mathf.Sin(Time.time * swimSpeed + swimOffset) * swimHeight;
        transform.position = new Vector3(
            transform.position.x,
            startPos.y + y,
            transform.position.z
        );

        if (transform.position.x > startPos.x + moveRange)
        {
            direction = -1;
            Flip();
        }

        if (transform.position.x < startPos.x - moveRange)
        {
            direction = 1;
            Flip();
        }
    }

    // 우클릭한 위치에 이 물고기가 있는지 확인
    void CheckRightClick()
    {
        if (cam == null || col == null) return;

        Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);

        if (col.OverlapPoint(mouseWorld))
        {
            TryHit();
        }
    }

    Vector2 GetPlayerScreenPos()
    {
        Canvas canvas = player.GetComponentInParent<Canvas>();

        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return player.position;
        else if (canvas != null && canvas.worldCamera != null)
            return canvas.worldCamera.WorldToScreenPoint(player.position);
        else
            return cam.WorldToScreenPoint(player.position);
    }

    float GetScreenDistance()
    {
        Vector2 fishScreen = cam.WorldToScreenPoint(transform.position);
        return Vector2.Distance(fishScreen, GetPlayerScreenPos());
    }

    // 기존 OnMouseDown 내용을 옮겨온 함수
    void TryHit()
    {
        if (isCaught) return;

        if (player != null && cam != null)
        {
            float dist = GetScreenDistance();
            Debug.Log("화면 거리: " + dist + " (허용: " + catchDistancePixels + ")");

            if (dist > catchDistancePixels)
            {
                Debug.Log("너무 멀어요!");
                return;
            }
        }

        hitCount++;

        if (hitCount >= hitsToCatch)
        {
            Catch();
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(HitReaction());
        }
    }

    IEnumerator HitReaction()
    {
        isFleeing = true;

        // 플레이어 반대 방향으로 도망
        if (player != null && cam != null)
        {
            Vector2 fishScreen = cam.WorldToScreenPoint(transform.position);
            Vector2 playerScreen = GetPlayerScreenPos();

            direction = (fishScreen.x >= playerScreen.x) ? 1 : -1;
            Flip();
        }

        float t = 0f;
        Color target = Color.Lerp(originalColor, hitColor, (float)hitCount / hitsToCatch);

        if (sr != null) sr.color = hitColor;

        while (t < fleeDuration)
        {
            t += Time.deltaTime;
            if (sr != null)
                sr.color = Color.Lerp(hitColor, target, t / fleeDuration);
            yield return null;
        }

        isFleeing = false;
    }

    void Catch()
    {
        isCaught = true;
        StopAllCoroutines();

        RunStats.fishCaught++;              // 추가
        RunStats.cashEarned += rewardCash;  // 추가

        if (CashManager.Instance != null)
            CashManager.Instance.AddCash(rewardCash);

        StartCoroutine(CatchEffect());
    }

    IEnumerator CatchEffect()
    {
        Vector3 startScale = transform.localScale;
        float t = 0f;
        float duration = 0.3f;

        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t / duration);
            yield return null;
        }

        Destroy(gameObject);
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;

        if (spriteFacesRight)
            scale.x = Mathf.Abs(scale.x) * direction;
        else
            scale.x = -Mathf.Abs(scale.x) * direction;

        transform.localScale = scale;
    }
}