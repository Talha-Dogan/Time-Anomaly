using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Collider))]
public class MushroomAI : MonoBehaviour
{
    #region Variables

    [Header("Animation Settings")]
    public Animator mushroomAnimator;
    public string walkBoolName = "isWalking";

    [Header("Stats")]
    public int maxHealth = 2;
    public int currentHealth;

    [Header("Recovery Settings")]
    public float stunDuration = 15.0f; // Time to wait before waking up
    public float interactionRange = 3.0f; // Distance required to collect

    [Header("Movement")]
    public float wanderSpeed = 2.0f;
    public float fleeSpeed = 3.5f;
    public float fleeDuration = 4.0f;
    public float wanderRadius = 10f;

    [Header("VFX Settings")]
    public GameObject hitVfxPrefab;
    public GameObject stunImpactVfxPrefab;
    public GameObject sleepingVfxPrefab;
    public float effectOffset = 1.5f;

    [Header("Status")]
    public bool isStunned = false;
    public bool isCollecting = false;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private float timer;
    private bool isFleeing = false;
    private float fleeTimer = 0f;

    // To keep track of the sleeping effect instance so we can destroy it when waking up
    private GameObject currentSleepVfxInstance;
    // To keep track of the recovery process
    private Coroutine recoveryCoroutine;

    #endregion

    #region Unity Methods

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (mushroomAnimator == null)
            mushroomAnimator = GetComponentInChildren<Animator>();

        currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;

        agent.speed = wanderSpeed;
        timer = 5f;
    }

    void Update()
    {
        UpdateAnimation();

        // If collecting, do nothing else
        if (isCollecting) return;

        // If stunned, check for player input to collect
        if (isStunned)
        {
            CheckForCollectionInput();
            return; // Don't move while stunned
        }

        // Normal movement logic
        if (isFleeing)
        {
            RunAwayFromPlayer();
            fleeTimer -= Time.deltaTime;
            if (fleeTimer <= 0)
            {
                isFleeing = false;
                agent.speed = wanderSpeed;
            }
        }
        else
        {
            WanderAround();
        }
    }

    #endregion

    #region Custom Methods

    // New method to handle 'E' input
    void CheckForCollectionInput()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= interactionRange && Input.GetKeyDown(KeyCode.E))
        {
            Collect();
        }
    }

    void UpdateAnimation()
    {
        if (mushroomAnimator == null) return;

        if (isStunned || isCollecting)
        {
            mushroomAnimator.SetBool(walkBoolName, false);
            return;
        }

        bool isMoving = agent.velocity.magnitude > 0.1f;
        mushroomAnimator.SetBool(walkBoolName, isMoving);
    }

    public void TakeDamage(int damageAmount)
    {
        if (isStunned || isCollecting) return;

        currentHealth -= damageAmount;

        if (hitVfxPrefab != null)
        {
            GameObject hitVfx = Instantiate(hitVfxPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            Destroy(hitVfx, 1.0f);
        }

        isFleeing = true;
        fleeTimer = fleeDuration;
        agent.speed = fleeSpeed;
        agent.ResetPath();

        if (currentHealth <= 0)
        {
            GetStunned();
        }
    }

    private void GetStunned()
    {
        if (isStunned) return; // Prevent double stun

        isStunned = true;
        isFleeing = false;

        agent.isStopped = true;
        agent.ResetPath();

        if (mushroomAnimator != null) mushroomAnimator.SetBool(walkBoolName, false);

        Vector3 spawnPos = transform.position + Vector3.up * effectOffset;

        // VFX 1: Impact
        if (stunImpactVfxPrefab != null)
        {
            GameObject impactVfx = Instantiate(stunImpactVfxPrefab, spawnPos, Quaternion.identity);
            Destroy(impactVfx, 2.0f);
        }

        // VFX 2: Sleeping ZZZ (Keep reference to destroy later)
        if (sleepingVfxPrefab != null)
        {
            currentSleepVfxInstance = Instantiate(sleepingVfxPrefab, spawnPos, Quaternion.identity);
            currentSleepVfxInstance.transform.SetParent(transform);
        }

        // Start the recovery countdown
        recoveryCoroutine = StartCoroutine(RecoveryRoutine());
    }

    // Wait for 15 seconds, then wake up if not collected
    private IEnumerator RecoveryRoutine()
    {
        yield return new WaitForSeconds(stunDuration);

        if (isStunned && !isCollecting)
        {
            WakeUp();
        }
    }

    // Restores the mushroom to normal state
    private void WakeUp()
    {
        isStunned = false;
        currentHealth = maxHealth; // Reset health so it can be stunned again later

        // Resume movement
        if (agent != null) agent.isStopped = false;

        // Destroy the ZZZ effect
        if (currentSleepVfxInstance != null)
        {
            Destroy(currentSleepVfxInstance);
        }

        Debug.Log("Mushroom woke up!");
    }

    public void Collect()
    {
        if (!isStunned || isCollecting) return;

        // Stop the recovery timer so it doesn't wake up mid-collection
        if (recoveryCoroutine != null) StopCoroutine(recoveryCoroutine);

        // Destroy ZZZ immediately
        if (currentSleepVfxInstance != null) Destroy(currentSleepVfxInstance);

        StartCoroutine(ShrinkAndCollect());
    }

    private IEnumerator ShrinkAndCollect()
    {
        isCollecting = true;

        // --- SCORE LOGIC (ACTIVATED) ---
        if (GameManager.instance != null)
        {
            Debug.Log("Mushroom: GameManager found, adding score...");
            GameManager.instance.AddScore();
        }
        else
        {
            Debug.LogError("Mushroom: GameManager NOT FOUND! Is the GameManager present in the scene?");
        }
        // -------------------------------

        GetComponent<Collider>().enabled = false;
        if (agent != null) agent.enabled = false;

        if (mushroomAnimator != null) mushroomAnimator.SetBool(walkBoolName, false);

        if (playerTransform == null)
        {
            Destroy(gameObject);
            yield break;
        }

        // PHASE 1: POP UP
        float popDuration = 0.3f;
        float timer = 0f;

        Vector3 groundPos = transform.position;
        Vector3 peakPos = groundPos + Vector3.up * 0.5f;

        Vector3 originalScale = transform.localScale;
        Vector3 midScale = originalScale * 0.3f;

        while (timer < popDuration)
        {
            timer += Time.deltaTime;
            float percent = timer / popDuration;

            transform.position = Vector3.Lerp(groundPos, peakPos, percent);
            transform.localScale = Vector3.Lerp(originalScale, midScale, percent);

            yield return null;
        }

        // PHASE 2: VACUUM TO PLAYER
        float suckDuration = 0.5f;
        timer = 0f;

        Vector3 startFlyPos = transform.position;

        while (timer < suckDuration)
        {
            timer += Time.deltaTime;
            float percent = timer / suckDuration;

            Vector3 handPos = playerTransform.position + (Vector3.up * 0.6f);

            transform.position = Vector3.Lerp(startFlyPos, handPos, percent);
            transform.localScale = Vector3.Lerp(midScale, Vector3.zero, percent);

            yield return null;
        }

        Destroy(gameObject);
    }

    private void WanderAround()
    {
        timer += Time.deltaTime;
        if (timer >= 5.0f)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
            agent.SetDestination(newPos);
            timer = 0;
        }
    }

    private void RunAwayFromPlayer()
    {
        if (playerTransform == null) return;
        Vector3 dirToPlayer = transform.position - playerTransform.position;
        Vector3 newPos = transform.position + dirToPlayer.normalized * 5.0f;
        agent.SetDestination(newPos);
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }

    #endregion
}