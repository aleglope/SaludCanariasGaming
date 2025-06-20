using UnityEngine;
using System.Collections.Generic;
using System;

public class VirusInvadersGameManager : MonoBehaviour
{
    [Header("Game Configuration")]
    public VirusInvadersDifficultyData[] difficultyLevels;
    public Transform[] spawnPoints;
    public Transform player;
    
    [Header("Score Configuration")]
    public int currentScore = 0;
    public int currentDifficultyLevel = 0;
    
    // Events
    public static event Action<int> OnScoreChanged;
    public static event Action<int> OnDifficultyChanged;
    public static event Action<VirusInvadersEnemyData> OnEnemyDefeated;
    
    // Private variables
    private float lastSpawnTime;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private VirusInvadersDifficultyData currentDifficulty;
    
    // Singleton pattern
    public static VirusInvadersGameManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        InitializeGame();
    }
    
    void Update()
    {
        HandleEnemySpawning();
        CheckDifficultyProgression();
        CleanupDestroyedEnemies();
    }
    
    void InitializeGame()
    {
        currentScore = 0;
        currentDifficultyLevel = 0;
        
        if (difficultyLevels.Length > 0)
        {
            currentDifficulty = difficultyLevels[0];
        }
        
        // Find player if not assigned
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
    }
    
    void HandleEnemySpawning()
    {
        if (currentDifficulty == null) return;
        
        if (Time.time - lastSpawnTime >= currentDifficulty.spawnRate)
        {
            if (activeEnemies.Count < currentDifficulty.maxEnemiesOnScreen)
            {
                SpawnRandomEnemy();
                lastSpawnTime = Time.time;
            }
        }
    }
    
    void SpawnRandomEnemy()
    {
        if (currentDifficulty.availableEnemies.Length == 0 || spawnPoints.Length == 0) return;
        
        // Select random enemy type and spawn point
        VirusInvadersEnemyData enemyData = currentDifficulty.availableEnemies[
            UnityEngine.Random.Range(0, currentDifficulty.availableEnemies.Length)
        ];
        
        Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
        
        GameObject enemy = CreateEnemy(enemyData, spawnPoint.position);
        activeEnemies.Add(enemy);
    }
    
    GameObject CreateEnemy(VirusInvadersEnemyData data, Vector3 position)
    {
        // Create enemy GameObject
        GameObject enemy = new GameObject($"Enemy_{data.enemyName}");
        enemy.transform.position = position;
        enemy.tag = "Enemy";
        enemy.layer = LayerMask.NameToLayer("Enemies");
        
        // Add the new EnemyController component
        VirusInvadersEnemyController enemyController = enemy.AddComponent<VirusInvadersEnemyController>();
        enemyController.enemyData = data;
        
        return enemy;
    }
    

    
    void CheckDifficultyProgression()
    {
        int targetDifficulty = currentDifficultyLevel;
        
        for (int i = difficultyLevels.Length - 1; i >= 0; i--)
        {
            if (currentScore >= difficultyLevels[i].pointsRequired)
            {
                targetDifficulty = i;
                break;
            }
        }
        
        if (targetDifficulty != currentDifficultyLevel)
        {
            ChangeDifficulty(targetDifficulty);
        }
    }
    
    void ChangeDifficulty(int newDifficultyLevel)
    {
        currentDifficultyLevel = newDifficultyLevel;
        currentDifficulty = difficultyLevels[currentDifficultyLevel];
        
        OnDifficultyChanged?.Invoke(currentDifficultyLevel);
        
        // Update existing enemies with new difficulty modifiers
        UpdateExistingEnemies();
    }
    
    void UpdateExistingEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                VirusInvadersEnemyController enemyController = enemy.GetComponent<VirusInvadersEnemyController>();
                if (enemyController != null && enemyController.enemyData != null)
                {
                    // Apply current difficulty modifiers to existing enemies
                    // The EnemyController will handle the difficulty adjustments automatically
                }
            }
        }
    }
    
    void CleanupDestroyedEnemies()
    {
        activeEnemies.RemoveAll(enemy => enemy == null);
    }
    
    public void AddScore(int points)
    {
        currentScore += points;
        OnScoreChanged?.Invoke(currentScore);
    }
    
    public void EnemyDefeated(VirusInvadersEnemyData enemyData)
    {
        AddScore(enemyData.pointsValue);
        OnEnemyDefeated?.Invoke(enemyData);
    }
}
