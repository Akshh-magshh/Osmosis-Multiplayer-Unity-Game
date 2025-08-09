using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;


public class PlayerController : MonoBehaviour
{
    public float moveSpeed;

    public bool isMoving;

    public Vector2 input;

    private Animator animator;

    [SerializeField] private LayerMask solidObjectsLayer;

    [SerializeField] private Grid grid;

    public int PlayerId;                 // unique for each player

    public Team startingTeam = Team.None;  // choose Red or Blue in Inspector

    [Header("UI")]
    public Button touchButton;

    [HideInInspector] public int coinCount;
    
    [HideInInspector] public bool isOnOwnBase;

    [Header("Hold Base Button")]
    public Button holdBaseButton;

    [Tooltip("How close you must be to your base to ‘hold’ it")]
    public float baseHoldRadius = 0.5f;


    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        // Register team immediately at startup
        TeamManager.instance.AssignTeam(PlayerId, startingTeam);
        touchButton.onClick.AddListener(OnTouchPressed);

        holdBaseButton.onClick.AddListener(OnHoldBasePressed);
    }

    private void Update()
    {
        if (!isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");


            if (input.x != 0)
            {
                input.y = 0; // Prevent diagonal movement
            }

            if (input != Vector2.zero)
            {
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);

                Vector3 targetPosition = GetTargetPosition();

                if (isWalkable(targetPosition))
                {
                    StartCoroutine(Move(targetPosition));
                }

            }

        }

        animator.SetBool("isMoving", isMoving);

        // If holding the base, skip all movement code
        if (isOnOwnBase)
            return;
    }

    IEnumerator Move(Vector3 targetPosition)
    {
        isMoving = true;

        while ((targetPosition - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;

        isMoving = false;


    }

    private bool isWalkable(Vector3 targetPosition)
    {
        Collider2D collider = Physics2D.OverlapCircle(targetPosition, 0.08f, solidObjectsLayer);
        if (collider != null)
        {
            return false; // There is a solid object in the way
        }
        return true; // The path is clear
    }

    private Vector3 GetTargetPosition()
    {
        // Round input to nearest whole number direction (up, down, left, right)
        Vector3Int moveDir = Vector3Int.RoundToInt(input);

        // Convert current world position to cell coordinates
        Vector3Int currentCell = grid.WorldToCell(transform.position);

        // Add direction to current cell to get destination cell
        Vector3Int targetCell = currentCell + moveDir;

        // Convert back to world position — this is the exact center of the tile
        return grid.GetCellCenterWorld(targetCell);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("GoldenCoin"))
        {
            Destroy(other.gameObject); // Destroy the coin object
            ScoreManager.instance.AddPoints(PlayerId, 1);
        }

        else if (other.CompareTag("Itembox500"))
        {
            Destroy(other.gameObject);
            ScoreManager.instance.AddPoints(PlayerId, 500);
        }

        else if (other.CompareTag("Itembox1000"))
        {
            Destroy(other.gameObject);
            ScoreManager.instance.AddPoints(PlayerId, 1000);
        }

        else if (other.CompareTag("Itembox1500"))
        {
            Destroy(other.gameObject);
            ScoreManager.instance.AddPoints(PlayerId, 1500);
        }

        else if (other.CompareTag("Itembox2000"))
        {
            Destroy(other.gameObject);
            ScoreManager.instance.AddPoints(PlayerId, 2000);
        }
        else if (other.CompareTag("Itembox2500"))
        {
            Destroy(other.gameObject);
            ScoreManager.instance.AddPoints(PlayerId, 2500);
        }
        else if (other.CompareTag("Itembox3000"))
        {
            Destroy(other.gameObject);
            ScoreManager.instance.AddPoints(PlayerId, 3000);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Only care if we hit another player:
        if (!collision.gameObject.CompareTag("Player")) return;

        var other = collision.gameObject.GetComponent<PlayerController>();
        if (other == null) return;

        // Only tag if on opposing teams:
        if (TeamManager.instance.GetTeam(PlayerId) != TeamManager.instance.GetTeam(other.PlayerId))
        {
            Debug.Log($"Tag collision: {PlayerId} hits {other.PlayerId}");
            BattleManager.instance.TagPlayer(PlayerId, other.PlayerId);
        }
    }

    // Called when the player taps the TouchButton.
    // Checks for an opponent in range and initiates battle.
    public void OnTouchPressed()
    {
        // Find all colliders in a small circle around this player
        float radius = 0.5f;  // adjust to your character size
        var hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var hit in hits)
        {
            // Skip non‐players
            if (!hit.CompareTag("Player")) continue;

            // Skip self
            var other = hit.GetComponent<PlayerController>();
            if (other == null || other.PlayerId == this.PlayerId) continue;

            // Only battle if on opposing teams
            if (TeamManager.instance.GetTeam(PlayerId) !=
                TeamManager.instance.GetTeam(other.PlayerId))
            {
                BattleManager.instance.TagPlayer(PlayerId, other.PlayerId);
                Debug.Log($"Battle triggered by TouchButton: {PlayerId} vs {other.PlayerId}");
                return;  // only handle the first opposing player found
            }
        }
        Debug.Log("TouchButton pressed, but no opposing player in range.");
    }

    // Called when the player clicks the Hold Base button.
    // Checks whether we're close enough to our own base, then grants infinite status.
    private void OnHoldBasePressed()
    {
        // Don’t allow re-holding if already active
        if (isOnOwnBase) return;

        // Find every collider within the hold radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, baseHoldRadius);

        foreach (var hit in hits)
        {
            // See if the hit object has a BaseZone component
            BaseZone zone = hit.GetComponent<BaseZone>();
            if (zone == null)
                continue;

            // Only proceed if it's *your* base
            if (zone.baseTeam == startingTeam)
            {
                // 1) Mark this zone as “held” and record who’s holding it
                zone.isHeld = true;
                zone.holderId = PlayerId;

                // 2) Mark yourself as on your own base (locks movement elsewhere)
                isOnOwnBase = true;

                Debug.Log($"{name} is now holding their base (infinite points).");
                return;  // we’re done
            }
        }

        Debug.Log($"{name}: Move closer to your base to hold it.");
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a circle around the player to visualize the base hold radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, baseHoldRadius);
    }

}