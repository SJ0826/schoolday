using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float runSpeed = 7f;
    public float lookSensitivity = 0.1f;
    public float gravity = -9.81f;

    // 오프닝·대사 연출 중 이동/시점을 잠글 때 false 로 둔다.
    [HideInInspector] public bool controlEnabled = true;

    CharacterController controller;
    Camera cam;
    float pitch = 0f;
    Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();
        LockCursor(true);
    }

    /// <summary>커서 잠금/해제. 연출·메뉴에서 호출.</summary>
    public void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    void Update()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        if (kb == null || mouse == null) return;

        if (controlEnabled)
        {
            // 마우스로 둘러보기
            Vector2 look = mouse.delta.ReadValue() * lookSensitivity;
            transform.Rotate(Vector3.up, look.x);
            pitch = Mathf.Clamp(pitch - look.y, -85f, 85f);
            if (cam != null) cam.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

            // WASD 이동 (+ Shift 달리기)
            float x = 0f, z = 0f;
            if (kb.aKey.isPressed) x -= 1f;
            if (kb.dKey.isPressed) x += 1f;
            if (kb.wKey.isPressed) z += 1f;
            if (kb.sKey.isPressed) z -= 1f;

            Vector3 move = transform.right * x + transform.forward * z;
            if (move.sqrMagnitude > 1f) move.Normalize();   // 대각선이 더 빠른 문제 보정
            float speed = kb.leftShiftKey.isPressed ? runSpeed : moveSpeed;
            controller.Move(move * speed * Time.deltaTime);
        }

        // 중력 (바닥에 붙어있게) — 잠겨 있어도 항상 적용
        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
