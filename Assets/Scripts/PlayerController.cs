using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [SerializeField] float torqueAmount = 1f;
    [SerializeField] float jumpForce = 10f; // Lực nhảy
    private Rigidbody2D rb2d;
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
    private float boostTime = 0f; // Thời gian đã boost
    private float boostScoreInterval = 1f; // Cộng điểm mỗi 1 giây boost
    
    // Combo system
    private int comboCount = 0; // Số thủ thuật liên tiếp
    private float comboTimeWindow = 3f; // Thời gian để duy trì combo (3 giây)
    private float lastComboTime = 0f; // Thời điểm thủ thuật cuối cùng
    private float comboMultiplier = 1f; // Hệ số nhân điểm
    
    // Time Trial Mode
    [SerializeField] private bool isTimeTrialMode = false; // Bật/tắt Time Trial Mode
    [SerializeField] private float timeTrialDuration = 60f; // Thời gian ban đầu (60 giây)
    [SerializeField] private TextMeshProUGUI timeText; // UI hiển thị thời gian
    private float currentTime; // Thời gian còn lại
    private bool isTimeTrialActive = false; // Trạng thái Time Trial

    [SerializeField] private ParticleSystem speedEffect;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip backgroundMusic;

    public static ScoreManager instance;

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

        audioSource = GetComponent<AudioSource>();
        if (audioSource != null && backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.Play();
        }

        // Khởi tạo Time Trial Mode
        if (isTimeTrialMode)
        {
            currentTime = timeTrialDuration;
            isTimeTrialActive = true;
            UpdateTimeDisplay();
        }
        
        // Đảm bảo speed effect hiển thị trên cùng và ẩn ban đầu
        if (speedEffect != null)
        {
            ParticleSystemRenderer particleRenderer = speedEffect.GetComponent<ParticleSystemRenderer>();
            if (particleRenderer != null)
            {
                particleRenderer.sortingOrder = 102; // Đặt sorting order cao hơn finish line và particle effect
            }
            
            // Ẩn speed effect ban đầu
            speedEffect.Stop();
            speedEffect.Clear();
        }
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
            // Cộng điểm khi tăng tốc
            boostTime += Time.deltaTime;
            if (boostTime >= boostScoreInterval)
            {
                AddScoreWithCombo(5); // Cộng 5 điểm mỗi giây boost với combo
                boostTime = 0f;
            }
        }
        else if (Keyboard.current.downArrowKey.isPressed)
        {
            currentSpeed = moveSpeed * 0.5f; // Giảm tốc xuống một nửa
        }
        else
        {
            // Reset boost time khi không boost
            boostTime = 0f;
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
        UpdateCombo();
        
        // Time Trial Mode - Đếm ngược thời gian
        if (isTimeTrialActive && isTimeTrialMode)
        {
            currentTime -= Time.deltaTime;
            UpdateTimeDisplay();
            
            // Kiểm tra hết thời gian
            if (currentTime <= 0f)
            {
                TimeTrialGameOver();
            }
        }
    }

    void TrackRotation()
    {
        float currentZRotation = rb2d.rotation;
        float deltaRotation = Mathf.DeltaAngle(lastZRotation, currentZRotation);
        totalRotation += deltaRotation;
        lastZRotation = currentZRotation;

        if (Mathf.Abs(totalRotation) >= 360f)
        {
            AddScoreWithCombo(10);
            totalRotation = 0f;
        }
    }

    void AddScoreWithCombo(int baseScore)
    {
        // Kiểm tra xem có trong thời gian combo không
        if (Time.time - lastComboTime <= comboTimeWindow)
        {
            comboCount++;
            comboMultiplier = 1f + (comboCount * 0.5f); // Tăng 0.5 cho mỗi combo
        }
        else
        {
            // Reset combo nếu quá thời gian
            comboCount = 1;
            comboMultiplier = 1.5f;
        }
        
        lastComboTime = Time.time;
        
        int finalScore = Mathf.RoundToInt(baseScore * comboMultiplier);
        AddScore(finalScore);
        
        // Hiển thị thông tin combo
        Debug.Log($"Combo x{comboCount}! Multiplier: x{comboMultiplier:F1} | Score: {finalScore}");
    }

    void UpdateCombo()
    {
        // Reset combo nếu quá thời gian
        if (Time.time - lastComboTime > comboTimeWindow && comboCount > 0)
        {
            comboCount = 0;
            comboMultiplier = 1f;
            Debug.Log("Combo broken!");
        }
    }

    void UpdateTimeDisplay()
    {
        if (timeText != null && isTimeTrialMode)
        {
            // Đảm bảo thời gian không âm
            float displayTime = Mathf.Max(0f, currentTime);
            
            int minutes = Mathf.FloorToInt(displayTime / 60f);
            int seconds = Mathf.FloorToInt(displayTime % 60f);
            timeText.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);
            
            // Đổi màu khi sắp hết thời gian
            if (currentTime <= 10f)
            {
                timeText.color = Color.red;
            }
            else if (currentTime <= 30f)
            {
                timeText.color = Color.yellow;
            }
            else
            {
                timeText.color = Color.white;
            }
        }
    }

    void TimeTrialGameOver()
    {
        Debug.Log("Time Trial Game Over! Hết thời gian!");
        
        // Dừng player
        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
        }
        
        // Vô hiệu hóa PlayerController
        enabled = false;
        
        // Chuyển về màn hình EndGame sau 1 giây
        Invoke("LoadEndGameScene", 1f);
    }

    void LoadEndGameScene()
    {
        //UnityEngine.SceneManagement.SceneManager.LoadScene("EndGame");
        instance.EndGame();
    }

    // Hàm public để thêm thời gian (có thể dùng cho coin bonus)
    public void AddTime(float timeToAdd)
    {
        if (isTimeTrialActive && isTimeTrialMode)
        {
            currentTime += timeToAdd;
            Debug.Log($"Added {timeToAdd} seconds! Current time: {currentTime:F1}");
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
        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.AddScore(amount);
        }
        else
        {
            Debug.LogWarning("⚠ ScoreManager instance not found!");
        }
    }
}
