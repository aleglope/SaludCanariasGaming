using UnityEngine;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance;

    public AudioClip hoverClip;
    public AudioClip clickClip;
    public bool sfxEnabled = true; // por defecto true
    public AudioClip scrollClip;
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayHover()
    {
        if (sfxEnabled && hoverClip != null)
            audioSource.PlayOneShot(hoverClip);
    }

    public void PlayClick()
    {
        if (sfxEnabled && clickClip != null)
            audioSource.PlayOneShot(clickClip);
    }
    public void PlayScroll()
    {
        if (sfxEnabled && scrollClip != null)
            audioSource.PlayOneShot(scrollClip);
    }
}
