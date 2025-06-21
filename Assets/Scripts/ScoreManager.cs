using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using UnityEngine.Networking;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    public TextMeshProUGUI scoreText;
    public TMP_InputField nameInputField;

    int score = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Debug.Log("Lưu tại: " + Application.persistentDataPath);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            Destroy(gameObject);
            return;
        }

        // Tự động tìm lại Text hiển thị điểm nếu chưa được gán
        if (scoreText == null)
        {
            GameObject foundScoreText = GameObject.FindGameObjectWithTag("Score");
            if (foundScoreText != null)
            {
                scoreText = foundScoreText.GetComponent<TextMeshProUGUI>();
                Debug.Log("✅ ScoreText auto-assigned after scene load!");
            }
            else
            {
                Debug.LogWarning("⚠ Không tìm thấy Score Text sau khi load scene.");
            }
        }

        UpdateScoreUI(); // Cập nhật lại điểm sau khi vào scene
    }

    public void AddScore(int value)
    {
        score += value;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
        else
        {
            Debug.LogWarning("⚠ ScoreText chưa được gán!");
        }
    }

    public void EndGame()
    {
        PlayerPrefs.SetInt("FinalScore", score);
        PlayerPrefs.Save();
        SceneManager.LoadScene("EndGame");
    }

    public void ResetScore()
    {
        score = 0;
        UpdateScoreUI();
    }

    public void SaveHighScoreWithName(string playerName)
    {
        Debug.Log($"Gửi điểm số {score} của {playerName} lên API...");
        StartCoroutine(SendHighScoreToApi(playerName, score));
    }

    IEnumerator SendHighScoreToApi(string playerName, int score)
    {
        HighScoreEntry entry = new HighScoreEntry
        {
            playerName = playerName,
            score = score
        };

        string json = JsonUtility.ToJson(entry);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest("https://todo-app-be-6p6d.onrender.com/api/highscores", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

#if UNITY_EDITOR
            request.certificateHandler = new BypassCertificate();
#endif

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ High score sent successfully!");
                Debug.Log("Phản hồi từ server: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("❌ Error sending high score: " + request.error);
            }
        }
    }

#if UNITY_EDITOR
    class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData) => true;
    }
#endif
}
