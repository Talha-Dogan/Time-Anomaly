using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class CityWalker : MonoBehaviour
{
    [Header("Movement Settings")]
    public float wanderRadius = 40f;
    public float waitTime = 3f;

    [Header("Interaction Settings")]
    public Transform player;
    public float detectionRange = 4f;
    public float waveCooldown = 10f;

    [Header("Smash Settings (UPWARD)")]
    public float hitRange = 3.5f;
    public float smashPower = 30f;
    public float impactDelay = 0.4f;

    [Header("Audio Settings")]
    public AudioClip waveSound;        // "Merhaba" sesi
    public AudioClip hitSound;         // --- YENİ: VURUŞ SESİ (Punch/Hit) ---
    public AudioClip[] idleSounds;     // Rastgele mırıldanmalar
    public float minIdleTime = 5f;
    public float maxIdleTime = 15f;

    // Components
    private NavMeshAgent agent;
    private Animator animator;
    private Rigidbody rb;
    private AudioSource audioSource;

    // Logic Variables
    private float moveTimer;
    private float waveTimer;
    private float idleSoundTimer;
    private bool isWaving = false;
    private bool isDead = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        rb.isKinematic = true;
        rb.mass = 1f;

        idleSoundTimer = Random.Range(minIdleTime, maxIdleTime);
        SetNewDestination();
    }

    void Update()
    {
        if (isDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // --- 0. IDLE SOUND LOGIC ---
        if (!isWaving)
        {
            idleSoundTimer -= Time.deltaTime;
            if (idleSoundTimer <= 0)
            {
                PlayRandomIdleSound();
                idleSoundTimer = Random.Range(minIdleTime, maxIdleTime);
            }
        }

        // --- 1. SMASH LOGIC ---
        if (Input.GetButtonDown("Fire1"))
        {
            if (distanceToPlayer <= hitRange)
            {
                StartCoroutine(SmashSequenceUpward());
                return;
            }
        }

        // --- 2. WAVE LOGIC ---
        if (distanceToPlayer <= detectionRange && waveTimer <= 0)
        {
            PerformWave();
        }

        if (waveTimer > 0) waveTimer -= Time.deltaTime;

        if (isWaving)
        {
            StopAndLookAtPlayer();
            if (waveTimer < (waveCooldown - 2f))
            {
                isWaving = false;
                agent.isStopped = false;
            }
            return;
        }

        // --- 3. MOVEMENT LOGIC ---
        UpdateAnimation();

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            moveTimer += Time.deltaTime;
            if (moveTimer >= waitTime)
            {
                SetNewDestination();
                moveTimer = 0;
            }
        }
    }

    // --- SES FONKSİYONLARI ---
    void PlayRandomIdleSound()
    {
        if (idleSounds.Length > 0 && !audioSource.isPlaying)
        {
            int randomIndex = Random.Range(0, idleSounds.Length);
            audioSource.PlayOneShot(idleSounds[randomIndex]);
        }
    }

    // --- SMASH VE HIT SESİ ---
    System.Collections.IEnumerator SmashSequenceUpward()
    {
        isDead = true;
        agent.isStopped = true;
        audioSource.Stop(); // Konuşuyorsa sussun

        // Sopanın inmesini bekle
        yield return new WaitForSeconds(impactDelay);

        // --- YENİ: VURUŞ SESİNİ ÇAL ---
        if (hitSound != null)
        {
            // PlayOneShot kullanıyoruz ki ses kesilmesin
            audioSource.PlayOneShot(hitSound);
        }

        agent.enabled = false;
        if (animator != null)
        {
            animator.SetTrigger("Dead");
            animator.SetBool("IsWalking", false);
        }

        rb.isKinematic = false;
        rb.velocity = Vector3.zero;

        Vector3 upwardForce = Vector3.up * smashPower;
        Vector3 backwardNudge = (transform.position - player.position).normalized * 5f;

        rb.AddForce(upwardForce + backwardNudge, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 15f, ForceMode.Impulse);

        Destroy(gameObject, 5f);
    }

    void PerformWave()
    {
        isWaving = true;
        waveTimer = waveCooldown;
        agent.isStopped = true;

        if (waveSound != null)
        {
            audioSource.PlayOneShot(waveSound);
        }

        if (animator != null)
        {
            animator.SetTrigger("Wave");
            animator.SetBool("IsWalking", false);
        }
    }

    void StopAndLookAtPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    void SetNewDestination()
    {
        if (!agent.enabled) return;
        Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
        agent.SetDestination(newPos);
    }

    void UpdateAnimation()
    {
        if (animator != null && agent.enabled)
        {
            bool isMoving = agent.velocity.magnitude > 0.1f;
            animator.SetBool("IsWalking", isMoving);
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }
}