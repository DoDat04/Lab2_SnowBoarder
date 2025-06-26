using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    private PlayerInputActions playerInputActions;
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

    // Time Trial Mode - Cải thiện để fix bug
    [SerializeField] private bool isTimeTrialMode = true; // Bật/tắt Time Trial Mode
    [SerializeField] private float timeTrialDuration = 60f; // Thời gian ban đầu (60 giây)
    [SerializeField] private TextMeshProUGUI timeText; // UI hiển thị thời gian
    private float currentTime; // Thời gian còn lại
    private bool isTimeTrialActive = false; // Trạng thái Time Trial

    // Static để lưu trữ thời gian qua các level
    private static float savedTimeTrialTime = 0f;
    private static bool hasTimeTrialStarted = false;

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
            // Không destroy khi chuyển scene nếu cần thiết
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        playerInputActions = UserInput.Instance.playerInput; // ✅ Dùng chung
    }
    private void OnEnable()
    {
        playerInputActions.Player.Enable();
    }

    private void OnDisable()
    {
        playerInputActions.Player.Disable();
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

        // Debug để kiểm tra Time Trial Mode
        Debug.Log($"PlayerController Start - isTimeTrialMode: {isTimeTrialMode}");
        Debug.Log($"PlayerController Start - hasTimeTrialStarted: {hasTimeTrialStarted}");
        Debug.Log($"PlayerController Start - savedTimeTrialTime: {savedTimeTrialTime}");

        // Khởi tạo Time Trial Mode được cải thiện
        InitializeTimeTrialMode();

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

    void OnEnable()
    {
        // Đảm bảo Time Trial được khởi tạo lại khi object được enable
        if (isTimeTrialMode)
        {
            InitializeTimeTrialMode();
        }
        
        // Tìm lại timeText sau khi HealthManager được khởi tạo
        StartCoroutine(FindTimeTextAfterDelay());
    }

    private System.Collections.IEnumerator FindTimeTextAfterDelay()
    {
        yield return new WaitForSeconds(0.2f); // Đợi HealthManager khởi tạo xong
        
        // Tự động tìm timeText UI nếu chưa được gán
        if (timeText == null)
        {
            // Tìm trong HealthManager trước
            if (HealthManager.instance != null)
            {
                TextMeshProUGUI[] allTexts = HealthManager.instance.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (TextMeshProUGUI text in allTexts)
                {
                    if (text.name.Contains("Time") || text.text.Contains("Time"))
                    {
                        timeText = text;
                        Debug.Log("✅ TimeText found in HealthManager after delay!");
                        UpdateTimeDisplay(); // Cập nhật UI ngay lập tức
                        break;
                    }
                }
            }
            
            // Nếu không tìm thấy trong HealthManager, thử tìm theo tag
            if (timeText == null)
            {
                GameObject foundTimeText = GameObject.FindGameObjectWithTag("TimeText");
                if (foundTimeText != null)
                {
                    timeText = foundTimeText.GetComponent<TextMeshProUGUI>();
                    Debug.Log("✅ TimeText auto-assigned after delay!");
                    UpdateTimeDisplay(); // Cập nhật UI ngay lập tức
                }
                else
                {
                    Debug.LogWarning("⚠ Không tìm thấy TimeText trong HealthManager hoặc với tag 'TimeText' sau delay");
                }
            }
        }
    }

    // Hàm khởi tạo Time Trial Mode được cải thiện
    void InitializeTimeTrialMode()
    {
        Debug.Log($"InitializeTimeTrialMode called - isTimeTrialMode: {isTimeTrialMode}");
        
        if (isTimeTrialMode)
        {
            // Nếu đây là lần đầu tiên bắt đầu Time Trial
            if (!hasTimeTrialStarted)
            {
                currentTime = timeTrialDuration;
                savedTimeTrialTime = currentTime;
                hasTimeTrialStarted = true;
                Debug.Log($"Time Trial Started! Initial time: {currentTime}");
            }
            else
            {
                // Tiếp tục với thời gian đã lưu
                currentTime = savedTimeTrialTime;
                Debug.Log($"Time Trial Continued! Remaining time: {currentTime}");
            }

            isTimeTrialActive = true;
            UpdateTimeDisplay();
            Debug.Log($"Time Trial Active: {isTimeTrialActive}, Current Time: {currentTime}");
        }
        else
        {
            Debug.Log("Time Trial Mode is disabled!");
        }
    }

    // Hàm để reset Time Trial (gọi khi bắt đầu game mới)
    public static void ResetTimeTrial()
    {
        savedTimeTrialTime = 0f;
        hasTimeTrialStarted = false;
        Debug.Log("Time Trial Reset!");
    }

    // Hàm để lưu thời gian hiện tại (gọi trước khi chuyển level)
    public void SaveTimeTrialProgress()
    {
        if (isTimeTrialMode && isTimeTrialActive)
        {
            savedTimeTrialTime = currentTime;
            Debug.Log($"Time Trial Progress Saved: {savedTimeTrialTime}");
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
        if (groundCheck == null)
        {
            Debug.LogError("GroundCheck chưa được gán!");
            return;
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Xoay ván bằng input Move (float)
        Vector2 moveInput = playerInputActions.Player.Move.ReadValue<Vector2>();

        if (moveInput.x < 0)
        {
            rb2d.AddTorque(torqueAmount); // quay trái
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
        if (boosting && speedEffect != null)
        {
            speedEffect.Play(true);
        }
        else if (moveInput.x > 0)
        {
            rb2d.AddTorque(-torqueAmount); // quay phải
        }

        // Nhảy
        if (playerInputActions.Player.Jump.triggered && isGrounded)
        {
            Jump();
        }

        TrackRotation();
        UpdateCombo();

        // Time Trial Mode - Đếm ngược thời gian

        // Time Trial Mode - Đếm ngược thời gian (cải thiện)
        if (isTimeTrialActive && isTimeTrialMode)
        {
            currentTime -= Time.deltaTime;
            savedTimeTrialTime = currentTime; // Liên tục cập nhật thời gian đã lưu
            UpdateTimeDisplay();


            // Debug để kiểm tra timer có chạy không
            if (Time.frameCount % 60 == 0) // Log mỗi 60 frames (khoảng 1 giây)
            {
                Debug.Log($"Timer running - Current Time: {currentTime:F1}");
            }

            // Kiểm tra hết thời gian
            if (currentTime <= 0f)
            {
                TimeTrialGameOver();
            }
        }
        else
        {
            // Debug để kiểm tra tại sao timer không chạy
            if (Time.frameCount % 120 == 0) // Log mỗi 120 frames
            {
                Debug.Log($"Timer NOT running - isTimeTrialActive: {isTimeTrialActive}, isTimeTrialMode: {isTimeTrialMode}");
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
        Debug.Log($"UpdateTimeDisplay - timeText: {(timeText != null ? "NOT NULL" : "NULL")}, isTimeTrialMode: {isTimeTrialMode}");
        
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
            
            Debug.Log($"Time display updated: {timeText.text}");
        }
        else
        {
            Debug.LogWarning($"Cannot update time display - timeText: {(timeText != null ? "NOT NULL" : "NULL")}, isTimeTrialMode: {isTimeTrialMode}");
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
        isTimeTrialActive = false;

        // Reset Time Trial để có thể chơi lại
        ResetTimeTrial();

        // Chuyển về màn hình EndGame sau 1 giây
        Invoke("LoadEndGameScene", 1f);
    }

    void LoadEndGameScene()
    {
        //UnityEngine.SceneManagement.SceneManager.LoadScene("EndGame");
        if (instance != null)
        {
            instance.EndGame();
        }
    }

    // Hàm public để thêm thời gian (có thể dùng cho coin bonus)
    public void AddTime(float timeToAdd)
    {
        if (isTimeTrialActive && isTimeTrialMode)
        {
            currentTime += timeToAdd;
            savedTimeTrialTime = currentTime; // Cập nhật thời gian đã lưu
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

    // Hàm để gọi trước khi destroy object (nếu cần)
    void OnDestroy()
    {
        SaveTimeTrialProgress();
    }
}