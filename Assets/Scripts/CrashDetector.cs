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


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>(); 
        // Tìm Surface Effector 2D trên ground
        surfaceEffector = GameObject.FindGameObjectWithTag("Ground")?.GetComponent<SurfaceEffector2D>();
        audioSource = GetComponent<AudioSource>();
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

            // Vô hiệu hóa điều khiển ngay lập tức
            if (playerController != null)
            {
                playerController.enabled = false;
            }

            // Tắt Surface Effector 2D
            if (surfaceEffector != null)
            {
                surfaceEffector.enabled = false;
            }

            // Dừng tất cả chuyển động
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            // Chuyển về màn hình EndGame ngay lập tức
            Invoke("LoadEndGameScene", delayBeforeReset);
        }
    }

    void LoadEndGameScene()
    {
       ScoreManager.instance.EndGame();
    }
}
