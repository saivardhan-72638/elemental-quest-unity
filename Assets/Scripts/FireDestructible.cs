using UnityEngine;

// Add this component to an obstacle that can be removed by a Fire projectile.
public class FireDestructible : MonoBehaviour
{
    public void Burn()
    {
        Destroy(gameObject);
    }
}
