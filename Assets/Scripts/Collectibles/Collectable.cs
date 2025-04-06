using UnityEngine;

public class Collectable : MonoBehaviour
{
    #region Fields

    [Header("Collectable Settings")]
    [SerializeField] private int scoreValue = 10;
    [SerializeField] private CollectableType type = CollectableType.Coin;
    [SerializeField] private AudioSource collectSource;
    [SerializeField] private float rotationSpeed = 90f;

    private bool isCollected = false;
    private Renderer collectableRenderer;
    private Collider collectableCollider;

    private enum CollectableType
    {
        Coin,
        Gem
    }
    
    #endregion
    private void Awake()
    {
        InitializeAudioSource();
        InitializeComponents();
    }
    private void OnEnable()
    {
        ResetCollectable();
    }
    private void Update()
    {
        if (!isCollected)
        {
            RotateTheCollectable();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!isCollected && other.CompareTag("Player"))
        {
            Collect();
        }
    }
    private void InitializeAudioSource()
    {
        if (collectSource == null)
        {
            Debug.LogWarning("AudioSource component not found");
        }
    }
    private void InitializeComponents()
    {
        collectableRenderer = GetComponent<Renderer>();
        collectableCollider = GetComponent<Collider>();
    }
    private void ResetCollectable()
    {
        isCollected = false;
        SetCollectableActive(true);
    }
    private void RotateTheCollectable()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
    private void Collect()
    {
        isCollected = true;
        AddScore();
        PlayCollectSound();
        SetCollectableActive(false);
    }
    private void AddScore()
    {
        int scoreMultiplier = type == CollectableType.Gem ? 5 : 1;
        GameManager.Instance?.AddScore(scoreValue * scoreMultiplier);
    }
    
    private void PlayCollectSound()
    {
        if (collectSource != null)
        {
            collectSource.PlayOneShot(collectSource.clip);
        }
    }
    private void SetCollectableActive(bool isActive)
    {
        if (collectableRenderer != null)
        {
            collectableRenderer.enabled = isActive;
        }
        if (collectableCollider != null)
        {
            collectableCollider.enabled = isActive;
        }
    }
}