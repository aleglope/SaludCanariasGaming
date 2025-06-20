using UnityEngine;

public class VirusInvadersPlayerShooter : MonoBehaviour
{
    [Header("VirusInvaders - Shooting Configuration")]
    public GameObject prefabBala;
    public Transform puntoDisparo;
    public float cadenciaDisparo = 0.3f;
    public KeyCode teclaDisparo = KeyCode.X;
    
    [Header("VirusInvaders - Bullet Configuration")]
    public Sprite texturaBala;
    public float velocidadBala = 15f;
    public float dañoBala = 50f;
    
    // Private references
    private float tiempoUltimoDisparo = 0f;
    
    void Start()
    {
        ConfigurarComponentes();
        CargarTexturaBala();
    }
    
    void ConfigurarComponentes()
    {
        // Create shoot point if it doesn't exist
        if (puntoDisparo == null)
        {
            GameObject puntoDisparoObj = new GameObject("VirusInvaders_ShootPoint");
            puntoDisparoObj.transform.SetParent(transform);
            puntoDisparoObj.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            puntoDisparo = puntoDisparoObj.transform;
        }
        
        // Create bullet prefab if it doesn't exist
        if (prefabBala == null)
        {
            CrearPrefabBalaSimple();
        }
    }
    
    void CargarTexturaBala()
    {
        if (texturaBala == null)
        {
            texturaBala = Resources.Load<Sprite>("VirusInvaders/Sprites/bullet");
        }
    }
    
    void CrearPrefabBalaSimple()
    {
        // Create basic prefab with essential components only
        GameObject balaObj = new GameObject("VirusInvadersBullet");
        
        // Add only the bullet script
        VirusInvadersBullet scriptBala = balaObj.AddComponent<VirusInvadersBullet>();
        scriptBala.velocidad = velocidadBala;
        scriptBala.daño = dañoBala;
        
        prefabBala = balaObj;
        balaObj.SetActive(false);
    }
    
    void Update()
    {
        // Only process input if this is the Player
        if (CompareTag("Player") && Input.GetKeyDown(teclaDisparo))
        {
            if (Time.time - tiempoUltimoDisparo >= cadenciaDisparo)
            {
                Disparar();
                tiempoUltimoDisparo = Time.time;
            }
        }
    }
    
    public void Disparar()
    {
        if (prefabBala == null || puntoDisparo == null) 
        {
            return;
        }
        
        // Instantiate the syringe
        GameObject nuevaJeringuilla = Instantiate(prefabBala, puntoDisparo.position, Quaternion.identity);
        nuevaJeringuilla.SetActive(true);
        
        // Configure the syringe
        VirusInvadersBullet bullet = nuevaJeringuilla.GetComponent<VirusInvadersBullet>();
        if (bullet != null)
        {
            bullet.velocidad = velocidadBala;
            bullet.daño = dañoBala;
            bullet.ConfigurarSprite(texturaBala);
        }
    }
    

}
