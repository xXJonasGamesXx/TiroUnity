using UnityEngine;

public class bullet : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        // Destroy bullet on any collision
        Destroy(gameObject);
    }
}