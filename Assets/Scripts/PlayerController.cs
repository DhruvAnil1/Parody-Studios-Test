using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Gravity Settings")]
    [SerializeField] private float gravityStrength = 20f;
    [SerializeField] private float gravityTransitionSpeed = 5f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.3f;

    [Header("Fall Detection")]
    [SerializeField] private float fallDistanceThreshold = 20f;

    [Header("References")]
    [SerializeField] private Transform hologramPrefab;
    [SerializeField] private Animator animator;

   
    private CharacterController controller;
    private Transform hologramInstance;

 
    private Vector3 currentGravityDirection = Vector3.down;
    private Vector3 targetGravityDirection = Vector3.down;
    private Vector3 velocity;
    private Vector3 moveVelocity;
    private bool isGrounded;
    private float lastGroundedTime;
    private Vector3 lastGroundedPosition;
    private const float groundedGracePeriod = 0.5f;

  
    private Vector3 selectedGravityDirection = Vector3.down;
    private bool isSelectingGravity;

    private readonly int moveSpeedHash = Animator.StringToHash("MoveSpeed");
    private readonly int isGroundedHash = Animator.StringToHash("IsGrounded");

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (hologramPrefab != null)
        {
            hologramInstance = Instantiate(hologramPrefab);
            hologramInstance.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        CheckGroundStatus();
        HandleGravitySelection();
        HandleMovementInput();
        HandleJump();
        ApplyGravity();
        UpdateAnimations();
    }

   
    private void CheckGroundStatus()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        bool wasGrounded = isGrounded;

        // Use SphereCast for more reliable ground detection
        RaycastHit hit;
        isGrounded = Physics.SphereCast(rayOrigin, groundCheckRadius, currentGravityDirection,
            out hit, groundCheckDistance + 0.15f, groundLayer);

        // Also check with a direct overlap for more reliability
        if (!isGrounded)
        {
            Vector3 spherePos = transform.position + currentGravityDirection * (groundCheckDistance + 0.1f);
            isGrounded = Physics.CheckSphere(spherePos, groundCheckRadius, groundLayer);
        }

        if (isGrounded)
        {
            lastGroundedTime = Time.time;
            lastGroundedPosition = transform.position;
        }
        else
        {
            // Check if player has fallen too far from last grounded position
            float fallDistance = Vector3.Distance(transform.position, lastGroundedPosition);

            if (fallDistance > fallDistanceThreshold && Time.time - lastGroundedTime > groundedGracePeriod)
            {
                GameManager.Instance?.TriggerGameOver("Fell off the platform!");
            }
        }
    }

    
    private void HandleGravitySelection()
    {
        Vector3 newGravityDirection = selectedGravityDirection;
        bool arrowKeyPressed = false;

        // Get camera-relative directions
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        forward.y = 0; forward.Normalize();
        right.y = 0; right.Normalize();

        if (Input.GetKey(KeyCode.UpArrow))
        {
            newGravityDirection = Vector3.down;
            arrowKeyPressed = true;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            newGravityDirection = Vector3.up;
            arrowKeyPressed = true;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            newGravityDirection = -right;
            arrowKeyPressed = true;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            newGravityDirection = right;
            arrowKeyPressed = true;
        }

        if (arrowKeyPressed)
        {
            selectedGravityDirection = newGravityDirection;
            isSelectingGravity = true;
            ShowHologram();
        }
        else
        {
            isSelectingGravity = false;
            HideHologram();
        }

        // Apply gravity change on Enter key
        if (Input.GetKeyDown(KeyCode.Return) && isSelectingGravity)
        {
            targetGravityDirection = selectedGravityDirection;
            velocity = Vector3.zero;
        }
    }

    private void ShowHologram()
    {
        if (hologramInstance == null) return;

        hologramInstance.gameObject.SetActive(true);
        hologramInstance.position = transform.position;

        if (selectedGravityDirection != Vector3.zero)
        {
            hologramInstance.rotation = Quaternion.FromToRotation(Vector3.down, selectedGravityDirection);
        }
    }

   
    private void HideHologram()
    {
        if (hologramInstance != null)
            hologramInstance.gameObject.SetActive(false);
    }


    private void HandleMovementInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 gravityUp = -currentGravityDirection;

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        cameraForward = Vector3.ProjectOnPlane(cameraForward, gravityUp);
        cameraRight = Vector3.ProjectOnPlane(cameraRight, gravityUp);

        if (cameraForward.magnitude > 0.001f) cameraForward.Normalize();
        if (cameraRight.magnitude > 0.001f) cameraRight.Normalize();

        Vector3 moveDirection = (cameraForward * vertical + cameraRight * horizontal);

        if (moveDirection.magnitude > 0.1f)
        {
            moveDirection.Normalize();

            moveVelocity = moveDirection * moveSpeed;
            Vector3 move = moveVelocity * Time.deltaTime;

            controller.Move(move);

            if (!isSelectingGravity)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection, gravityUp);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            moveVelocity = Vector3.zero;
        }
    }

   
    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            float jumpVelocity = jumpForce;
            velocity = -currentGravityDirection * jumpVelocity;

            isGrounded = false;
            lastGroundedTime = Time.time - groundedGracePeriod;

            Debug.Log($"JUMP EXECUTED! Velocity: {velocity.magnitude}, Direction: {-currentGravityDirection}");
        }
    }

  
    private void ApplyGravity()
    {
        currentGravityDirection = Vector3.Slerp(currentGravityDirection, targetGravityDirection,
            gravityTransitionSpeed * Time.deltaTime);

        if (!isGrounded)
        {
            velocity += currentGravityDirection * gravityStrength * Time.deltaTime;
        }
        else
        {
            float damping = 5f;
            velocity = Vector3.Lerp(velocity, Vector3.zero, damping * Time.deltaTime);
        }

        controller.Move(velocity * Time.deltaTime);

        Vector3 gravityUp = -currentGravityDirection;
        Quaternion targetUp = Quaternion.FromToRotation(transform.up, gravityUp) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetUp, gravityTransitionSpeed * Time.deltaTime);
    }

 
    private void UpdateAnimations()
    {
        if (animator == null) return;

        float speed = moveVelocity.magnitude;

        animator.SetFloat(moveSpeedHash, speed);
        animator.SetBool(isGroundedHash, isGrounded);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 spherePos = transform.position + currentGravityDirection * groundCheckDistance;
        Gizmos.DrawWireSphere(spherePos, groundCheckRadius);
        Gizmos.DrawRay(transform.position, currentGravityDirection * (groundCheckDistance + 0.1f));

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, velocity);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, currentGravityDirection * 2f);
    }
}