using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public bool musicEnabled = true;
    private AudioSource audioSource;

    // Diccionario: escena → música
    [System.Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public AudioClip musicClip;
    }

    public SceneMusic[] sceneMusicList;
    private Dictionary<string, AudioClip> musicByScene;
    private string currentScene = "";
    private AudioClip currentClip = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.playOnAwake = false;

            musicByScene = new Dictionary<string, AudioClip>();
            foreach (var item in sceneMusicList)
            {
                if (!musicByScene.ContainsKey(item.sceneName))
                    musicByScene.Add(item.sceneName, item.musicClip);
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentScene = scene.name;
        UpdateMusicForScene(currentScene);
    }

    private void Start()
    {
        currentScene = SceneManager.GetActiveScene().name;
        UpdateMusicForScene(currentScene);
    }

    private void UpdateMusicForScene(string sceneName)
    {
        if (!musicByScene.ContainsKey(sceneName))
        {
            Debug.LogWarning("No music assigned for scene: " + sceneName);
            return;
        }

        AudioClip newClip = musicByScene[sceneName];

        if (newClip == currentClip)
            return; // No cambiar si ya es la actual

        currentClip = newClip;
        audioSource.clip = currentClip;

        if (musicEnabled)
            audioSource.Play();
    }

    public void SetMusicEnabled(bool enabled)
    {
        musicEnabled = enabled;

        if (!enabled)
            audioSource.Pause();
        else if (!audioSource.isPlaying && currentClip != null)
            audioSource.Play();
    }
}
