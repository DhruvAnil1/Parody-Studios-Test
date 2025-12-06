using UnityEngine;


public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -5);

    [Header("Camera Settings")]
    [SerializeField] private float followSpeed = 10f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float mousesensitivity = 2f;

    [Header("Collision Settings")]
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float collisionOffset = 0.3f;
    [SerializeField] private LayerMask collisionLayers;

    private Vector3 currentOffset;
    private float currentYaw;
    private float currentPitch;

    private void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        currentOffset = offset;

        // Initialize camera angles
        Vector3 angles = transform.eulerAngles;
        currentYaw = angles.y;
        currentPitch = angles.x;

        // Don't lock cursor at start to allow UI interaction
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleCameraRotation();
        UpdateCameraPosition();
    }

    private void HandleCameraRotation()
    {
        // Only rotate camera when right mouse button is held
        if (Input.GetMouseButton(1))
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            float mouseX = Input.GetAxis("Mouse X") * mousesensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mousesensitivity;

            currentYaw += mouseX;
            currentPitch -= mouseY;
            currentPitch = Mathf.Clamp(currentPitch, -30f, 60f);
        }
        else
        {
            // Unlock cursor when not rotating camera
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    private void UpdateCameraPosition()
    {
        // Calculate desired position based on rotation
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        Vector3 desiredPosition = target.position + rotation * currentOffset;

        // Check for collisions between camera and target
        Vector3 direction = desiredPosition - target.position;
        float distance = direction.magnitude;

        RaycastHit hit;
        if (Physics.Raycast(target.position, direction.normalized, out hit, distance, collisionLayers))
        {
            // Position camera before the collision point
            desiredPosition = target.position + direction.normalized *
                Mathf.Max(hit.distance - collisionOffset, minDistance);
        }

        // Smoothly move camera to desired position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Look at target with slight offset upward
        Vector3 lookTarget = target.position + Vector3.up * 1.5f;
        Quaternion targetRotation = Quaternion.LookRotation(lookTarget - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }


    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}