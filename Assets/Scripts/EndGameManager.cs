using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameManager : MonoBehaviour
{
    public TextMeshProUGUI FinalScore;

    private void Start()
    {
        int score = PlayerPrefs.GetInt("FinalScore", 0);
        FinalScore.text = "Score: " + score.ToString();
    }
    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void PlayAgain()
    {
        // Reset điểm trước khi load lại Level
        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.ResetScore();
        }

        SceneManager.LoadScene("Level1");
    }
}
