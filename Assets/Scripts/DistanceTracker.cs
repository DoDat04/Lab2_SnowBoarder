using UnityEngine;
using TMPro;

public class DistanceTracker : MonoBehaviour
{
    public Transform player;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI speedText;

    private Vector3 startPosition;
    private Vector3 lastSpeedCheckPosition;
    public float distanceTravelled = 0f;

    private float speed = 0f;
    private float speedCheckTimer = 0f;
    public float speedCheckInterval = 0.2f; // Cập nhật tốc độ mỗi 0.2 giây

    void Start()
    {
        startPosition = player.position;
        lastSpeedCheckPosition = player.position;
    }

    void Update()
    {
        // Tính tổng quãng đường
        distanceTravelled = Vector3.Distance(startPosition, player.position);
        distanceText.text = distanceTravelled.ToString("F0") + " m";

        // Tính tốc độ sau mỗi khoảng thời gian nhất định
        speedCheckTimer += Time.deltaTime;
        if (speedCheckTimer >= speedCheckInterval)
        {
            float deltaDistance = Vector3.Distance(player.position, lastSpeedCheckPosition);
            speed = deltaDistance / speedCheckTimer;

            speedText.text = speed.ToString("F1") + " m/s";

            lastSpeedCheckPosition = player.position;
            speedCheckTimer = 0f;
        }
    }
}
