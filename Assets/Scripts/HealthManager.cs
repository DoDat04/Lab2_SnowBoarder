using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class HealthManager : MonoBehaviour
{
    public static HealthManager instance;
    public Image[] hearts;          // mảng 3 Image tim
    public Sprite fullHeart;        // sprite tim đầy
    public Sprite emptyHeart;       // sprite tim rỗng

    private int maxLives = 3;
    private int currentLives;
    private bool isInitialized = false;
    private GameObject heartUICanvas; // Lưu reference đến HealthManager Canvas

    void Awake()
    {
        Debug.Log("HealthManager Awake được gọi, GameObject: " + gameObject.name);
        
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            isInitialized = true;

            // Sử dụng reference trực tiếp đến GameObject hiện tại
            heartUICanvas = this.gameObject;
            Debug.Log("Đã set heartUICanvas = " + heartUICanvas.name);
            
            if (heartUICanvas != null)
            {
                // Ẩn HealthManager ban đầu, sẽ được hiện khi vào scene gameplay
                heartUICanvas.SetActive(false);
                Debug.Log("Đã ẩn HealthManager");
            }
        }
        else
        {
            Debug.Log("HealthManager instance đã tồn tại, destroy GameObject hiện tại");
            Destroy(gameObject);
            return;
        }
    }

    void OnDestroy()
    {
        if (isInitialized)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded được gọi với scene: " + scene.name);
        
        if (scene.name == "MainMenu" || scene.name == "EndGame")
        {
            Debug.Log("Scene là MainMenu hoặc EndGame, destroy HealthManager");
            Destroy(gameObject);
        }
        else
        {
            // Hiện HealthManager ở các scene gameplay (Level1, Gameplay, etc.), ẩn ở các scene khác
            if (scene.name == "Level1" || scene.name.StartsWith("Gameplay") || scene.name.StartsWith("Level"))
            {
                Debug.Log("Scene mới được load: " + scene.name + ", số mạng hiện tại: " + currentLives);
                // Hiện HealthManager và cập nhật
                if (heartUICanvas != null)
                {
                    Debug.Log("Hiện HealthManager và khởi tạo UI");
                    heartUICanvas.SetActive(true);
                    StartCoroutine(InitializeHeartsUI());
                }
                else
                {
                    Debug.LogError("heartUICanvas là null trong OnSceneLoaded!");
                }
            }
            else
            {
                Debug.Log("Scene không phải gameplay, ẩn HealthManager");
                // Ẩn HealthManager ở MainMenu và EndGame
                if (heartUICanvas != null)
                {
                    heartUICanvas.SetActive(false);
                }
            }
        }
    }

    private System.Collections.IEnumerator InitializeHeartsUI()
    {
        Debug.Log("InitializeHeartsUI được gọi");
        yield return null; // Đợi một frame
        yield return new WaitForSeconds(0.1f); // Đợi thêm một chút để đảm bảo mọi thứ được khởi tạo
        Debug.Log("Sau khi đợi, gọi FindAndUpdateHeartsUI");
        FindAndUpdateHeartsUI();
        Debug.Log("Sau khi tìm tim, gọi UpdateHeartsUI");
        UpdateHeartsUI();
    }

    void FindAndUpdateHeartsUI()
    {
        // Không tìm HealthManager ở MainMenu và EndGame
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "MainMenu" || currentScene.Contains("EndGame"))
        {
            return;
        }

        if (hearts != null && hearts.Length == maxLives)
        {
            // Kiểm tra xem các tim có còn tồn tại không
            bool allHeartsValid = true;
            foreach (Image heart in hearts)
            {
                if (heart == null)
                {
                    allHeartsValid = false;
                    break;
                }
            }
            if (allHeartsValid) return; // Nếu tất cả tim đều hợp lệ thì không cần tìm lại
        }

        // Reset mảng hearts
        hearts = new Image[maxLives];

        // Sử dụng heartUICanvas đã lưu thay vì tìm lại
        if (heartUICanvas != null)
        {
            Debug.Log("Tìm kiếm tim trong GameObject: " + heartUICanvas.name);
            
            // Thử tìm trực tiếp các child objects có tên chứa "Heart"
            Transform[] children = heartUICanvas.GetComponentsInChildren<Transform>(true);
            Debug.Log("Số lượng child objects: " + children.Length);
            
            List<Image> foundHearts = new List<Image>();

            foreach (Transform child in children)
            {
                Debug.Log("Child object: " + child.name);
                if (child.name.Contains("Heart"))
                {
                    Image heartImage = child.GetComponent<Image>();
                    if (heartImage != null)
                    {
                        Debug.Log("Tìm thấy tim: " + child.name);
                        foundHearts.Add(heartImage);
                    }
                    else
                    {
                        Debug.LogWarning("Child " + child.name + " không có component Image");
                    }
                }
            }

            Debug.Log("Tổng số tim tìm thấy: " + foundHearts.Count);

            if (foundHearts.Count >= maxLives)
            {
                foundHearts.Sort((a, b) => a.name.CompareTo(b.name));
                hearts = foundHearts.GetRange(0, maxLives).ToArray();
                Debug.Log("Đã tìm thấy và sắp xếp " + hearts.Length + " tim trong HealthManager");
                return;
            }
            else
            {
                Debug.LogWarning("Chỉ tìm thấy " + foundHearts.Count + " tim, cần " + maxLives + " tim");
            }
        }
        else
        {
            Debug.LogError("heartUICanvas là null!");
        }

        Debug.LogError("Không tìm thấy đủ " + maxLives + " Image tim trong HealthManager Canvas!");
    }

    void Start()
    {
        Debug.Log("HealthManager Start được gọi, isInitialized: " + isInitialized);
        
        if (!isInitialized) 
        {
            Debug.Log("HealthManager chưa được khởi tạo, return");
            return;
        }

        // Chỉ set lại số mạng nếu chưa có (lần đầu chơi)
        if (currentLives == 0)
        {
            Debug.Log("currentLives = 0, gọi ResetLives");
            ResetLives();
        }
        else
        {
            Debug.Log("currentLives = " + currentLives + ", cập nhật UI");
            // Đảm bảo UI được cập nhật với số mạng hiện tại
            FindAndUpdateHeartsUI();
            UpdateHeartsUI();
        }
    }

    public void ResetLives()
    {
        Debug.Log("ResetLives được gọi");
        currentLives = maxLives;
        Debug.Log("Đã set currentLives = " + currentLives);
        FindAndUpdateHeartsUI();
        UpdateHeartsUI();
        Debug.Log("Reset mạng sống về " + maxLives);
    }

    public void LoseLife()
    {
        if (currentLives > 0)
        {
            currentLives--;
            Debug.Log("Mất 1 mạng, còn lại: " + currentLives);

            // Đảm bảo hearts đã được tìm thấy trước khi cập nhật UI
            if (hearts == null || hearts.Length == 0)
            {
                FindAndUpdateHeartsUI();
            }
            UpdateHeartsUI();
        }
    }

    void UpdateHeartsUI()
    {
        // Không cập nhật UI nếu đang ở MainMenu
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            return;
        }

        if (hearts == null || hearts.Length == 0)
        {
            Debug.LogWarning("Không thể cập nhật UI tim vì mảng hearts trống!");
            return;
        }

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
            {
                hearts[i].sprite = (i < currentLives) ? fullHeart : emptyHeart;
                hearts[i].enabled = true; // Đảm bảo tim luôn hiển thị
                Debug.Log("Cập nhật tim " + i + ": " + (i < currentLives ? "đầy" : "rỗng") + " (số mạng: " + currentLives + ")");
            }
        }
    }

    public bool IsGameOver()
    {
        return currentLives <= 0;
    }
}
