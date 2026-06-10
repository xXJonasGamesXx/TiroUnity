using UnityEngine;

public class Target : MonoBehaviour
{
    [HideInInspector] public TargetSpawner.SpawnPoint spawnPoint;

    [HideInInspector] public bool moveHorizontal = false;
    [HideInInspector] public bool moveVertical = false;
    [HideInInspector] public float moveSpeed = 3f;
    [HideInInspector] public float moveRange = 5f;
    [HideInInspector] public int health = 1;
    [HideInInspector] public int pointsValue = 10;

    private Vector3 startPosition;
    private float directionX = 1f;
    private float directionY = 1f;
    
    // Conectado ao seu GameManager em vez de FPSAimController
    private GameManager gameManager; 

    void Start()
    {
        startPosition = transform.position;
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
{
    // Movimento Suave de um lado para o outro (Horizontal)
    Vector3 newPos = transform.position;

    if (moveHorizontal)
    {
        newPos.x += directionX * moveSpeed * Time.deltaTime;
        if (Mathf.Abs(newPos.x - startPosition.x) >= moveRange)
        {
            directionX *= -1; // Inverte a direção sem rotacionar o modelo
        }
    }

    if (moveVertical)
    {
        newPos.y += directionY * moveSpeed * Time.deltaTime;
        if (Mathf.Abs(newPos.y - startPosition.y) >= moveRange)
        {
            directionY *= -1;
        }
    }

    transform.position = newPos;

    // A LINHA DE ROTAÇÃO FOI REMOVIDA DAQUI PARA ELE FICAR ESTÁTICO!
}

    // O segredo para não "bugar": Usando Trigger!
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bala")) 
        {
            health--;

            if (health <= 0)
            {
                if (gameManager != null)
                    gameManager.AdicionarScore(pointsValue);
                
                // Destrói a bala e o alvo
                Destroy(other.gameObject);
                Destroy(gameObject);
            }
            else
            {
                // Destrói só a bala se o alvo ainda tiver vida
                Destroy(other.gameObject);
            }
        }
    }
}