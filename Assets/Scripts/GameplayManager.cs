using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance { get; private set; } // Fix: Change type to GameplayManager

    [SerializeField] private GameObject healthManagerPrefab;
    [SerializeField] private GameObject scoreManagerPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManagers();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InitializeManagers()
    {
        // Khởi tạo HealthManager nếu chưa tồn tại
        if (HealthManager.instance == null)
        {
            if (healthManagerPrefab != null)
            {
                Instantiate(healthManagerPrefab);
            }
            else
            {
                Debug.LogError("HealthManager Prefab chưa được gán trong GameManager!");
            }
        }

        // Khởi tạo ScoreManager nếu chưa tồn tại
        if (ScoreManager.instance == null)
        {
            if (scoreManagerPrefab != null)
            {
                Instantiate(scoreManagerPrefab);
            }
            else
            {
                Debug.LogError("ScoreManager Prefab chưa được gán trong GameManager!");
            }
        }
    }

    public void ResetGame()
    {
        if (HealthManager.instance != null)
        {
            HealthManager.instance.ResetLives();
        }
        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.ResetScore();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu" || scene.name == "EndGame")
        {
            Destroy(gameObject);
        }
    }
}
