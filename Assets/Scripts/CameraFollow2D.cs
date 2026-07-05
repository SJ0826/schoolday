using UnityEngine;

/// <summary>2D 카메라가 타깃(플레이어)을 부드럽게 따라간다. lockY면 세로는 고정(사이드뷰용).</summary>
public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public float smooth = 8f;
    public bool lockY = false;
    public float fixedY = 0f;

    void LateUpdate()
    {
        if (target == null) return;
        float gy = lockY ? fixedY : target.position.y;
        Vector3 goal = new Vector3(target.position.x, gy, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, goal, 1f - Mathf.Exp(-smooth * Time.deltaTime));
    }
}
