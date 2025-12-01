using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 12f;
    public float crouchSpeed = 2.5f;

    public float turnSmoothTime = 0.1f;
    public float jumpHeight = 1.5f;
    public float gravity = -19.62f;

    [Header("References")]
    public Transform cam;
    // Link to the Audio Script so we can tell it when we Jump
    public PlayerAudioManager audioManager;

    private Animator animator;
    private CharacterController controller;
    private float turnSmoothVelocity;
    private Vector3 velocity;

    // --- STATE VARIABLES (Public so Audio Script can read them) ---
    public bool canMove = true;
    public bool isGrounded;
    public bool isCrouching = false;
    public bool isSprinting = false; // Made public for Audio script

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 1. MOVEMENT LOCK CHECK
        if (!canMove)
        {
            if (animator != null) animator.SetFloat("Speed", 0f);
            if (!controller.isGrounded) velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
            return;
        }

        // 2. GROUND CHECK
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        // 3. INPUTS & STATES
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (Input.GetKeyDown(KeyCode.C)) isCrouching = !isCrouching;

        // Update public sprint variable
        isSprinting = Input.GetKey(KeyCode.LeftShift);

        float targetSpeed = walkSpeed;
        if (isCrouching) targetSpeed = crouchSpeed;
        else if (isSprinting) targetSpeed = sprintSpeed;

        if (animator != null) animator.SetBool("IsCrouching", isCrouching);

        // 4. MOVEMENT LOGIC
        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * targetSpeed * Time.deltaTime);

            float animValue = 0.5f;
            if (isCrouching) animValue = 1f;
            else if (isSprinting) animValue = 1f;
            if (animator != null) animator.SetFloat("Speed", animValue, 0.1f, Time.deltaTime);
        }
        else
        {
            if (animator != null) animator.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);
        }

        // 5. JUMP
        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (animator != null) animator.SetTrigger("Jump");

            // Tell the Audio Manager to play sound
            if (audioManager != null) audioManager.PlayJumpSound();
        }

        // 6. GRAVITY
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // Helper to check if moving (for the audio script)
    public bool IsMoving()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        return Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;
    }

}