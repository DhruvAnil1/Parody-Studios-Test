using UnityEngine;


public class Collectible : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;
    [SerializeField] private ParticleSystem collectEffect;

    [Header("Audio")]
    [SerializeField] private AudioClip collectSound;

    private Vector3 startPosition;
    private AudioSource audioSource;

    private void Start()
    {
        startPosition = transform.position;

        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && collectSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void Update()
    {
        // Rotate the collectible
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Bob up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

 
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }


    private void Collect()
    {
        // Notify game manager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectItem();
        }

        // Play collect effect
        if (collectEffect != null)
        {
            ParticleSystem effect = Instantiate(collectEffect, transform.position, Quaternion.identity);
            Destroy(effect.gameObject, effect.main.duration);
        }

        // Play collect sound
        if (collectSound != null && audioSource != null)
        {
            // Play sound at position and destroy after it finishes
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        // Destroy this collectible
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        // Visualize trigger area in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
