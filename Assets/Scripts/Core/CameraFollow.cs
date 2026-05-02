using UnityEngine;

/// <summary>
/// Smooth camera that follows the player.
/// Attach to Main Camera.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform Target;        // Drag Player here

    [Header("Follow Settings")]
    public float SmoothSpeed = 5f;
    public Vector3 Offset = new Vector3(0, 2, -10);

    [Header("Lock Y (keeps ground at bottom)")]
    public bool LockY = false;
    public float LockedY = 0f;

    [Header("Bounds (optional)")]
    public bool UseBounds = false;
    public float MinX, MaxX, MinY, MaxY;

    void LateUpdate()
    {
        if (Target == null) return;

        Vector3 desired = Target.position + Offset;

        if (LockY)
            desired.y = LockedY;

        if (UseBounds)
        {
            desired.x = Mathf.Clamp(desired.x, MinX, MaxX);
            desired.y = Mathf.Clamp(desired.y, MinY, MaxY);
        }

        transform.position = Vector3.Lerp(transform.position, desired, SmoothSpeed * Time.deltaTime);
    }
}
