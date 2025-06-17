using UnityEngine;
using UnityEngine.SceneManagement;

public class SnowRockCollision : MonoBehaviour
{
    [SerializeField] AudioClip crashSound;
    private AudioSource audioSource;
    private bool hasCollided = false; // Để tránh trigger nhiều lần

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasCollided)
        {
            hasCollided = true; // Đánh dấu đã va chạm để tránh trigger nhiều lần

            // Phát âm thanh crash nếu có
            if (audioSource != null && crashSound != null)
            {
                float effectVolume = PlayerPrefs.GetFloat("EffectVolume", 1f);
                audioSource.PlayOneShot(crashSound, effectVolume);
            }

            // Vô hiệu hóa PlayerController ngay lập tức
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }

            // Dừng tất cả chuyển động của player ngay lập tức
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
                playerRb.angularVelocity = 0f;
                playerRb.isKinematic = true; // Tắt physics để tránh bị văng
            }

            // Tắt Surface Effector 2D nếu có
            SurfaceEffector2D surfaceEffector = GameObject.FindGameObjectWithTag("Ground")?.GetComponent<SurfaceEffector2D>();
            if (surfaceEffector != null)
            {
                surfaceEffector.enabled = false;
            }

            // Vô hiệu hóa CrashDetector để tránh conflict
            CrashDetector crashDetector = other.GetComponent<CrashDetector>();
            if (crashDetector != null)
            {
                crashDetector.enabled = false;
            }

            // Chuyển về màn hình end game ngay lập tức
            Invoke("LoadEndGameScene", 0.5f);
        }
    }

    void LoadEndGameScene()
    {
        SceneManager.LoadScene("EndGame");
    }
} 