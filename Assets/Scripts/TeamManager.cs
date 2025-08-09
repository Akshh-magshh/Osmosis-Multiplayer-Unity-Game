using System;
using System.Collections.Generic;
using UnityEngine;

// Defines the two teams
public enum Team { None, Red, Blue }

public class TeamManager : MonoBehaviour
{
    public static TeamManager instance;

    // Maps playerId → assigned Team
    private Dictionary<int, Team> assignments = new Dictionary<int, Team>();

    // Fired whenever a team assignment happens
    public event Action<int, Team> OnTeamAssigned;

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
    /// Assigns the given playerId to a team.
    /// </summary>
    public void AssignTeam(int playerId, Team team)
    {
        assignments[playerId] = team;
        Debug.Log($"Player {playerId} → {team} team");
        OnTeamAssigned?.Invoke(playerId, team);
    }

    /// <summary>
    /// Returns the team for a given playerId (None if not assigned).
    /// </summary>
    public Team GetTeam(int playerId)
    {
        return assignments.TryGetValue(playerId, out Team t) ? t : Team.None;
    }
}
