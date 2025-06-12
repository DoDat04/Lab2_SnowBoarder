using UnityEngine;
using UnityEngine.SceneManagement;

public class CrashDetector : MonoBehaviour
{
    [SerializeField] float delayBeforeReset = 1.5f;
    [SerializeField] AudioClip crashSound;
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
                audioSource.PlayOneShot(crashSound, effectVolume);
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

            // Đợi một khoảng thời gian rồi reset scene
            Invoke("ReloadScene", delayBeforeReset);
        }
    }

    void ReloadScene()
    {
        // Bật lại Surface Effector 2D trước khi reset scene
        if (surfaceEffector != null)
        {
            surfaceEffector.enabled = true;
        }
        SceneManager.LoadScene(0);
    }
}
