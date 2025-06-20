using UnityEngine;

[System.Serializable]
public class VirusInvadersPlayerController : MonoBehaviour
{
    [Header("VirusInvaders - Movement Configuration")]
    public float velocidadMovimiento = 5f;
    public float fuerzaSalto = 5f;
    
    [Header("VirusInvaders - Jump Configuration")]
    public Transform verificadorSuelo;
    public float radioVerificacion = 0.3f;
    public LayerMask capaSuelo;
    
    [Header("VirusInvaders - Physics Configuration")]
    public bool usarRigidbody = true;
    public float gravedad = -9.8f;
    
    [Header("VirusInvaders - Visual Configuration")]
    public float escalaJugador = 0.3f;
    
    [Header("VirusInvaders - Shooting Configuration")]
    public bool habilitarDisparo = true;
    
    [Header("VirusInvaders - Health System")]
    public float vidaMaxima = 100f;
    public bool crearBarraDeVida = true;
    public bool usarBarraUI = true; // true = UI Canvas, false = World Space
    public Vector3 posicionBarraRelativa = new Vector3(0, 0.25f, 0);
    
    // Private references
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private VirusInvadersPlayerShooter shooter;
    private VirusInvadersHealthBar barraVida;
    private float vidaActual;
    private bool estaEnSuelo;
    private float inputHorizontal;
    private float velocidadVertical;
    private bool estaMuerto = false;
    
    // Events
    public System.Action OnPlayerDeath;
    public System.Action<float> OnHealthChanged;
    
    void Start()
    {
        ConfigurarComponentes();
        ConfigurarFisicas();
        ConfigurarDisparo();
        ConfigurarSistemaVida();
    }
    
    void ConfigurarComponentes()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        transform.localScale = new Vector3(escalaJugador, escalaJugador, 1);
        
