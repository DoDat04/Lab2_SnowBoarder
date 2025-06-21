using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class LeaderboardUI : MonoBehaviour
{
    public GameObject leaderboardPanel;
    public TMP_Text[] rankTexts;
    private void Awake()
    {
        leaderboardPanel.SetActive(false);
    }

    public void ShowLeaderboard()
    {
        leaderboardPanel.SetActive(true);

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(GetLeaderboardFromApi());
        }
        else
        {
            Debug.LogWarning("⚠ Cannot start coroutine - GameObject inactive.");
        }
    }


    IEnumerator GetLeaderboardFromApi()
    {
        string url = "https://todo-app-be-6p6d.onrender.com/api/highscores/top";

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;

            HighScoreList result = JsonUtility.FromJson<HighScoreList>("{\"highScores\":" + json + "}");
            Debug.Log("Dữ liệu điểm cao đã nhận: " + result.highScores);

            if (result != null && result.highScores != null)
            {
                for (int i = 0; i < rankTexts.Length; i++)
                {
                    if (i < result.highScores.Count)
                    {
                        rankTexts[i].text = $"{i + 1}. {result.highScores[i].playerName} - {result.highScores[i].score} point";
                    }
                    else
                    {
                        rankTexts[i].text = $"{i + 1}. ---";
                    }
                    Debug.Log($"Cập nhật rank {i + 1}: {rankTexts[i].text}");
                }
            }
            else
            {
                Debug.LogWarning("Không có dữ liệu điểm cao từ API.");
                ClearLeaderboardUI();
            }
        }
        else
        {
            Debug.LogError("Lỗi khi lấy leaderboard từ API: " + request.error);
            ClearLeaderboardUI();
        }
    }

    public void HideLeaderboard()
    {
        leaderboardPanel.SetActive(false);
    }

    private void ClearLeaderboardUI()
    {
        for (int i = 0; i < rankTexts.Length; i++)
        {
            rankTexts[i].text = $"{i + 1}. ---";
        }
    }
}
