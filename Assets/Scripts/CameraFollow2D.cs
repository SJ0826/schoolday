using UnityEngine;

/// <summary>2D 탑다운 카메라가 타깃(플레이어)을 부드럽게 따라간다.</summary>
public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public float smooth = 8f;

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 goal = new Vector3(target.position.x, target.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, goal, 1f - Mathf.Exp(-smooth * Time.deltaTime));
    }
}