        if (usarRigidbody)
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
            }
            
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 1f;
            rb.linearDamping = 0f;
            rb.angularDamping = 0.05f;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
    
    void ConfigurarFisicas()
    {
        if (verificadorSuelo == null)
        {
            GameObject groundChecker = new GameObject("GroundChecker");
            groundChecker.transform.SetParent(transform);
            groundChecker.transform.localPosition = new Vector3(0, -0.5f, 0);
            verificadorSuelo = groundChecker.transform;
        }
        
        capaSuelo = 1 << 3; // Layer 3 = Ground
    }
    
    void ConfigurarDisparo()
    {
        if (habilitarDisparo)
        {
            shooter = GetComponent<VirusInvadersPlayerShooter>();
            if (shooter == null)
            {
                shooter = gameObject.AddComponent<VirusInvadersPlayerShooter>();
            }
        }
    }
    
    void ConfigurarSistemaVida()
    {
        vidaActual = vidaMaxima;
        
        if (crearBarraDeVida)
        {
            CrearBarraDeVida();
        }
    }
    
    void CrearBarraDeVida()
    {
        if (usarBarraUI)
        {
            CrearBarraUI();
        }
        else
        {
            CrearBarraWorldSpace();
        }
    }
    
    void CrearBarraUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("VirusInvaders: No se encontró Canvas. Creando uno.");
            GameObject canvasGO = new GameObject("VirusInvaders_Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        GameObject barraGO = new GameObject("VirusInvaders_PlayerHealthBar_UI");
        barraGO.transform.SetParent(canvas.transform, false);
        
        RectTransform rt = barraGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 0.05f);
        rt.anchorMax = new Vector2(0.35f, 0.1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        
        UnityEngine.UI.Image imagen = barraGO.AddComponent<UnityEngine.UI.Image>();
        
        Sprite barSprite = Resources.Load<Sprite>("VirusInvaders/Sprites/blood_red_bar");
        if (barSprite != null)
        {
            imagen.sprite = barSprite;
        }
        else
        {
            Debug.LogWarning("VirusInvaders: No se encontró blood_red_bar.png en Resources/VirusInvaders/Sprites/!");
        }
        
        barraVida = barraGO.AddComponent<VirusInvadersHealthBar>();
        barraVida.useImageComponent = true;
        barraVida.maxHealth = vidaMaxima;
        barraVida.currentHealth = vidaActual;
    }
    
    void CrearBarraWorldSpace()
    {
        Vector3 posicionBarra = transform.position + posicionBarraRelativa;
        GameObject barraGO = new GameObject("VirusInvaders_PlayerHealthBar_World");
        barraGO.transform.position = posicionBarra;
        barraGO.transform.SetParent(transform);
        
        SpriteRenderer sr = barraGO.AddComponent<SpriteRenderer>();
        
        Sprite barSprite = Resources.Load<Sprite>("VirusInvaders/Sprites/blood_red_bar");
        if (barSprite != null)
        {
            sr.sprite = barSprite;
        }
        else
        {
            Debug.LogWarning("VirusInvaders: No se encontró blood_red_bar.png en Resources/VirusInvaders/Sprites/!");
        }
        
        sr.sortingLayerName = "UI";
        sr.sortingOrder = 100;
        
        barraGO.transform.localScale = new Vector3(0.15f, 0.03f, 1f);
        
        barraVida = barraGO.AddComponent<VirusInvadersHealthBar>();
        barraVida.useImageComponent = false;
        barraVida.maxHealth = vidaMaxima;
        barraVida.currentHealth = vidaActual;
    }

    void Update()
    {
        if (estaMuerto) return;
        
        inputHorizontal = Input.GetAxisRaw("Horizontal");
        bool jumpInputThisFrame = Input.GetKeyDown(KeyCode.Space);
        
        // Ground detection with primary and fallback checks
        if (verificadorSuelo != null)
        {
            bool groundLayer3 = Physics2D.OverlapCircle(verificadorSuelo.position, radioVerificacion, 1 << 3);
            bool groundAllLayers = Physics2D.OverlapCircle(verificadorSuelo.position, radioVerificacion, ~0);
            
            estaEnSuelo = groundLayer3 || groundAllLayers;
        }
        
        if (jumpInputThisFrame && estaEnSuelo)
        {
            Saltar();
        }
        
        // Sprite flipping
        if (spriteRenderer != null)
        {
            if (inputHorizontal > 0.1f)
            {
                spriteRenderer.flipX = false;
            }
            else if (inputHorizontal < -0.1f)
            {
                spriteRenderer.flipX = true;
            }
        }
        
        transform.localScale = new Vector3(escalaJugador, escalaJugador, 1);
        
        if (!usarRigidbody)
        {
            AplicarGravedadManual();
            MoverSinRigidbody();
        }
        
        // Debug health controls
        if (Input.GetKeyDown(KeyCode.H))
        {
            RecibirDaño(10f);
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            Curar(15f);
        }
    }
    
    void FixedUpdate()
    {
        if (usarRigidbody && rb != null && !estaMuerto)
        {
            Mover();
        }
    }
    
    void Mover()
    {
        rb.linearVelocity = new Vector2(inputHorizontal * velocidadMovimiento, rb.linearVelocity.y);
    }
    
    void MoverSinRigidbody()
    {
        Vector3 movimiento = new Vector3(inputHorizontal * velocidadMovimiento * Time.deltaTime, 
                                        velocidadVertical * Time.deltaTime, 0);
        transform.Translate(movimiento);
    }
    
    void AplicarGravedadManual()
    {
        if (!estaEnSuelo)
        {
            velocidadVertical += gravedad * Time.deltaTime;
        }
        else
        {
            velocidadVertical = 0;
        }
    }
    
    void Saltar()
    {
        if (usarRigidbody && rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.isKinematic = false;
            rb.simulated = true;
            
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
        }
        else
        {
            velocidadVertical = fuerzaSalto;
        }
    }
    
    // === HEALTH SYSTEM ===
    
    public void RecibirDaño(float daño)
    {
        if (estaMuerto) return;
        
        float vidaAnterior = vidaActual;
        vidaActual = Mathf.Clamp(vidaActual - daño, 0f, vidaMaxima);
        
        if (barraVida != null)
        {
            barraVida.SetHealth(vidaActual);
        }
        
        OnHealthChanged?.Invoke(vidaActual);
        
        if (vidaActual <= 0f && !estaMuerto)
        {
            Morir();
        }
    }
    
    public void Curar(float cantidadCuracion)
    {
        if (estaMuerto) return;
        
        float vidaAnterior = vidaActual;
        vidaActual = Mathf.Clamp(vidaActual + cantidadCuracion, 0f, vidaMaxima);
        
        if (barraVida != null)
        {
            barraVida.SetHealth(vidaActual);
        }
        
        OnHealthChanged?.Invoke(vidaActual);
    }
    
    public void EstablecerVidaMaxima(float nuevaVidaMaxima)
    {
        vidaMaxima = Mathf.Max(1f, nuevaVidaMaxima);
        vidaActual = Mathf.Clamp(vidaActual, 0f, vidaMaxima);
        
        if (barraVida != null)
        {
            barraVida.SetMaxHealth(vidaMaxima);
        }
    }
    
    void Morir()
    {
        if (estaMuerto) return;
        
        estaMuerto = true;
        
        OnPlayerDeath?.Invoke();
        
        // Disable controls and physics
        enabled = false;
        if (rb != null)
        {
            rb.simulated = false;
            rb.linearVelocity = Vector2.zero;
        }
    }
    
    // Public query methods
    public float GetVidaActual() => vidaActual;
    public float GetVidaMaxima() => vidaMaxima;
    public float GetPorcentajeVida() => vidaActual / vidaMaxima;
    public bool EstaVivo() => !estaMuerto;
    public bool TieneVidaCompleta() => Mathf.Approximately(vidaActual, vidaMaxima);
    
    // Overload with knockback - compatibility method
    public void RecibirDaño(Vector2 direccionKnockback, float fuerzaKnockback)
    {
        RecibirDaño(10f); // Standard damage amount
        
        if (rb != null)
        {
            rb.AddForce(direccionKnockback.normalized * fuerzaKnockback, ForceMode2D.Impulse);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (verificadorSuelo != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(verificadorSuelo.position, radioVerificacion);
        }
        
        if (crearBarraDeVida && !usarBarraUI)
        {
            Gizmos.color = Color.green;
            Vector3 posicionBarra = transform.position + posicionBarraRelativa;
            Gizmos.DrawWireCube(posicionBarra, new Vector3(1f, 0.1f, 0f));
        }
    }
} 