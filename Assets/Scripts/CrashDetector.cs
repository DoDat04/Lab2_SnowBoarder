using UnityEngine;
using UnityEngine.SceneManagement;

public class CrashDetector : MonoBehaviour
{
    [SerializeField] float delayBeforeReset = 1.5f;
    private Rigidbody2D rb;
    private PlayerController playerController; 
    private SurfaceEffector2D surfaceEffector; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>(); 
        // Tìm Surface Effector 2D trên ground
        surfaceEffector = GameObject.FindGameObjectWithTag("Ground")?.GetComponent<SurfaceEffector2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Ground")
        {
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
