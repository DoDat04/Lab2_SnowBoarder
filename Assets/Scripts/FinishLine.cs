using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishLine : MonoBehaviour
{
    [SerializeField] float delay = 1.5f;
    [SerializeField] AudioClip finishSound;
    private AudioSource audioSource;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Finish Line Reached!");
        if (audioSource != null && finishSound != null)
        {
            float effectVolume = PlayerPrefs.GetFloat("EffectVolume", 1f);
            audioSource.PlayOneShot(finishSound, effectVolume);
        }
        Invoke("ReloadScene", delay);
    }

    void ReloadScene()
    {
        SceneManager.LoadScene("Level1");
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
}
