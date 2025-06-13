using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float torqueAmount = 1f;
    private Rigidbody2D rb2d;
    [SerializeField] private TextMeshProUGUI scoreText;

    private float totalRotation = 0f;
    private float lastZRotation = 0f;

    private int score = 0;

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
            score += 100;
            scoreText.text = "Score: " + score;
            Debug.Log("Scored! Current Score: " + score);
            totalRotation = 0f;
        }

    }
}
