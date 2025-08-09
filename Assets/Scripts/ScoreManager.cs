using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [Header("Player 1 UI")]
    public Text coinText;
    public Text scoreText;

    [Header("Player 2 UI")]
    public Text coinTextP2;
    public Text scoreTextP2;

    // Track coins and points per player
    private Dictionary<int, int> coinCounts = new Dictionary<int, int>();
    private Dictionary<int, int> pointCounts = new Dictionary<int, int>();

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        // Initialize UI
        coinText.text = "COINS: 0";
        scoreText.text = "SCORE: 0";
        coinTextP2.text = "COINS: 0";
        scoreTextP2.text = "SCORE: 0";
    }

    /// <summary>
    /// Main scoring API: award 'amount' points (or coins if amount=1) to playerId.
    /// </summary>
    public void AddPoints(int playerId, int amount)
    {
        // Treat amount=1 as a “coin,” else “points”
        if (!coinCounts.ContainsKey(playerId)) coinCounts[playerId] = 0;
        if (!pointCounts.ContainsKey(playerId)) pointCounts[playerId] = 0;

        if (amount == 1)
        {
            // Coin pickup
            coinCounts[playerId]++;
            if (playerId == 1) coinText.text = "COINS: " + coinCounts[playerId];
            else if (playerId == 2) coinTextP2.text = "COINS: " + coinCounts[playerId];
        }
        else
        {
            // Item box or battle points
            pointCounts[playerId] += amount;
            if (playerId == 1) scoreText.text = "SCORE: " + pointCounts[playerId];
            else if (playerId == 2) scoreTextP2.text = "SCORE: " + pointCounts[playerId];
        }

        Debug.Log($"Player {playerId} => Coins: {coinCounts[playerId]}, Points: {pointCounts[playerId]}");
    }

    /// <summary>
    /// Read any player’s totals.
    /// </summary>
    public int GetCoins(int playerId) => coinCounts.TryGetValue(playerId, out var c) ? c : 0;
    public int GetPoints(int playerId) => pointCounts.TryGetValue(playerId, out var p) ? p : 0;
}
