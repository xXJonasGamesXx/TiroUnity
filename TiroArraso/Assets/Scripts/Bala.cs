using UnityEngine;

public class Bala : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        // Destroy bullet on any collision
        Destroy(gameObject);
    }
}