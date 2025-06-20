using UnityEngine;
using System.Collections;

[System.Serializable]
public class VirusInvadersCoronavirusEnemy : MonoBehaviour
{
    [Header("VirusInvaders - Configuración del Enemigo")]
    public float velocidadMovimiento = 2f;
    public float velocidadDescenso = 1f;
    public float vida = 100f;
    public float distanciaDeteccion = 1.5f;
    public float distanciaAtaque = 1.5f;
    public float dañoAtaque = 25f;
    public float tiempoEntreAtaques = 2f;
    
    [Header("VirusInvaders - Configuración Visual")]
    public float velocidadAnimacion = 0.15f;
    public Color colorEnemigo = Color.white;
    public string sortingLayerName = "Default";
    public int sortingOrder = 10;
    
    [Header("VirusInvaders - Configuración de Animación")]
    public string tipoCoronavirus = "classic"; // classic, green, blue-rim-light, red-rim-light
    
    [Header("VirusInvaders - Animación de Muerte")]
    public bool animacionMuerteEspecial = true;
    public float fuerzaMovimientoMuerte = 8f;
    public bool rotarAlMorir = true;
    [Range(0f, 1f)]
    public float probabilidadAnimacionEpica = 0.3f; // 30% de probabilidad por defecto
    
    [Header("VirusInvaders - Timing del Juego")]
    public float delayAntesDeMovimiento = 30f;
    
    // Component references
    private SpriteRenderer spriteRenderer;
    private Transform jugador;
    private Rigidbody2D rb;
    private CircleCollider2D col;
    private bool estaMuerto = false;
    private float vidaActual;
    private float tiempoUltimoAtaque = 0f;
    
    // Game timing
    private float tiempoInicioJuego;
    private bool puedeMoverse = false;
    
    // Animation system
    private Sprite[] spritesIdle;
    private Sprite[] spritesPulse;
    private Sprite[] spritesAttack;
    private Sprite[] spritesHit;
    private Sprite[] spritesDeath;
    private Sprite spriteDefault;
    
    // Animation state
    private int frameActual = 0;
    private float tiempoFrame = 0f;
    private Sprite[] animacionActual;
    private bool animacionLoop = true;
    private string estadoAnimacionActual = "";
    
    // Enemy behavior
    private enum EstadoEnemigo { Idle, Persiguiendo, Atacando, Golpeado, Muriendo }
    private EstadoEnemigo estadoActual = EstadoEnemigo.Idle;
    
    void Start()
    {
        tiempoInicioJuego = Time.time;
        vidaActual = vida;
        
        ConfigurarComponentes();
        BuscarJugador();
        
        CargarSprites();
        IniciarAnimacionSegura();
    }
    
    void ConfigurarComponentes()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // Configure sorting to appear in front
        spriteRenderer.sortingLayerName = sortingLayerName;
        spriteRenderer.sortingOrder = sortingOrder;
        spriteRenderer.color = colorEnemigo;
        
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.linearDamping = 2f;
        rb.angularDamping = 5f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;
        
