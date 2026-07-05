using UnityEngine;

/// <summary>
/// 2D 카메라가 타깃(플레이어)을 부드럽게 따라간다.
/// lockY면 세로 고정(사이드뷰). clampToWorld면 배경 좌우 경계 밖으로 안 나간다(검정 방지).
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public float smooth = 8f;
    public bool lockY = false;
    public float fixedY = 0f;
    public bool clampToWorld = false;
    public float worldLeft = 0f;
    public float worldRight = 0f;

    Camera cam;

    void Awake() { cam = GetComponent<Camera>(); }

    void LateUpdate()
    {
        if (target == null) return;

        float gx = target.position.x;
        if (clampToWorld && cam != null && cam.orthographic)
        {
            float halfW = cam.orthographicSize * cam.aspect;   // 화면 가로 절반(월드 단위)
            float lo = worldLeft + halfW;
            float hi = worldRight - halfW;
            gx = (lo <= hi) ? Mathf.Clamp(gx, lo, hi) : (worldLeft + worldRight) * 0.5f;
        }

        float gy = lockY ? fixedY : target.position.y;
        Vector3 goal = new Vector3(gx, gy, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, goal, 1f - Mathf.Exp(-smooth * Time.deltaTime));
    }
}
