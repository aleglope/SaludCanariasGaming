using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class MainMenuController : MonoBehaviour
{
    [Header("Paneles animados")]
    [SerializeField] private List<UIPanelEntry> panelesLista;
    private Dictionary<string, UIPanelPopUpAnimator> panelesDict;

    [Header("Iconos de sonido")]
    public Image musicIcon;
    public Image sfxIcon;
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;
    public Sprite sfxOnSprite;
    public Sprite sfxOffSprite;
    [Header("URLs externas")]
    [SerializeField] private List<URLEntry> urlEntries;
    private Dictionary<string, string> urlDictionary;
    private bool musicOn = true;
    private bool sfxOn = true;
    private Locale[] availableLocales;

    private async void Start()
    {
        Debug.Log("Start: Esperando a que se inicialice LocalizationSettings...");
        await LocalizationSettings.InitializationOperation.Task;

        availableLocales = LocalizationSettings.AvailableLocales.Locales.ToArray();
        var spanishLocale = availableLocales.FirstOrDefault(locale => locale.Identifier.Code == "es");
        if (spanishLocale != null)
            LocalizationSettings.Instance.SetSelectedLocale(spanishLocale);

        InicializarDiccionarioPaneles();
        InicializarDiccionarioURLs();
        OcultarTodosLosPaneles();
        LoadPreferences();
        UpdateIcons();
    }
    private void InicializarDiccionarioURLs()
    {
        urlDictionary = urlEntries.ToDictionary(entry => entry.nombre, entry => entry.url);
    }
    private void InicializarDiccionarioPaneles()
    {
        panelesDict = panelesLista.ToDictionary(p => p.nombre, p => p.animator);
    }

    private void OcultarTodosLosPaneles()
    {
        foreach (var panel in panelesDict.Values)
            panel.HideInstantly();
    }

    public void OnPlayButton()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }
    public void OpenWebPage(string nombreURL)
    {
        if (urlDictionary.TryGetValue(nombreURL, out var url))
        {
            Application.OpenURL(url);
        }
        else
        {
            Debug.LogWarning($"URL no encontrada para la clave: {nombreURL}");
        }
    }
    public void OnPanelButton(string nombrePanel)
    {
        if (panelesDict.TryGetValue(nombrePanel, out var animator))
        {
            Debug.Log($"Abrir panel: {nombrePanel}");
            animator.ShowPanel();
        }
        else
        {
            Debug.LogWarning($"Panel desconocido: {nombrePanel}");
        }
    }

    public void OnBackFromPanel(string nombrePanel)
    {
        if (panelesDict.TryGetValue(nombrePanel, out var animator))
        {
            Debug.Log($"Cerrar panel: {nombrePanel}");
            animator.HidePanel();
            SavePreferences();
        }
        else
        {
            Debug.LogWarning($"Panel desconocido: {nombrePanel}");
        }
    }

    public void OnToggleMusic()
    {
        musicOn = !musicOn;
        UpdateIcons();
    }

    public void OnToggleSFX()
    {
        sfxOn = !sfxOn;
        UpdateIcons();
    }

    private void UpdateIcons()
    {
        musicIcon.sprite = musicOn ? musicOnSprite : musicOffSprite;
        sfxIcon.sprite = sfxOn ? sfxOnSprite : sfxOffSprite;

        if (UIAudioManager.Instance != null)
            UIAudioManager.Instance.sfxEnabled = sfxOn;

        if (MusicManager.Instance != null)
            MusicManager.Instance.SetMusicEnabled(musicOn);
    }

    private void SavePreferences()
    {
        PlayerPrefs.SetInt("musicOn", musicOn ? 1 : 0);
        PlayerPrefs.SetInt("sfxOn", sfxOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadPreferences()
    {
        musicOn = PlayerPrefs.GetInt("musicOn", 1) == 1;
        sfxOn = PlayerPrefs.GetInt("sfxOn", 1) == 1;
    }

    [System.Serializable]
    public class UIPanelEntry
    {
        public string nombre;
        public UIPanelPopUpAnimator animator;
    }
    [System.Serializable]
    public class URLEntry
    {
        public string nombre;
        public string url;
    }
}
