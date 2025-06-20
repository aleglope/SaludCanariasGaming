using UnityEngine;
using System.Collections;

public class VirusInvadersEnemyController : MonoBehaviour
{
    [Header("Enemy Data")]
    public VirusInvadersEnemyData enemyData;
    
    // References from original enemy script
    private SpriteRenderer spriteRenderer;
    private Transform player;
    private Rigidbody2D rb;
    private CircleCollider2D col;
    private IVirusInvadersMovement movementComponent;
    
    // State management
    private bool isDead = false;
    private float currentHealth;
    private float lastAttackTime = 0f;
    private EnemyState currentState = EnemyState.Idle;
    
    // Animation system (keeping from original)
    private Sprite[] spritesIdle;
    private Sprite[] spritesPulse;
    private Sprite[] spritesAttack;
    private Sprite[] spritesHit;
    private Sprite[] spritesDeath;
    private Sprite spriteDefault;
    
    private int currentFrame = 0;
    private float frameTime = 0f;
    private Sprite[] currentAnimation;
    private bool animationLoop = true;
    private string currentAnimationState = "";
    
    void Start()
    {
        if (enemyData == null)
        {
            Debug.LogError("Enemy Data is null! Please assign an EnemyData ScriptableObject.");
            return;
        }
        
        SetupComponents();
        CreateDefaultSprite();
        LoadSprites();
        FindPlayer();
        SetupMovementComponent();
        
        // Apply difficulty modifiers to health
        float difficultyHealthMultiplier = 1f;
        if (VirusInvadersGameManager.Instance != null && VirusInvadersGameManager.Instance.difficultyLevels.Length > 0)
        {
            var currentDifficulty = VirusInvadersGameManager.Instance.difficultyLevels[VirusInvadersGameManager.Instance.currentDifficultyLevel];
            difficultyHealthMultiplier = currentDifficulty.enemyHealthMultiplier;
        }
        
        currentHealth = enemyData.health * difficultyHealthMultiplier;
        StartSafeAnimation();
    }
    
    void SetupComponents()
    {
        // SpriteRenderer setup
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        spriteRenderer.color = enemyData.enemyColor;
        spriteRenderer.sortingLayerName = enemyData.sortingLayerName;
        spriteRenderer.sortingOrder = enemyData.sortingOrder;
        
        // Rigidbody2D setup
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        
        // Collider setup
        col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }
        col.radius = 0.5f * enemyData.scale; // Scale collider with enemy size
        col.isTrigger = true;
        
        // Scale setup
        transform.localScale = Vector3.one * enemyData.scale;
        
        // Position Z adjustment
        Vector3 pos = transform.position;
        pos.z = -1f;
        transform.position = pos;
        
