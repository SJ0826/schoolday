using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float lookSensitivity = 0.1f;
    public float gravity = -9.81f;

    CharacterController controller;
    Camera cam;
    float pitch = 0f;
    Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;  // 마우스 화면 중앙 고정
        Cursor.visible = false;
    }

    void Update()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        if (kb == null || mouse == null) return;

        // 마우스로 둘러보기
        Vector2 look = mouse.delta.ReadValue() * lookSensitivity;
        transform.Rotate(Vector3.up, look.x);
        pitch = Mathf.Clamp(pitch - look.y, -85f, 85f);
        if (cam != null) cam.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // WASD 이동
        float x = 0f, z = 0f;
        if (kb.aKey.isPressed) x -= 1f;
        if (kb.dKey.isPressed) x += 1f;
        if (kb.wKey.isPressed) z += 1f;
        if (kb.sKey.isPressed) z -= 1f;
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // 중력 (바닥에 붙어있게)
        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}