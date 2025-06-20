using UnityEngine;

public class VirusInvadersBullet : MonoBehaviour
{
    [Header("VirusInvaders - Syringe Configuration")]
    public float velocidad = 15f;
    public float daño = 50f;
    public float tiempoVida = 5f;
    public float radioDañoColision = 0.4f; // Radio de colisión más grande para facilitar el impacto
    
    [Header("VirusInvaders - Impact Effects")]
    public bool createExplosionOnHit = true;
    public float explosionScale = 1f;
    
    private Vector2 direccion = Vector2.up; // Always upward
    private bool configurada = false;
    
    void Start()
    {
        // Syringe must move upward immediately
        ConfigurarJeringuilla();
        
        // Auto-destroy after lifetime
        Destroy(gameObject, tiempoVida);
    }
    
    void ConfigurarJeringuilla()
    {
        // Ensure we have Rigidbody2D
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Setup physics
        rb.gravityScale = 0f;
        rb.linearVelocity = direccion * velocidad;
        rb.angularVelocity = 0f;
        
        // Ensure we have collider
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
            col.radius = radioDañoColision; // Usar variable configurable
            col.isTrigger = true;
        }
        else
        {
            col.radius = radioDañoColision; // Actualizar si ya existe
        }
        
        // Setup Tag and Layer
        gameObject.tag = "Bullet";
        gameObject.layer = LayerMask.NameToLayer("Default");
        
        // Rotate to point upward (vertical syringe)
        transform.rotation = Quaternion.AngleAxis(180f, Vector3.forward);
        
        configurada = true;
    }
    
    public void ConfigurarSprite(Sprite sprite)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
        }
        
        if (sprite != null)
        {
            sr.sprite = sprite;
        }
        else
        {
            // Create temporary cyan sprite
            Texture2D texture = new Texture2D(4, 12);
            Color[] pixels = new Color[4 * 12];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.cyan;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            
            sr.sprite = Sprite.Create(texture, new Rect(0, 0, 4, 12), new Vector2(0.5f, 0.5f));
        }
        
        // Setup sorting
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 15;
    }
    
    void Update()
    {
        // Ensure syringe keeps moving upward
        if (configurada)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null && rb.linearVelocity.y < velocidad * 0.5f)
            {
                rb.linearVelocity = direccion * velocidad;
            }
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"VirusInvaders: Bullet hit {other.name} (Tag: {other.tag})");
        bool hitSomething = false;
        
        // Check if hit an enemy
        if (other.CompareTag("Enemy"))
        {
            hitSomething = true;
            
            // First try new EnemyController system
            VirusInvadersEnemyController enemyController = other.GetComponent<VirusInvadersEnemyController>();
            if (enemyController != null)
            {
                enemyController.TakeDamage(daño);
            }
            else
            {
                // Fallback to old enemy system for compatibility
                VirusInvadersCoronavirusEnemy enemigo = other.GetComponent<VirusInvadersCoronavirusEnemy>();
                if (enemigo != null)
                {
                    enemigo.RecibirDaño(daño);
                }
            }
        }
        
        // Check walls (destroy on contact with borders)
        if (other.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            hitSomething = true;
        }
        
        // Create explosion effect at impact point (only once)
        if (hitSomething && createExplosionOnHit)
        {
            float scale = other.CompareTag("Enemy") ? explosionScale : explosionScale * 0.7f;
            VirusInvadersBoomEffect.CreateExplosion(transform.position, scale);
        }
        
        // Destroy bullet after impact
        if (hitSomething)
        {
            Destroy(gameObject);
        }
    }
    
    void OnBecameInvisible()
    {
        // Destroy when off-screen
        Destroy(gameObject);
    }
}
