using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 2D 플레이어 이동. sideView=true 면 좌우 전용(옆에서 보는 시점, 캐릭터가 진행방향으로 뒤집힘).
/// sideView=false 면 8방향(탑다운). 연출 중에는 controlEnabled=false 로 잠근다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    public float moveSpeed = 4f;
    public bool sideView = true;
    [HideInInspector] public bool controlEnabled = true;

    Rigidbody2D rb;
    SpriteRenderer sr;
    Vector2 input;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Update()
    {
        if (!controlEnabled) { input = Vector2.zero; return; }

        var kb = Keyboard.current;
        if (kb == null) { input = Vector2.zero; return; }

        float x = 0f, y = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
        if (!sideView)
        {
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y -= 1f;
        }
        input = new Vector2(x, y).normalized;

        // 진행 방향으로 캐릭터 뒤집기 (사이드뷰)
        if (sr != null && Mathf.Abs(x) > 0.01f) sr.flipX = x < 0f;
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + input * moveSpeed * Time.fixedDeltaTime);
    }
}
