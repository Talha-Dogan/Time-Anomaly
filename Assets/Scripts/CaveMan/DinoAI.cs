using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class DinoAI : MonoBehaviour
{
    [Header("Scene Settings")]
    public string menuSceneName = "MainMenu";

    [Header("Audio References")]
    public DinoAudio dinoAudio;
    public float roarCooldown = 10.0f;
    private float nextRoarTime = 0f;

    [Header("Target Settings")]
    public Transform playerTarget;
    public GameObject playerObject;
    public float attackDistance = 4.0f;

    [Header("Nest / Return Settings")]
    public Transform nestTransform;
    public float timeToLoseInterest = 5.0f;
    [Tooltip("Yuvaya bu kadar yaklaşınca dursun")]
    public float nestStopDistance = 1.5f; // Yuvaya ne kadar yaklaşınca dursun?
    private Vector3 nestPosition;
    private float lostPlayerTimer = 0f;

    // Durumlar
    private bool isReturningToNest = false;
    private bool isAtNest = false;

    [Header("Camera Settings")]
    public GameObject deathCamera;

    [Header("Bone Settings")]
    public Transform neckBone;
    public Transform bitePoint;
    public Vector3 rotationOffset;

    // Private Variables
    private NavMeshAgent agent;
    private Animator anim;
    private bool isGameOver = false;
    private Camera mainCam;
    private bool canSeePlayer = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        mainCam = Camera.main;

        if (dinoAudio == null) dinoAudio = GetComponent<DinoAudio>();
        if (bitePoint == null) bitePoint = transform;

        if (nestTransform != null)
            nestPosition = nestTransform.position;
        else
            nestPosition = transform.position;
    }

    void Update()
    {
        if (isGameOver) return;

        // --- 1. YUVADA BEKLEME DURUMU ---
        if (isAtNest)
        {
            // Hareket etme
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            anim.SetBool("IsWalking", false);

            if (nestTransform != null)
            {
                // Yavaşça dışarı dön
                transform.rotation = Quaternion.Slerp(transform.rotation, nestTransform.rotation, Time.deltaTime * 2.0f);
            }

            // DİKKAT: Buradaki "return" komutunu sildim!
            // Artık kod aşağıya akmaya devam edecek ve Raycast çalışacak.
        }

        // --- 2. EVE DÖNÜŞ YOLCULUĞU ---
        if (isReturningToNest)
        {
            // Mesafeye bak: Yuvaya vardık mı?
            float distToNest = Vector3.Distance(transform.position, nestPosition);

            if (distToNest <= nestStopDistance)
            {
                isReturningToNest = false;
                isAtNest = true; // Yuvaya yerleş

                agent.ResetPath();
                agent.isStopped = true;
                anim.SetBool("IsWalking", false);
            }
        }

        // --- 3. GÖRÜŞ VE KOVALAMA (ARTIK HER DURUMDA ÇALIŞIR) ---
        if (playerTarget != null)
        {
            // Raycast gönderelim
            Vector3 laserOrigin = transform.position + Vector3.up * 2.0f;
            Vector3 targetPoint = playerTarget.position + Vector3.up;
            Vector3 direction = targetPoint - laserOrigin;
            RaycastHit hit;

            // Eğer oyuncuyu görürse
            if (Physics.Raycast(laserOrigin, direction, out hit))
            {
                if (hit.transform.CompareTag("Player") || hit.transform.root.CompareTag("Player"))
                {
                    // ARTIK BURASI ÇALIŞACAK VE DİNO YUVADAN FIRLAYACAK
                    EngagePlayer();
                }
                else
                {
                    // Duvar arkasındaysa
                    HandleLostPlayer();
                }
            }
            else
            {
                // Göremiyorsa
                HandleLostPlayer();
            }
        }
    }

    void EngagePlayer()
    {
        canSeePlayer = true;
        isReturningToNest = false;
        isAtNest = false;
        lostPlayerTimer = 0f;

        if (Time.time >= nextRoarTime)
        {
            if (dinoAudio != null) dinoAudio.PlayRoar();
            nextRoarTime = Time.time + roarCooldown;
        }

        float distance = Vector3.Distance(transform.position, playerTarget.position);

        if (distance <= attackDistance)
        {
            KillPlayer();
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(playerTarget.position);
            anim.SetBool("IsWalking", true);
        }
    }

    void HandleLostPlayer()
    {
        canSeePlayer = false;
        if (isAtNest) return; // Zaten yuvadaysa kayıp işlemi yapma

        lostPlayerTimer += Time.deltaTime;

        if (lostPlayerTimer >= timeToLoseInterest)
        {
            ReturnToNest();
        }
        else
        {
            agent.isStopped = true;
            agent.ResetPath();
            anim.SetBool("IsWalking", false);
        }
    }

    void ReturnToNest()
    {
        if (isReturningToNest || isAtNest) return;

        isReturningToNest = true;

        agent.isStopped = false;
        agent.SetDestination(nestPosition);
        anim.SetBool("IsWalking", true);
    }

    void LateUpdate()
    {
        if (playerTarget != null && !isGameOver && neckBone != null && canSeePlayer)
        {
            Vector3 lookDirection = (playerTarget.position + Vector3.up * 1.2f) - neckBone.position;
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            neckBone.rotation = Quaternion.Slerp(neckBone.rotation, targetRotation, Time.deltaTime * 10f);
            neckBone.Rotate(rotationOffset);
        }
    }

    void KillPlayer()
    {
        if (isGameOver) return;
        isGameOver = true;
        if (dinoAudio != null) dinoAudio.PlayBite();

        agent.isStopped = true;
        anim.SetBool("IsWalking", false);

        if (mainCam != null) mainCam.gameObject.SetActive(false);
        if (deathCamera != null) deathCamera.SetActive(true);
        if (playerObject != null) playerObject.SetActive(false);

        anim.SetTrigger("AttackTrigger");
        Invoke("LoadMainMenu", 3.5f);
    }

    void LoadMainMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }
}