using UnityEngine;

/// <summary>
/// ScriptableObject that holds all game balance numbers in one place
/// Easy to tweak without touching code
/// </summary>
[CreateAssetMenu(fileName = "GameRules", menuName = "Band Manager/Game Rules")]
public class GameRules : ScriptableObject
{
    [Header("Time Settings")]
    [Tooltip("How long each quarter lasts in seconds")]
    public float quarterDuration = 30f; // 30 seconds per quarter

    [Header("Starting Values")]
    public int startingMoney = 500;
    public int startingFans = 50;

    [Header("Action Costs")]
    public int recordCost = 100;
    public int tourCost = 50;

    [Header("Action Effects - Tour")]
    public int tourMoneyMultiplier = 10; // earnings = performance * this
    public int tourFanGain = 20;
    public int tourUnityCost = 10;

    [Header("Action Effects - Practice")]
    public int practiceStatGain = 1;

    [Header("Action Effects - Rest")]
    public int restUnityGain = 15;

    [Header("Win/Lose Thresholds")]
    public int bankruptcyThreshold = 0;
    public int breakupThreshold = 0;
}