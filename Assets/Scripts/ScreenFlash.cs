using UnityEngine;

public class ScreenFlash : MonoBehaviour
{
    [SerializeField] private SpriteRenderer flashSprite;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.3f;
    
    void Start()
    {
        // Tự động tìm SpriteRenderer nếu chưa được gán
        if (flashSprite == null)
        {
            flashSprite = GetComponent<SpriteRenderer>();
        }
        
        // Ẩn flash ban đầu
        if (flashSprite != null)
        {
            Color color = flashSprite.color;
            color.a = 0f;
            flashSprite.color = color;
        }
    }
    
    public void Flash()
    {
        StartCoroutine(FlashCoroutine());
    }
    
    private System.Collections.IEnumerator FlashCoroutine()
    {
        if (flashSprite == null) yield break;
        
        // Hiện flash
        Color color = flashColor;
        color.a = 1f;
        flashSprite.color = color;
        
        // Fade out
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / flashDuration);
            flashSprite.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            yield return null;
        }
        
        // Ẩn hoàn toàn
        color.a = 0f;
        flashSprite.color = color;
    }
} 