        // Tags and layers
        gameObject.tag = "Enemy";
        gameObject.layer = LayerMask.NameToLayer("Enemies");
    }
    
    void SetupMovementComponent()
    {
        // Remove any existing movement components
        var existingMovements = GetComponents<IVirusInvadersMovement>();
        foreach (var movement in existingMovements)
        {
            if (movement is MonoBehaviour mb)
            {
                DestroyImmediate(mb);
            }
        }
        
        // Add the appropriate movement component
        switch (enemyData.movementType)
        {
            case MovementType.Static:
                movementComponent = gameObject.AddComponent<VirusInvadersStaticMovement>();
                break;
            case MovementType.Descend:
                movementComponent = gameObject.AddComponent<VirusInvadersDescendMovement>();
                break;
            case MovementType.Chase:
                movementComponent = gameObject.AddComponent<VirusInvadersChaseMovement>();
                break;
        }
        
        if (movementComponent != null)
        {
            movementComponent.Initialize();
            movementComponent.SetMovementSpeed(enemyData.moveSpeed);
            movementComponent.SetMovementParameters(enemyData.movementParameters);
        }
    }
    
    // Keep all the sprite loading and animation logic from the original script
    void CreateDefaultSprite()
    {
        Texture2D texture = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];
        Vector2 center = new Vector2(32, 32);
        
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                
                if (distance <= 28)
                {
                    pixels[y * 64 + x] = new Color(0.8f, 0.3f, 0.3f, 1f);
                }
                else if (distance <= 30)
                {
                    pixels[y * 64 + x] = new Color(0.6f, 0.2f, 0.2f, 1f);
                }
                else
                {
                    pixels[y * 64 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        spriteDefault = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        spriteRenderer.sprite = spriteDefault;
    }
    
    void LoadSprites()
    {
        string virusTypeString = enemyData.GetVirusTypeString();
        
        string[] basePaths = new string[]
        {
            $"VirusInvaders/Coronavirus/virus_spriteanimation/{virusTypeString}/",
            $"Spine/Coronavirus/virus_spriteanimation/{virusTypeString}/",
            $"Coronavirus/virus_spriteanimation/{virusTypeString}/",
            $"virus_spriteanimation/{virusTypeString}/"
        };
        
        foreach (string basePath in basePaths)
        {
            spritesIdle = LoadAnimation(basePath, "idle1");
            if (spritesIdle != null && spritesIdle.Length > 0)
            {
                spritesPulse = LoadAnimation(basePath, "pulse") ?? spritesIdle;
                spritesAttack = LoadAnimation(basePath, "attack") ?? spritesIdle;
                spritesHit = LoadAnimation(basePath, "hit") ?? spritesIdle;
                spritesDeath = LoadAnimation(basePath, "death") ?? spritesIdle;
                return;
            }
        }
        
        // Fallback to default sprites
        spritesIdle = new Sprite[] { spriteDefault };
        spritesPulse = spritesIdle;
        spritesAttack = spritesIdle;
        spritesHit = spritesIdle;
        spritesDeath = spritesIdle;
    }
    
    Sprite[] LoadAnimation(string basePath, string animation)
    {
        System.Collections.Generic.List<Sprite> sprites = new System.Collections.Generic.List<Sprite>();
        string prefix = basePath + $"coronavirus-{enemyData.GetVirusTypeString()}-{animation}_";
        
        for (int i = 0; i < 30; i++)
        {
            Sprite sprite = Resources.Load<Sprite>(prefix + i.ToString("00"));
            if (sprite == null)
            {
                sprite = Resources.Load<Sprite>(prefix + i.ToString());
            }
            
            if (sprite != null)
            {
                sprites.Add(sprite);
            }
            else if (sprites.Count > 0)
            {
                break;
            }
        }
        
        return sprites.Count > 0 ? sprites.ToArray() : null;
    }
    
    void FindPlayer()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                VirusInvadersPlayerController playerController = FindObjectOfType<VirusInvadersPlayerController>();
                if (playerController != null)
                {
                    player = playerController.transform;
                }
            }
        }
    }
    
    void Update()
    {
        if (isDead || player == null) return;
        
        UpdateAnimation();
        UpdateBehavior();
        UpdateMovement();
        LookAtPlayer();
    }
    
    void UpdateMovement()
    {
        if (movementComponent != null && currentState != EnemyState.Attacking)
        {
            movementComponent.UpdateMovement(player);
        }
    }
    
    void UpdateBehavior()
    {
        if (currentState == EnemyState.Hit || currentState == EnemyState.Dying) return;
        
        float distance = Vector2.Distance(transform.position, player.position);
        EnemyState newState = currentState;
        
        if (distance <= enemyData.attackRange)
        {
            newState = EnemyState.Attacking;
        }
        else if (distance <= enemyData.detectionRange)
        {
            newState = EnemyState.Moving;
        }
        else
        {
            newState = EnemyState.Idle;
        }
        
        if (newState != currentState)
        {
            currentState = newState;
            
            switch (currentState)
            {
                case EnemyState.Idle:
                    ChangeAnimation(spritesIdle, true, "idle");
                    break;
                case EnemyState.Moving:
                    ChangeAnimation(spritesPulse, true, "pulse");
                    break;
                case EnemyState.Attacking:
                    ChangeAnimation(spritesAttack, false, "attack");
                    break;
            }
        }
        
        // Handle attacking
        if (currentState == EnemyState.Attacking)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            
            if (Time.time - lastAttackTime >= enemyData.timeBetweenAttacks)
            {
                StartCoroutine(ExecuteAttack());
                lastAttackTime = Time.time;
            }
        }
    }
    
    // Keep all animation methods from original script
    void UpdateAnimation()
    {
        if (currentAnimation == null || currentAnimation.Length == 0) 
        {
            spriteRenderer.sprite = spriteDefault;
            return;
        }
        
        frameTime += Time.deltaTime;
        
        if (frameTime >= enemyData.animationSpeed)
        {
            currentFrame++;
            
            if (currentFrame >= currentAnimation.Length)
            {
                if (animationLoop)
                {
                    currentFrame = 0;
                }
                else
                {
                    currentFrame = currentAnimation.Length - 1;
                }
            }
            
            if (currentFrame >= 0 && currentFrame < currentAnimation.Length && currentAnimation[currentFrame] != null)
            {
                spriteRenderer.sprite = currentAnimation[currentFrame];
            }
            else
            {
                spriteRenderer.sprite = spriteDefault;
            }
            
            frameTime = 0f;
        }
    }
    
    void ChangeAnimation(Sprite[] newAnimation, bool loop, string name)
    {
        if (currentAnimationState == name && currentAnimation == newAnimation) return;
        
        if (newAnimation != null && newAnimation.Length > 0)
        {
            currentAnimation = newAnimation;
            animationLoop = loop;
            currentFrame = 0;
            frameTime = 0f;
            currentAnimationState = name;
            
            if (currentAnimation[0] != null)
            {
                spriteRenderer.sprite = currentAnimation[0];
            }
            else
            {
                spriteRenderer.sprite = spriteDefault;
            }
        }
    }
    
    void StartSafeAnimation()
    {
        if (spritesIdle != null && spritesIdle.Length > 0)
        {
            ChangeAnimation(spritesIdle, true, "idle");
        }
        else
        {
            spriteRenderer.sprite = spriteDefault;
        }
    }
    
    IEnumerator ExecuteAttack()
    {
        yield return new WaitForSeconds(0.5f);
        
        if (Vector2.Distance(transform.position, player.position) <= enemyData.attackRange)
        {
            VirusInvadersPlayerController playerController = player.GetComponent<VirusInvadersPlayerController>();
            if (playerController != null)
            {
                Vector2 knockbackDirection = (player.position - transform.position).normalized;
                playerController.RecibirDaño(knockbackDirection, enemyData.attackDamage);
            }
        }
    }
    
    void LookAtPlayer()
    {
        if (player == null || enemyData == null) return;
        
        float targetScale = enemyData.scale;
        
        if (player.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(-targetScale, targetScale, 1);
        }
        else
        {
            transform.localScale = new Vector3(targetScale, targetScale, 1);
        }
    }
    
    public void TakeDamage(float amount)
    {
        if (isDead) return;
        
        currentHealth -= amount;
        StartCoroutine(DamageEffect());
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    IEnumerator DamageEffect()
    {
        EnemyState previousState = currentState;
        currentState = EnemyState.Hit;
        ChangeAnimation(spritesHit, false, "hit");
        
        yield return new WaitForSeconds(0.3f);
        
        if (!isDead)
        {
            currentState = previousState;
        }
    }
    
    void Die()
    {
        isDead = true;
        currentState = EnemyState.Dying;
        col.enabled = false;
        rb.simulated = false;
        
        ChangeAnimation(spritesDeath, false, "death");
        
        // Notify GameManager
        if (VirusInvadersGameManager.Instance != null)
        {
            VirusInvadersGameManager.Instance.EnemyDefeated(enemyData);
        }
        
        StartCoroutine(DeathAnimation());
    }
    
    IEnumerator DeathAnimation()
    {
        yield return new WaitForSeconds(1f);
        
        // Fade out
        float time = 0f;
        Color initialColor = spriteRenderer.color;
        
        while (time < 0.5f)
        {
            time += Time.deltaTime;
            Color color = initialColor;
            color.a = Mathf.Lerp(1f, 0f, time / 0.5f);
            spriteRenderer.color = color;
            yield return null;
        }
        
        Destroy(gameObject);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            VirusInvadersPlayerController playerController = other.GetComponent<VirusInvadersPlayerController>();
            if (playerController != null)
            {
                Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
                playerController.RecibirDaño(knockbackDirection, enemyData.attackDamage);
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (enemyData == null) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyData.detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyData.attackRange);
    }
    
    // Public method to update enemy data (for difficulty changes)
    public void UpdateEnemyData(VirusInvadersEnemyData newData)
    {
        enemyData = newData;
        
        // Update visual properties
        spriteRenderer.color = enemyData.enemyColor;
        transform.localScale = Vector3.one * enemyData.scale;
        
        // Update collider size
        if (col != null)
        {
            col.radius = 0.5f * enemyData.scale;
        }
        
        // Update movement component with difficulty modifiers
        if (movementComponent != null)
        {
            float difficultySpeedMultiplier = 1f;
            if (VirusInvadersGameManager.Instance != null && VirusInvadersGameManager.Instance.difficultyLevels.Length > 0)
            {
                var currentDifficulty = VirusInvadersGameManager.Instance.difficultyLevels[VirusInvadersGameManager.Instance.currentDifficultyLevel];
                difficultySpeedMultiplier = currentDifficulty.enemySpeedMultiplier;
            }
            
            movementComponent.SetMovementSpeed(enemyData.moveSpeed * difficultySpeedMultiplier);
            movementComponent.SetMovementParameters(enemyData.movementParameters);
        }
        
        // Reload sprites if virus type changed
        LoadSprites();
        StartSafeAnimation();
    }
} 