using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class FPSAimController : MonoBehaviour
{
    [Header("Mouse Look")]
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    public bool lockCursor = true;
    
    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 50f;
    public float fireRate = 0.2f;
    public int maxAmmo = 30;
    public float reloadTime = 1.5f;
    public float maxShootDistance = 100f;
    
    [Header("UI")]
    public Image crosshair;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gameOverText;
    public UnityEngine.UI.Button restartButton;
    
    [Header("Game Settings")]
    public int scorePerHit = 10;
    public float gameDuration = 60f;
    public LayerMask targetLayer;
    
    // Mouse look variables
    private float xRotation = 0f;
    
    // Shooting variables
    private int currentAmmo;
    private int currentScore;
    private float nextFireTime;
    private bool isReloading = false;
    private float gameTimer;
    private bool isGameActive = true;
    private Camera playerCamera;
    
    void Start()
    {
        currentAmmo = maxAmmo;
        currentScore = 0;
        gameTimer = gameDuration;
        playerCamera = Camera.main;
        
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        UpdateUI();
        
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        
        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);
    }
    
    void Update()
    {
        if (!isGameActive) return;
        
        // Handle mouse look (camera rotation)
        HandleMouseLook();
        
        // Update crosshair color based on target
        UpdateCrosshairFeedback();
        
        // Game timer
        gameTimer -= Time.deltaTime;
        if (gameTimer <= 0)
        {
            EndGame();
            return;
        }
        
        // Shooting
        if (Input.GetButtonDown("Fire1") && !isReloading && currentAmmo > 0 && Time.time >= nextFireTime)
        {
            Shoot();
        }
        
        // Reload
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
        }
        
        // Toggle cursor lock with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            lockCursor = false;
        }
        
        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            lockCursor = true;
        }
        
        // Update UI
        if (ammoText != null)
        {
            if (isReloading)
                ammoText.text = "RELOADING...";
            else
                ammoText.text = $"Ammo: {currentAmmo}/{maxAmmo}\nTime: {Mathf.CeilToInt(gameTimer)}s";
        }
    }
    
    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
        // Rotate camera up/down
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
        // Rotate player body left/right
        if (playerBody != null)
            playerBody.Rotate(Vector3.up * mouseX);
        else
            transform.Rotate(Vector3.up * mouseX);
    }
    
    void UpdateCrosshairFeedback()
    {
        if (crosshair != null)
        {
            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            if (Physics.Raycast(ray, out RaycastHit hit, maxShootDistance, targetLayer))
            {
                crosshair.color = Color.red; // Turn red when aiming at target
            }
            else
            {
                crosshair.color = Color.white; // Default white
            }
        }
    }
    
    void Shoot()
    {
        nextFireTime = Time.time + fireRate;
        currentAmmo--;
        
        // Get the exact point where the crosshair is aiming
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        Vector3 targetPoint;
        
        if (Physics.Raycast(ray, out RaycastHit hit, maxShootDistance, targetLayer))
        {
            // Crosshair is pointing at a target
            targetPoint = hit.point;
            
            // Optional: Instant hit detection (uncomment if you want instant damage)
            // Target target = hit.collider.GetComponent<Target>();
            // if (target != null)
            // {
            //     target.TakeDamage(1);
            //     AddScore(scorePerHit);
            // }
        }
        else
        {
            // Crosshair is pointing at empty space - shoot at max distance
            targetPoint = ray.GetPoint(maxShootDistance);
        }
        
        // Calculate direction from fire point to target point
        Vector3 shootDirection = (targetPoint - firePoint.position).normalized;
        
        // Create and shoot bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(shootDirection));
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            rb.linearVelocity = shootDirection * bulletSpeed;
        }
        
        // Optional: Draw debug line to see trajectory
        Debug.DrawLine(firePoint.position, targetPoint, Color.red, 1f);
        
        // Clean up bullet
        Destroy(bullet, 5f);
        
        UpdateUI();
        
        // Flash crosshair on shoot
        if (crosshair != null)
            StartCoroutine(FlashCrosshair());
    }
    
    IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
        UpdateUI();
    }
    
    IEnumerator FlashCrosshair()
    {
        Color originalColor = crosshair.color;
        crosshair.color = Color.yellow;
        yield return new WaitForSeconds(0.05f);
        crosshair.color = originalColor;
    }
    
    public void AddScore(int points)
    {
        currentScore += points;
        UpdateUI();
    }
    
    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {currentScore}";
    }
    
    void EndGame()
    {
        isGameActive = false;
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = $"Game Over!\nFinal Score: {currentScore}\nPress Restart";
        }
        if (restartButton != null)
            restartButton.gameObject.SetActive(true);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    void RestartGame()
    {
        currentScore = 0;
        currentAmmo = maxAmmo;
        gameTimer = gameDuration;
        isGameActive = true;
        isReloading = false;
        
        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);
        if (restartButton != null)
            restartButton.gameObject.SetActive(false);
        
        UpdateUI();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        lockCursor = true;
        xRotation = 0f;
    }
}