        col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }
        col.radius = 0.7f;
        col.isTrigger = true;
        
        // Adjust Z position to avoid depth issues
        Vector3 pos = transform.position;
        pos.z = -1f;
        transform.position = pos;
        
        gameObject.tag = "Enemy";
        gameObject.layer = LayerMask.NameToLayer("Default");
        
        CrearSpriteDefault();
    }
    
    void CrearSpriteDefault()
    {
        Texture2D texture = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];
        Vector2 center = new Vector2(32, 32);
        
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= 30)
                {
                    float alpha = 1f - (distance / 30f);
                    pixels[y * 64 + x] = new Color(colorEnemigo.r, colorEnemigo.g, colorEnemigo.b, alpha * 0.8f);
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
    
    void CargarSprites()
    {
        string[] rutasBase = new string[]
        {
            $"VirusInvaders/Coronavirus/virus_spriteanimation/{tipoCoronavirus}/",
            $"Spine/Coronavirus/virus_spriteanimation/{tipoCoronavirus}/",
            $"Coronavirus/virus_spriteanimation/{tipoCoronavirus}/",
            $"virus_spriteanimation/{tipoCoronavirus}/"
        };
        
        foreach (string rutaBase in rutasBase)
        {
            spritesIdle = CargarAnimacion(rutaBase, "idle1");
            if (spritesIdle != null && spritesIdle.Length > 0)
            {
                spritesPulse = CargarAnimacion(rutaBase, "pulse") ?? spritesIdle;
                spritesAttack = CargarAnimacion(rutaBase, "attack") ?? spritesIdle;
                spritesHit = CargarAnimacion(rutaBase, "hit") ?? spritesIdle;
                spritesDeath = CargarAnimacion(rutaBase, "death") ?? spritesIdle;
                
                return;
            }
        }
        
        Debug.LogWarning($"No se pudieron cargar sprites para {tipoCoronavirus}. Usando fallback.");
        spritesIdle = new Sprite[] { spriteDefault };
        spritesPulse = spritesIdle;
        spritesAttack = spritesIdle;
        spritesHit = spritesIdle;
        spritesDeath = spritesIdle;
    }
    
    Sprite[] CargarAnimacion(string rutaBase, string animacion)
    {
        System.Collections.Generic.List<Sprite> sprites = new System.Collections.Generic.List<Sprite>();
        
        string prefijo = rutaBase + $"coronavirus-{tipoCoronavirus}-{animacion}_";
        
        for (int i = 0; i < 30; i++)
        {
            Sprite sprite = Resources.Load<Sprite>(prefijo + i.ToString("00"));
            if (sprite == null)
            {
                sprite = Resources.Load<Sprite>(prefijo + i.ToString());
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
    
    void IniciarAnimacionSegura()
    {
        if (spritesIdle != null && spritesIdle.Length > 0)
        {
            CambiarAnimacion(spritesIdle, true, "idle");
        }
        else
        {
            spriteRenderer.sprite = spriteDefault;
        }
    }
    
    void BuscarJugador()
    {
        if (jugador == null)
        {
            GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
            if (jugadorObj != null)
            {
                jugador = jugadorObj.transform;
            }
            else
            {
                VirusInvadersPlayerController playerController = FindFirstObjectByType<VirusInvadersPlayerController>();
                if (playerController != null)
                {
                    jugador = playerController.transform;
                }
            }
        }
    }
    
    void Update()
    {
        if (estaMuerto || jugador == null) return;
        
        VerificarDelayMovimiento();
        ActualizarAnimacion();
        ActualizarComportamiento();
        MirarHaciaJugador();
    }
    
    void VerificarDelayMovimiento()
    {
        if (!puedeMoverse && Time.time - tiempoInicioJuego >= delayAntesDeMovimiento)
        {
            puedeMoverse = true;
        }
    }
    
    void ActualizarAnimacion()
    {
        if (animacionActual == null || animacionActual.Length == 0) 
        {
            spriteRenderer.sprite = spriteDefault;
            return;
        }
        
        tiempoFrame += Time.deltaTime;
        
        if (tiempoFrame >= velocidadAnimacion)
        {
            frameActual++;
            
            if (frameActual >= animacionActual.Length)
            {
                if (animacionLoop)
                {
                    frameActual = 0;
                }
                else
                {
                    frameActual = animacionActual.Length - 1;
                }
            }
            
            if (frameActual >= 0 && frameActual < animacionActual.Length && animacionActual[frameActual] != null)
            {
                spriteRenderer.sprite = animacionActual[frameActual];
            }
            else
            {
                spriteRenderer.sprite = spriteDefault;
            }
            
            tiempoFrame = 0f;
        }
    }
    
    void CambiarAnimacion(Sprite[] nuevaAnimacion, bool loop, string nombre)
    {
        if (estadoAnimacionActual == nombre && animacionActual == nuevaAnimacion) return;
        
        if (nuevaAnimacion != null && nuevaAnimacion.Length > 0)
        {
            animacionActual = nuevaAnimacion;
            animacionLoop = loop;
            frameActual = 0;
            tiempoFrame = 0f;
            estadoAnimacionActual = nombre;
            
            if (animacionActual[0] != null)
            {
                spriteRenderer.sprite = animacionActual[0];
            }
            else
            {
                spriteRenderer.sprite = spriteDefault;
            }
        }
    }
    
    void ActualizarComportamiento()
    {
        if (estadoActual == EstadoEnemigo.Golpeado || estadoActual == EstadoEnemigo.Muriendo) return;
        
        if (!puedeMoverse)
        {
            if (estadoActual != EstadoEnemigo.Idle)
            {
                estadoActual = EstadoEnemigo.Idle;
                CambiarAnimacion(spritesIdle, true, "idle");
            }
            
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
            return;
        }
        
        float distancia = Vector2.Distance(transform.position, jugador.position);
        
        if (distancia <= distanciaAtaque && Time.time - tiempoUltimoAtaque >= tiempoEntreAtaques)
        {
            if (estadoActual != EstadoEnemigo.Atacando)
            {
                estadoActual = EstadoEnemigo.Atacando;
                CambiarAnimacion(spritesAttack, false, "attack");
                StartCoroutine(EjecutarAtaque());
            }
        }
        else if (distancia <= distanciaDeteccion)
        {
            if (estadoActual != EstadoEnemigo.Persiguiendo)
            {
                estadoActual = EstadoEnemigo.Persiguiendo;
                CambiarAnimacion(spritesPulse, true, "pulse");
            }
            
            Vector2 direccion = (jugador.position - transform.position).normalized;
            if (rb != null)
            {
                rb.linearVelocity = direccion * velocidadMovimiento;
            }
        }
        else
        {
            if (estadoActual != EstadoEnemigo.Idle)
            {
                estadoActual = EstadoEnemigo.Idle;
                CambiarAnimacion(spritesIdle, true, "idle");
            }
            
            // Automatic descent when idle and can move
            if (rb != null)
            {
                rb.linearVelocity = Vector2.down * velocidadDescenso;
            }
        }
    }
    
    IEnumerator EjecutarAtaque()
    {
        tiempoUltimoAtaque = Time.time;
        
        yield return new WaitForSeconds(0.5f);
        
        if (jugador != null)
        {
            VirusInvadersPlayerController playerController = jugador.GetComponent<VirusInvadersPlayerController>();
            if (playerController != null)
            {
                Vector2 direccionKnockback = (jugador.position - transform.position).normalized;
                playerController.RecibirDaño(direccionKnockback, 5f);
            }
        }
    }
    
    void MirarHaciaJugador()
    {
        if (jugador != null && spriteRenderer != null)
        {
            Vector3 direccion = jugador.position - transform.position;
            spriteRenderer.flipX = direccion.x < 0;
        }
    }
    
    public void RecibirDaño(float cantidad)
    {
        if (estaMuerto) return;
        
        vidaActual -= cantidad;
        CambiarAnimacion(spritesHit, false, "hit");
        StartCoroutine(EfectoDaño());
        
        if (vidaActual <= 0)
        {
            Morir();
        }
    }
    
    IEnumerator EfectoDaño()
    {
        yield return new WaitForSeconds(0.2f);
        
        if (!estaMuerto && estadoActual != EstadoEnemigo.Muriendo)
        {
            estadoActual = EstadoEnemigo.Idle;
            CambiarAnimacion(spritesIdle, true, "idle");
        }
    }
    
    void Morir()
    {
        if (estaMuerto) return;
        
        estaMuerto = true;
        estadoActual = EstadoEnemigo.Muriendo;
        
        if (animacionMuerteEspecial)
        {
            // Decidir aleatoriamente si hacer la animación épica
            bool hacerAnimacionEpica = Random.Range(0f, 1f) <= probabilidadAnimacionEpica;
            
            if (hacerAnimacionEpica)
            {
                // Animación épica completa (deambulación + expansión)
                StartCoroutine(AnimacionMuerte());
            }
            else
            {
                // Solo deambulación básica, sin expansión épica
                StartCoroutine(AnimacionMuerteBasica());
            }
        }
        else
        {
            // Si no hay animación especial, destruir inmediatamente
            Destroy(gameObject);
        }
    }
    
    IEnumerator AnimacionMuerte()
    {
        CambiarAnimacion(spritesDeath, false, "death");
        
        // Phase 1: Balloon deflating animation (1.5 seconds)
        yield return StartCoroutine(AnimacionGloboDesinflando());
        
        // Phase 2: Virus expansion that stains the entire screen (1 second)
        yield return StartCoroutine(AnimacionExpansionManchaVirus());
        
        Destroy(gameObject);
    }
    
    IEnumerator AnimacionMuerteBasica()
    {
        CambiarAnimacion(spritesDeath, false, "death");
        
        // Solo deambulación básica, sin expansión épica
        yield return StartCoroutine(AnimacionGloboDesinflando());
        
        // Fade out simple y rápido
        yield return StartCoroutine(AnimacionFadeOutBasico());
        
        Destroy(gameObject);
    }
    
    IEnumerator AnimacionGloboDesinflando()
    {
        float duracion = 1.5f;
        float tiempo = 0f;
        
        // Variables iniciales
        Vector3 escalaInicial = transform.localScale;
        Color colorInicial = spriteRenderer.color;
        
        // Variables para el movimiento errático
        float fuerzaMovimiento = fuerzaMovimientoMuerte;
        float frecuenciaMovimiento = 15f;
        float reduccionFuerza = 0.85f;
        
        // Variables para el cambio de escala
        float amplitudEscala = 0.3f;
        float frecuenciaEscala = 20f;
        
        while (tiempo < duracion)
        {
            float progreso = tiempo / duracion;
            
            // === MOVIMIENTO ERRÁTICO (como globo desinflándose) ===
            Vector2 direccionAleatoria = new Vector2(
                Mathf.Sin(Time.time * frecuenciaMovimiento + Random.Range(0f, Mathf.PI * 2f)),
                Mathf.Cos(Time.time * frecuenciaMovimiento * 0.7f + Random.Range(0f, Mathf.PI * 2f))
            ).normalized;
            
            // La fuerza disminuye con el tiempo
            float fuerzaActual = fuerzaMovimiento * (1f - progreso * 0.8f);
            
            // Aplicar movimiento errático
            if (rb != null)
            {
                rb.linearVelocity = direccionAleatoria * fuerzaActual;
            }
            
            // === ESCALA OSCILANTE (efecto de desinflado) ===
            float factorEscala = 1f + Mathf.Sin(Time.time * frecuenciaEscala) * amplitudEscala * (1f - progreso);
            // Reducir tamaño general gradualmente
            float reduccionGeneral = Mathf.Lerp(1f, 0.3f, progreso);
            
            Vector3 nuevaEscala = escalaInicial * factorEscala * reduccionGeneral;
            transform.localScale = nuevaEscala;
            
            // === CAMBIO DE COLOR (rojizo como si estuviera "explotando") ===
            Color colorMuerte = Color.Lerp(colorInicial, new Color(1f, 0.3f, 0.3f, 0.8f), progreso);
            spriteRenderer.color = colorMuerte;
            
            // === ROTACIÓN ERRÁTICA (opcional) ===
            if (rotarAlMorir)
            {
                float rotacion = Mathf.Sin(Time.time * frecuenciaMovimiento * 1.5f) * 180f * progreso;
                transform.rotation = Quaternion.Euler(0, 0, rotacion);
            }
            
            // Reducir frecuencias gradualmente para efecto de "pérdida de energía"
            frecuenciaMovimiento *= reduccionFuerza;
            frecuenciaEscala *= reduccionFuerza;
            
            tiempo += Time.deltaTime;
            yield return null;
        }
        
        // Detener movimiento al final
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false; // Ahora sí desactivamos el Rigidbody2D
        }
    }
    
    IEnumerator AnimacionExpansionManchaVirus()
    {
        float duracion = 1f;
        float tiempo = 0f;
        Color colorInicial = spriteRenderer.color;
        Vector3 escalaInicial = transform.localScale;
        
        // Target scale to cover the entire screen (massive expansion)
        float escalaMaxima = 25f; // Huge scale to cover screen
        
        while (tiempo < duracion)
        {
            float progreso = tiempo / duracion;
            float escalaPop = 1f;
            
            // Efecto "pop" en los últimos 30% de la animación
            if (progreso > 0.7f)
            {
                float factorPop = (progreso - 0.7f) / 0.3f;
                escalaPop = 1f + Mathf.Sin(factorPop * Mathf.PI) * 0.5f;
            }
            
            // Exponential expansion (starts slow, accelerates)
            float progresoExpansion = Mathf.Pow(progreso, 0.3f);
            float escalaActual = Mathf.Lerp(escalaInicial.x, escalaMaxima, progresoExpansion);
            
            // Aplicar escala con efecto pop
            Vector3 escalaFinal = Vector3.one * escalaActual * escalaPop;
            transform.localScale = escalaFinal;
            
            // Fade out as it expands
            float alpha = Mathf.Lerp(1f, 0f, Mathf.Pow(progreso, 1.5f));
            Color color = colorInicial;
            color.a = alpha;
            spriteRenderer.color = color;
            
            // Optional rotation for more dramatic effect
            transform.Rotate(0, 0, 30f * Time.deltaTime * (1f - progreso));
            
            tiempo += Time.deltaTime;
            yield return null;
        }
        
        // Ensure it's completely invisible at the end
        spriteRenderer.color = new Color(colorInicial.r, colorInicial.g, colorInicial.b, 0f);
    }
    
    IEnumerator AnimacionFadeOutBasico()
    {
        float duracion = 0.5f;
        float tiempo = 0f;
        Color colorInicial = spriteRenderer.color;
        
        while (tiempo < duracion)
        {
            float progreso = tiempo / duracion;
            
            // Simple fade out sin efectos adicionales
            float alpha = Mathf.Lerp(1f, 0f, progreso);
            Color color = colorInicial;
            color.a = alpha;
            spriteRenderer.color = color;
            
            tiempo += Time.deltaTime;
            yield return null;
        }
        
        // Asegurar que quede completamente invisible
        spriteRenderer.color = new Color(colorInicial.r, colorInicial.g, colorInicial.b, 0f);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (estaMuerto) return;
        
        Debug.Log($"VirusInvaders: Enemy collided with {other.name} (Tag: {other.tag})");
        
        if (other.CompareTag("Bullet"))
        {
            Debug.Log("VirusInvaders: Enemy hit by bullet! Creating explosion...");
            RecibirDaño(25f);
            
            VirusInvadersBoomEffect.CreateExplosion(other.transform.position, 0.8f);
            
            // Destroy the bullet
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Player"))
        {
            Debug.Log("VirusInvaders: Enemy touched player!");
            VirusInvadersPlayerController playerController = other.GetComponent<VirusInvadersPlayerController>();
            if (playerController != null)
            {
                Vector2 direccionKnockback = (other.transform.position - transform.position).normalized;
                playerController.RecibirDaño(direccionKnockback, dañoAtaque);
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaDeteccion);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 2f);
    }
    
    // Public methods for control and debugging
    public void CambiarTipoCoronavirus(string nuevoTipo)
    {
        tipoCoronavirus = nuevoTipo;
        CargarSprites();
        IniciarAnimacionSegura();
    }
    
    public bool PuedeMoverse()
    {
        return puedeMoverse;
    }
    
    public void ForzarInicioMovimiento()
    {
        puedeMoverse = true;
        tiempoInicioJuego = Time.time - delayAntesDeMovimiento;
    }
    
    public float TiempoHastaMovimiento()
    {
        return Mathf.Max(0f, delayAntesDeMovimiento - (Time.time - tiempoInicioJuego));
    }
}