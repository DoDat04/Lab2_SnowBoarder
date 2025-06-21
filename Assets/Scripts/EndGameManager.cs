using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameManager : MonoBehaviour
{

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
