using UnityEngine;
using System;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    // How many points the winner gains (and loser loses)
    [Tooltip("Points transferred on a successful tag/battle.")]
    public int tagPoints = 500;

    public int baseTouchPoints = 10000;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Resolves a battle when two players touch.
    /// </summary>
    /// <param name="attackerId">The ID of the player who initiated contact</param>
    /// <param name="victimId">The ID of the other player</param>
    public void TagPlayer(int attackerId, int victimId)
    {
        // Check if the victim is on their own base (infinite)
        var victimGO = FindPlayer(victimId);
        if (victimGO != null && victimGO.isOnOwnBase)
        {
            // Attacker loses 10000
            ScoreManager.instance.AddPoints(attackerId, -baseTouchPoints);
            Debug.Log($"Player {attackerId} tried to tag infinite defender {victimId} → -{baseTouchPoints} points");
            return;
        }


        // Don’t battle same-player or unassigned IDs
        if (attackerId == victimId) return;

        // If same team, combine their points then battle the victim alone
        var teamA = TeamManager.instance.GetTeam(attackerId);
        var teamB = TeamManager.instance.GetTeam(victimId);
        int attackerPoints = ScoreManager.instance.GetPoints(attackerId);
        int victimPoints   = ScoreManager.instance.GetPoints(victimId);

        if (teamA == teamB && teamA != Team.None)
        {
            // Combine all same?team members who are touching? For now, just double attacker’s power
            attackerPoints *= 2;
        }

        // Decide winner and loser
        if (attackerPoints > victimPoints)
        {
            // Attacker wins
            ScoreManager.instance.AddPoints(attackerId, tagPoints);
            ScoreManager.instance.AddPoints(victimId, -tagPoints);
            Debug.Log($"Player {attackerId} tagged {victimId}: +{tagPoints} / -{tagPoints}");
        }
        else if (victimPoints > attackerPoints)
        {
            // Victim “tags back”
            ScoreManager.instance.AddPoints(victimId, tagPoints);
            ScoreManager.instance.AddPoints(attackerId, -tagPoints);
            Debug.Log($"Player {victimId} tagged {attackerId}: +{tagPoints} / -{tagPoints}");
        }
        else
        {
            // Tie: no change
            Debug.Log($"Player {attackerId} and {victimId} tied—no points transferred.");
        }
    }

    private PlayerController FindPlayer(int id)
    {
        foreach (var pc in FindObjectsOfType<PlayerController>())
            if (pc.PlayerId == id)
                return pc;
        return null;
    }
}
