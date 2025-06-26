using UnityEngine;
using UnityEngine.SceneManagement;

public class CrashDetector : MonoBehaviour
{
    [SerializeField] float delayBeforeReset = 0.5f;
    [SerializeField] AudioClip crashSound;
    [SerializeField] float crashVolumeMultiplier = 1.5f; // Hệ số tăng âm lượng cho tiếng va chạm đất
    private Rigidbody2D rb;
    private PlayerController playerController; 
    private SurfaceEffector2D surfaceEffector; 
    private AudioSource audioSource;
    private Vector3 playerStartPosition; // Lưu vị trí bắt đầu của player

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>(); 
        // Tìm Surface Effector 2D trên ground
        surfaceEffector = GameObject.FindGameObjectWithTag("Ground")?.GetComponent<SurfaceEffector2D>();
        audioSource = GetComponent<AudioSource>();
        
        // Lưu vị trí bắt đầu của player
        playerStartPosition = transform.position;
        Debug.Log("Đã lưu vị trí bắt đầu của player: " + playerStartPosition);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Ground")
        {
            // Phát âm thanh crash nếu có
            if (audioSource != null && crashSound != null)
            {
                float effectVolume = PlayerPrefs.GetFloat("EffectVolume", 1f);
                float finalVolume = effectVolume * crashVolumeMultiplier; // Tăng âm lượng
                audioSource.PlayOneShot(crashSound, finalVolume);
                Debug.Log("Đã phát âm thanh va chạm đất với volume: " + finalVolume);
            }

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
                    StopPlayerImmediately();
                    Invoke("LoadEndGameScene", delayBeforeReset);
                }
                else
                {
                    // Còn mạng, hồi sinh tại vị trí bắt đầu sau 1.5 giây
                    Debug.Log("Còn mạng, hồi sinh tại vị trí bắt đầu sau 1.5 giây");
                    StopPlayerImmediately();
                    Invoke("RespawnPlayer", 1.5f);
                }
            }
            else
            {
                Debug.LogWarning("HealthManager không tồn tại, dừng player và chuyển về EndGame");
                StopPlayerImmediately();
                Invoke("LoadEndGameScene", delayBeforeReset);
            }
        }
    }

    void StopPlayerImmediately()
    {
        // Vô hiệu hóa PlayerController tạm thời
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Dừng tất cả chuyển động của player
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true; // Tắt physics tạm thời
        }

        // Tắt Surface Effector 2D tạm thời
        if (surfaceEffector != null)
        {
            surfaceEffector.enabled = false;
        }
    }

    void RespawnPlayer()
    {
        // Sử dụng vị trí bắt đầu của player
        Vector3 respawnPosition = playerStartPosition;
        
        // Tìm player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Đặt lại vị trí
            player.transform.position = respawnPosition;
            
            // Reset rotation về 0 để tránh tư thế đụng đầu
            player.transform.rotation = Quaternion.identity;
            
            // Bật lại PlayerController
            if (playerController != null)
            {
                playerController.enabled = true;
            }

            // Bật lại physics
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.rotation = 0f; // Reset rotation của Rigidbody2D
            }

            // Bật lại Surface Effector 2D
            if (surfaceEffector != null)
            {
                surfaceEffector.enabled = true;
            }

            Debug.Log("Đã hồi sinh player tại vị trí: " + respawnPosition + " với rotation: 0");
        }
    }

    void LoadEndGameScene()
    {
       ScoreManager.instance.EndGame();
    }
}
