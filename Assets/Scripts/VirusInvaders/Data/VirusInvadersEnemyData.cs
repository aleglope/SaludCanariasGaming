using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "VirusInvaders/Enemy Data")]
public class VirusInvadersEnemyData : ScriptableObject
{
    [Header("Basic Configuration")]
    public string enemyName;
    public float health = 100f;
    public float attackDamage = 25f;
    public float moveSpeed = 2f;
    public int pointsValue = 10;
    
    [Header("Visual Configuration")]
    public VirusType virusType = VirusType.Classic;
    public Color enemyColor = Color.white;
    public float scale = 0.5f;
    public string sortingLayerName = "Characters";
    public int sortingOrder = 10;
    
    [Header("Movement Configuration")]
    public MovementType movementType = MovementType.Static;
    public float[] movementParameters = new float[4] { 0.5f, 2f, 1f, 2f }; // amplitude, speed, descend, smoothing
    
    [Header("Combat Configuration")]
    public float detectionRange = 1.5f;
    public float attackRange = 1.5f;
    public float timeBetweenAttacks = 2f;
    
    [Header("Animation Configuration")]
    public float animationSpeed = 0.15f;
    
    [Header("Difficulty Configuration")]
    public int minimumDifficultyLevel = 0;
    
    // Helper method to get virus type string for sprite loading
    public string GetVirusTypeString()
    {
        return virusType switch
        {
            VirusType.Classic => "classic",
            VirusType.Green => "green",
            VirusType.BlueRimLight => "blue-rim-light",
            VirusType.RedRimLight => "red-rim-light",
            _ => "classic"
        };
    }
} 