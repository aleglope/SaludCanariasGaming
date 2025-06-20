using UnityEngine;
using System.Collections;

[System.Serializable]
public class VirusInvadersBoomEffect : MonoBehaviour
{
    [Header("VirusInvaders - Boom Effect Configuration")]
    public float animationSpeed = 0.03f;
    public float scaleMultiplier = 1f;
    public int maxFrames = 30;
    
    private SpriteRenderer spriteRenderer;
    private Sprite[] boomSprites;
    private int currentFrame = 0;
    private bool isPlaying = false;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        spriteRenderer.sortingLayerName = "Default";
        spriteRenderer.sortingOrder = 20;
        
        LoadBoomSprites();
    }
    
    void LoadBoomSprites()
    {
        System.Collections.Generic.List<Sprite> sprites = new System.Collections.Generic.List<Sprite>();
        
        Object[] allSprites = Resources.LoadAll("VirusInvaders/Sprites/boom1", typeof(Sprite));
        
        if (allSprites != null && allSprites.Length > 0)
        {
            foreach (Object obj in allSprites)
            {
                if (obj is Sprite sprite)
                {
                    sprites.Add(sprite);
                }
            }
            
            // Sort by name to ensure correct sequence
            sprites.Sort((a, b) => {
                string aNum = a.name.Replace("boom1_", "");
                string bNum = b.name.Replace("boom1_", "");
                
                if (int.TryParse(aNum, out int aInt) && int.TryParse(bNum, out int bInt))
                {
                    return aInt.CompareTo(bInt);
                }
                return a.name.CompareTo(b.name);
            });
            
            int framesToUse = Mathf.Min(sprites.Count, maxFrames);
            boomSprites = new Sprite[framesToUse];
            
            for (int i = 0; i < framesToUse; i++)
            {
                boomSprites[i] = sprites[i];
            }
        }
        else
        {
            Debug.LogWarning("VirusInvaders: No se encontraron sprites de boom1! Creando efecto simple.");
            CreateSimpleExplosion();
        }
    }
    
    void CreateSimpleExplosion()
    {
        Texture2D texture = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];
        Vector2 center = new Vector2(32, 32);
        
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                
                if (distance <= 12)
                {
                    pixels[y * 64 + x] = new Color(1f, 1f, 0.2f, 0.9f);
                }
                else if (distance <= 20)
                {
                    pixels[y * 64 + x] = new Color(1f, 0.6f, 0f, 0.7f);
                }
                else if (distance <= 32)
                {
                    pixels[y * 64 + x] = new Color(1f, 0.2f, 0f, 0.4f);
                }
                else
                {
                    pixels[y * 64 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        Sprite simpleSprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        boomSprites = new Sprite[] { simpleSprite };
    }
    
    public void PlayExplosion()
    {
        if (boomSprites == null || boomSprites.Length == 0) 
        {
            Debug.LogError("VirusInvaders: No hay sprites de explosión disponibles!");
            return;
        }
        
        isPlaying = true;
        currentFrame = 0;
        
        spriteRenderer.sprite = boomSprites[0];
        transform.localScale = Vector3.one * scaleMultiplier;
        spriteRenderer.color = Color.white;
        
        StartCoroutine(AnimateExplosion());
    }
    
    IEnumerator AnimateExplosion()
    {
        float frameTimer = 0f;
        
        while (isPlaying && currentFrame < boomSprites.Length)
        {
            frameTimer += Time.deltaTime;
            
            if (frameTimer >= animationSpeed)
            {
                currentFrame++;
                
                if (currentFrame >= boomSprites.Length)
                {
                    isPlaying = false;
                    Destroy(gameObject);
                    yield break;
                }
                
                spriteRenderer.sprite = boomSprites[currentFrame];
                frameTimer = 0f;
            }
            
            yield return null;
        }
    }
    
    // Static factory method - simple explosion for bullet impacts
    public static void CreateExplosion(Vector3 position, float scale = 1f)
    {
        GameObject explosionGO = new GameObject("VirusInvadersBoomEffect");
        explosionGO.transform.position = position;
        
        VirusInvadersBoomEffect explosion = explosionGO.AddComponent<VirusInvadersBoomEffect>();
        explosion.scaleMultiplier = scale;
        
        explosion.PlayExplosion();
    }
} 