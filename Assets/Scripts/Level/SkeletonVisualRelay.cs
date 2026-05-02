using UnityEngine;

public class SkeletonVisualRelay : MonoBehaviour
{
    private SkeletonArcher archer;

    void Awake()
    {
        archer = GetComponentInParent<SkeletonArcher>();
    }

    public void SpawnProjectile()
    {
        archer?.SpawnProjectile();
    }
}
