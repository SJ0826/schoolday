using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 2D 탑다운 플레이어 이동. WASD/방향키로 상하좌우, Rigidbody2D로 벽 충돌.
/// 연출 중에는 controlEnabled=false 로 잠근다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    public float moveSpeed = 4f;
    [HideInInspector] public bool controlEnabled = true;

    Rigidbody2D rb;
    Vector2 input;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y -= 1f;
        input = new Vector2(x, y).normalized;
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + input * moveSpeed * Time.fixedDeltaTime);
    }
}
