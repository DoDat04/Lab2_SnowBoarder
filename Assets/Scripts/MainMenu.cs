using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] AudioClip menuMusic;
    private AudioSource audioSource;
    public GameObject optionsPanel;
    public GameObject instructionPanel;
    private float effectVolume = 1.0f;
    [SerializeField] private UnityEngine.UI.Slider effectSlider;
    [SerializeField] private UnityEngine.UI.Slider musicSlider;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null && menuMusic != null)
        {
            audioSource.clip = menuMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
        float effectVol = PlayerPrefs.GetFloat("EffectVolume", 1f);
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.value = musicVol;
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }
        if (effectSlider != null)
        {
            effectSlider.onValueChanged.RemoveAllListeners();
            effectSlider.value = effectVol;
            effectSlider.onValueChanged.AddListener(SetEffectVolume);
        }
        if (audioSource != null)
            audioSource.volume = musicVol;
    }

    // Hàm này sẽ được gọi khi nhấn nút Play
    public void PlayGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Level1"); // Đổi tên scene nếu cần
    }

    public void OpenOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(true);
        if (effectSlider != null)
        {
            effectSlider.onValueChanged.RemoveAllListeners();
            effectSlider.value = PlayerPrefs.GetFloat("EffectVolume", 1f);
            effectSlider.onValueChanged.AddListener(SetEffectVolume);
        }
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }
    }

    public void OpenInstructionPanel()
    {
        if(instructionPanel != null)
            instructionPanel.SetActive(true);
        
    }

    public void SetMusicVolume(float value)
    {
        if (audioSource != null)
            audioSource.volume = value;
    }

    public void SetEffectVolume(float value)
    {
        effectVolume = value;
        PlayerPrefs.SetFloat("EffectVolume", value);
    }

    public void setClose()
    {
        Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;

    }
}
