using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [SerializeField] float torqueAmount = 1f;
    private Rigidbody2D rb2d;
    [SerializeField] private TextMeshProUGUI scoreText;

    private float totalRotation = 0f;
    private float lastZRotation = 0f;
    private int score = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        lastZRotation = rb2d.rotation;
    }

    void Update()
    {
        if (Keyboard.current.leftArrowKey.isPressed)
        {
            rb2d.AddTorque(torqueAmount);
        }
        else if (Keyboard.current.rightArrowKey.isPressed)
        {
            rb2d.AddTorque(-torqueAmount);
        }

        TrackRotation();
    }

    void TrackRotation()
    {
        float currentZRotation = rb2d.rotation;
        float deltaRotation = Mathf.DeltaAngle(lastZRotation, currentZRotation);
        totalRotation += deltaRotation;
        lastZRotation = currentZRotation;

        if (Mathf.Abs(totalRotation) >= 360f)
        {
            AddScore(100);
            totalRotation = 0f;
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = "Score: " + score;
        Debug.Log("Scored! Current Score: " + score);
    }
}
