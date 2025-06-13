using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameManager : MonoBehaviour
{

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Đổi tên scene nếu cần
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene("Level1"); // Đổi tên scene nếu cần
    }
}
