using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishLine : MonoBehaviour
{
    [SerializeField] float delay = 1.5f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Finish Line Reached!");
        Invoke("ReloadScene", delay);
    }

    void ReloadScene()
    {
        SceneManager.LoadScene("Level1");
    }
}
