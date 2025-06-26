using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishLine : MonoBehaviour
{
    [SerializeField] float delay = 1.5f;
    [SerializeField] AudioClip finishSound;
    [SerializeField] ParticleSystem finishParticleEffect;
    private AudioSource audioSource;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Finish Line Reached!");
        
        // Kích hoạt particle effect nếu có
        if (finishParticleEffect != null)
        {
            finishParticleEffect.Play();
        }
        
        if (audioSource != null && finishSound != null)
        {
            float effectVolume = PlayerPrefs.GetFloat("EffectVolume", 1f);
            audioSource.PlayOneShot(finishSound, effectVolume);
        }
        Invoke("ReloadScene", delay);
    }

    void ReloadScene()
    {
        // Lưu tiến trình Time Trial trước khi chuyển scene
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.SaveTimeTrialProgress();
        }
        
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"ReloadScene called - Current scene: '{currentScene}'");
        
        if (currentScene == "Level1")
        {
            Debug.Log("Loading Level2...");
            SceneManager.LoadScene("Level2");
        }
        else if (currentScene == "Level2")
        {
            Debug.Log("Loading EndGame...");
            // Gọi EndGame để lưu điểm đúng cách thay vì load scene trực tiếp
            if (ScoreManager.instance != null)
            {
                ScoreManager.instance.EndGame();
            }
            else
            {
                SceneManager.LoadScene("EndGame");
            }
        }
        else
        {
            Debug.LogWarning($"Unknown scene name: '{currentScene}'. Cannot determine next scene.");
        }
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        // Đảm bảo finish line hiển thị trên cùng
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 100; // Đặt sorting order cao để hiển thị trên cùng
        }
        
        // Đảm bảo particle effect hiển thị trên cùng và ẩn ban đầu
        if (finishParticleEffect != null)
        {
            ParticleSystemRenderer particleRenderer = finishParticleEffect.GetComponent<ParticleSystemRenderer>();
            if (particleRenderer != null)
            {
                particleRenderer.sortingOrder = 101; // Đặt sorting order cao hơn finish line
            }
            
            // Ẩn particle effect ban đầu
            finishParticleEffect.Stop();
            finishParticleEffect.Clear();
        }
    }
}
