using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SnowRockCollision : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] AudioClip crashSound;
    [SerializeField] float crashVolumeMultiplier = 1.5f; // Hệ số tăng âm lượng cho tiếng va chạm đá
    
    [Header("Respawn Settings")]
    [SerializeField] Vector3 respawnPosition = new Vector3(-10f, 0f, 0f); // Vị trí hồi sinh mặc định
    [SerializeField] float respawnDelay = 1f; // Thời gian delay trước khi hồi sinh
    [SerializeField] bool usePlayerStartPosition = true; // Tự động sử dụng vị trí bắt đầu của player
    
    [Header("Effects")]
    [SerializeField] ParticleSystem crashEffect; // Hiệu ứng khi đụng đá
    [SerializeField] ParticleSystem respawnEffect; // Hiệu ứng khi hồi sinh
    [SerializeField] ScreenFlash screenFlash; // Hiệu ứng flash màn hình khi đụng đá
    
    private AudioSource audioSource;
    private bool hasCollided = false; // Để tránh trigger nhiều lần
    private Vector3 playerStartPosition; // Lưu vị trí bắt đầu của player
    private SpriteRenderer playerSprite; // Để làm hiệu ứng nhấp nháy
    private bool isRespawning = false; // Để tránh trigger trong quá trình hồi sinh

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        // Debug log để kiểm tra AudioSource
        if (audioSource != null)
        {
            Debug.Log("AudioSource đã được tìm thấy trên " + gameObject.name);
        }
        else
        {
            Debug.LogError("AudioSource không được tìm thấy trên " + gameObject.name + "! Hãy thêm AudioSource component.");
        }
        
        // Debug log để kiểm tra crashSound
        if (crashSound != null)
        {
            Debug.Log("Crash sound đã được gán: " + crashSound.name);
        }
        else
        {
            Debug.LogError("Crash sound chưa được gán trên " + gameObject.name + "!");
        }
        
        // Lưu vị trí bắt đầu của player nếu có
        if (usePlayerStartPosition)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerStartPosition = player.transform.position;
                playerSprite = player.GetComponent<SpriteRenderer>();
                Debug.Log("Đã lưu vị trí bắt đầu của player: " + playerStartPosition);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasCollided && !isRespawning)
        {
            hasCollided = true; // Đánh dấu đã va chạm để tránh trigger nhiều lần

            // Phát âm thanh crash nếu có
            if (audioSource != null && crashSound != null)
            {
                float effectVolume = PlayerPrefs.GetFloat("EffectVolume", 1f);
                float finalVolume = effectVolume * crashVolumeMultiplier; // Tăng âm lượng
                audioSource.PlayOneShot(crashSound, finalVolume);
                Debug.Log("Đã phát âm thanh crash với volume: " + finalVolume);
            }
            else
            {
                if (audioSource == null)
                {
                    Debug.LogError("AudioSource không tồn tại trên " + gameObject.name);
                }
                if (crashSound == null)
                {
                    Debug.LogError("Crash sound chưa được gán trên " + gameObject.name);
                }
            }

            // Hiệu ứng đụng đá
            PlayCrashEffects(other.gameObject);

            // Kiểm tra HealthManager
            if (HealthManager.instance != null)
            {
                // Trừ 1 mạng
                HealthManager.instance.LoseLife();
                
                // Kiểm tra xem còn mạng không
                if (HealthManager.instance.IsGameOver())
                {
                    // Hết mạng, dừng player ngay lập tức và chuyển về EndGame
                    Debug.Log("Hết mạng, dừng player và chuyển về EndGame");
                    StopPlayerImmediately(other.gameObject);
                    Invoke("LoadEndGameScene", 1f);
                }
                else
                {
                    // Còn mạng, hồi sinh tại vị trí bắt đầu
                    Debug.Log("Còn mạng, hồi sinh tại vị trí bắt đầu");
                    RespawnPlayer(other.gameObject);
                }
            }
            else
            {
                Debug.LogWarning("HealthManager không tồn tại, dừng player và chuyển về EndGame");
                StopPlayerImmediately(other.gameObject);
                Invoke("LoadEndGameScene", 1f);
            }
        }
    }

    void PlayCrashEffects(GameObject player)
    {
        // Hiệu ứng particle khi đụng đá
        if (crashEffect != null)
        {
            crashEffect.transform.position = player.transform.position;
            crashEffect.Play();
        }

        // Hiệu ứng flash màn hình
        if (screenFlash != null)
        {
            screenFlash.Flash();
        }

        // Hiệu ứng nhấp nháy player
        if (playerSprite != null)
        {
            StartCoroutine(PlayerBlinkEffect());
        }
    }

    System.Collections.IEnumerator PlayerBlinkEffect()
    {
        if (playerSprite == null) yield break;

        Color originalColor = playerSprite.color;
        int blinkCount = 0;
        int maxBlinks = 6; // Nhấp nháy 3 lần (6 lần thay đổi màu)

        while (blinkCount < maxBlinks)
        {
            playerSprite.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            playerSprite.color = originalColor;
            yield return new WaitForSeconds(0.1f);
            blinkCount++;
        }
    }

    void RespawnPlayer(GameObject player)
    {
        isRespawning = true;

        // Vô hiệu hóa PlayerController tạm thời
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Dừng tất cả chuyển động của player
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
            playerRb.isKinematic = true; // Tắt physics tạm thời
        }

        // Tắt Surface Effector 2D tạm thời
        SurfaceEffector2D surfaceEffector = GameObject.FindGameObjectWithTag("Ground")?.GetComponent<SurfaceEffector2D>();
        if (surfaceEffector != null)
        {
            surfaceEffector.enabled = false;
        }

        // Vô hiệu hóa CrashDetector tạm thời
        CrashDetector crashDetector = player.GetComponent<CrashDetector>();
        if (crashDetector != null)
        {
            crashDetector.enabled = false;
        }

        // Đợi một chút rồi hồi sinh
        Invoke("PerformRespawn", respawnDelay);
    }

    void PerformRespawn()
    {
        // Tìm player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Chọn vị trí hồi sinh
            Vector3 targetPosition = usePlayerStartPosition ? playerStartPosition : respawnPosition;
            
            // Hiệu ứng hồi sinh
            PlayRespawnEffects(player, targetPosition);
            
            // Đặt lại vị trí
            player.transform.position = targetPosition;
            
            // Bật lại PlayerController
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = true;
            }

            // Bật lại physics
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.isKinematic = false;
            }

            // Bật lại Surface Effector 2D
            SurfaceEffector2D surfaceEffector = GameObject.FindGameObjectWithTag("Ground")?.GetComponent<SurfaceEffector2D>();
            if (surfaceEffector != null)
            {
                surfaceEffector.enabled = true;
            }

            // Bật lại CrashDetector
            CrashDetector crashDetector = player.GetComponent<CrashDetector>();
            if (crashDetector != null)
            {
                crashDetector.enabled = true;
            }

            Debug.Log("Đã hồi sinh player tại vị trí: " + targetPosition);
        }

        // Reset flags
        hasCollided = false;
        isRespawning = false;
    }

    void PlayRespawnEffects(GameObject player, Vector3 position)
    {
        // Hiệu ứng particle khi hồi sinh
        if (respawnEffect != null)
        {
            respawnEffect.transform.position = position;
            respawnEffect.Play();
        }

        // Hiệu ứng nhấp nháy xanh khi hồi sinh
        if (playerSprite != null)
        {
            StartCoroutine(RespawnBlinkEffect());
        }
    }

    System.Collections.IEnumerator RespawnBlinkEffect()
    {
        if (playerSprite == null) yield break;

        Color originalColor = playerSprite.color;
        int blinkCount = 0;
        int maxBlinks = 4; // Nhấp nháy 2 lần (4 lần thay đổi màu)

        while (blinkCount < maxBlinks)
        {
            playerSprite.color = Color.green;
            yield return new WaitForSeconds(0.15f);
            playerSprite.color = originalColor;
            yield return new WaitForSeconds(0.15f);
            blinkCount++;
        }
    }

    void LoadEndGameScene()
    {
        ScoreManager.instance.EndGame();
    }

    void StopPlayerImmediately(GameObject player)
    {
        // Vô hiệu hóa PlayerController tạm thời
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Dừng tất cả chuyển động của player
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
            playerRb.isKinematic = true; // Tắt physics tạm thời
        }

        // Tắt Surface Effector 2D tạm thời
        SurfaceEffector2D surfaceEffector = GameObject.FindGameObjectWithTag("Ground")?.GetComponent<SurfaceEffector2D>();
        if (surfaceEffector != null)
        {
            surfaceEffector.enabled = false;
        }

        // Vô hiệu hóa CrashDetector tạm thời
        CrashDetector crashDetector = player.GetComponent<CrashDetector>();
        if (crashDetector != null)
        {
            crashDetector.enabled = false;
        }
    }
} 