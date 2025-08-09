using UnityEngine;

public class BaseZone : MonoBehaviour
{
    [HideInInspector] public bool isHeld = false;
    [HideInInspector] public int holderId = -1;

    public Team baseTeam;            // set to Red or Blue in Inspector
    [Tooltip("Points awarded for touching opponent base")]
    public int baseTouchPoints = 10000;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var pc = other.GetComponent<PlayerController>();
        if (pc == null) return;

        // --- Case A: They entered THEIR OWN base zone --- 
        // (we don’t award points here; holding is manual via the button)
        if (pc.startingTeam == baseTeam)
            return;

        // --- Case B: Unheld opponent base touch ---
        if (!isHeld)
        {
            // Check coin requirement
            if (pc.coinCount >= 5)
            {
                ScoreManager.instance.AddPoints(pc.PlayerId, baseTouchPoints);
                Debug.Log($"{pc.name} touched unheld opponent base → +{baseTouchPoints}");
            }
            else
            {
                Debug.Log($"{pc.name} needs at least 5 coins to score base touch");
            }
            return;
        }

        // --- Case C: Held-base defense (infinite) ---
        if (isHeld && pc.startingTeam != baseTeam)
        {
            // The trespasser loses
            ScoreManager.instance.AddPoints(pc.PlayerId, -baseTouchPoints);
            // The holder gains
            ScoreManager.instance.AddPoints(holderId, baseTouchPoints);
            Debug.Log($"{pc.name} trespassed held {baseTeam} base → {holderId} +{baseTouchPoints}, {pc.PlayerId} -{baseTouchPoints}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var pc = other.GetComponent<PlayerController>();
        if (pc == null) return;

        if (pc.startingTeam == baseTeam)
        {
            pc.isOnOwnBase = false;
            Debug.Log($"{pc.name} left their own base");
        }
    }
}
