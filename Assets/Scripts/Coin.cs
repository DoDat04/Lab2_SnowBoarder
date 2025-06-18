using UnityEngine;

public class Coin : MonoBehaviour
{
    public AudioClip coinSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Tăng điểm
            PlayerController.Instance.AddScore(1);

            // Thêm thời gian trong Time Trial Mode
            PlayerController.Instance.AddTime(5f); // Cộng 5 giây

            // Play sound (nếu có)
            if (coinSound != null)
                AudioSource.PlayClipAtPoint(coinSound, transform.position);

            // Huỷ coin
            Destroy(gameObject);
        }
    }
}
