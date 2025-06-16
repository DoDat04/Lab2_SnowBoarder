using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [SerializeField] float torqueAmount = 1f;
    [SerializeField] float jumpForce = 10f; // Lực nhảy
    private Rigidbody2D rb2d;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private LayerMask groundLayer; // Layer của mặt đất
    [SerializeField] private Transform groundCheck; // Điểm kiểm tra va chạm với mặt đất
    [SerializeField] private float groundCheckRadius = 0.2f; // Bán kính kiểm tra va chạm
    [SerializeField] private float moveSpeed = 5f;      // Tốc độ bình thường
    [SerializeField] private float boostSpeed = 10f;    // Tốc độ khi tăng tốc
    private bool isBoosting = false;

    private float totalRotation = 0f;
    private float lastZRotation = 0f;
    private int score = 0;
    private bool isGrounded; // Kiểm tra xem player có đang ở trên mặt đất không

    [SerializeField] private ParticleSystem speedEffect;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioSource audioSource;

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

    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            // Vẽ vòng tròn kiểm tra va chạm
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    void Update()
    {
        // Kiểm tra va chạm với mặt đất
        if (groundCheck == null)
        {
            Debug.LogError("GroundCheck chưa được gán!");
            return;
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Xoay ván bằng phím mũi tên
        if (Keyboard.current.leftArrowKey.isPressed)
        {
            rb2d.AddTorque(torqueAmount);
        }
        else if (Keyboard.current.rightArrowKey.isPressed)
        {
            rb2d.AddTorque(-torqueAmount);
        }

        // Điều khiển tốc độ bằng phím mũi tên lên/xuống
        float currentSpeed = moveSpeed;
        if (Keyboard.current.upArrowKey.isPressed)
        {
            currentSpeed = boostSpeed;
        }
        else if (Keyboard.current.downArrowKey.isPressed)
        {
            currentSpeed = moveSpeed * 0.5f; // Giảm tốc xuống một nửa
        }

        // Luôn trượt sang phải
        rb2d.linearVelocity = new Vector2(currentSpeed, rb2d.linearVelocity.y);

        // Bật/tắt hiệu ứng speed khi tăng tốc
        bool boosting = Keyboard.current.upArrowKey.isPressed;
        Debug.Log($"Boosting: {boosting}, isPlaying: {(speedEffect != null && speedEffect.isPlaying)}");
        if (boosting && speedEffect != null)
        {
            speedEffect.Play(true);
        }
        else if (!boosting && speedEffect != null)
        {
            speedEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // Nhảy
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (isGrounded)
            {
                Jump();
            }
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

    void Jump()
    {
        if (rb2d == null)
        {
            Debug.LogError("Rigidbody2D is null!");
            return;
        }
        Debug.Log($"Jumping with force: {jumpForce}");
        rb2d.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        // Phát âm thanh nhảy
        if (audioSource != null && jumpClip != null)
            audioSource.PlayOneShot(jumpClip);
    }

    public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = "Score: " + score;
        Debug.Log("Scored! Current Score: " + score);
    }
}
