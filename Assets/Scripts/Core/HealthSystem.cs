using System;
using System.Collections;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    #region Fields

    [Header("Health System")] [SerializeField]
    private int maxHealth = 3; // Maximum hearts
    [SerializeField] private UIManager uiManager;
    [SerializeField] private int currentHealth = 3;
    [SerializeField] private float invincibilityDuration = 2f;
    [SerializeField] private float hitBlinkRate = 0.2f; 
    private Renderer playerRenderer;
    private bool isInvincible;
    private bool isDead;
    
    private Animator animator;
    private static readonly int IsDying = Animator.StringToHash("IsDying");
    public bool IsInvincible => isInvincible;
    public bool IsDead {get; set;}

    #endregion

    private void Awake()
    {
        InitializeComponents();
    }
    private void InitializeComponents()
    {
        animator = GetComponent<Animator>();
        playerRenderer = GetComponentInChildren<Renderer>();
        if (uiManager == null)
        {
            Debug.LogError("UIManager is not assigned.");
        }
    }
    private void Start()
    {
        ResetHealth();
        currentHealth = maxHealth;
    }

    #region Health System
    
    public void TakeDamage()
    {
        if (isInvincible || isDead) return;

        currentHealth--;
        Debug.Log($"Player hit!!! Health remaining: {currentHealth}");
        UpdateHealthUI();
        StartCoroutine(currentHealth <= 0 ? Die() : BecomeInvincibleRoutine());
    }
    private void UpdateHealthUI()
    {
        uiManager.UpdateHealthDisplay(currentHealth, maxHealth);
    }
    private IEnumerator BecomeInvincibleRoutine()
    {
        isInvincible = true;
        // Visual feedback - blink the player during invincibility
        if (playerRenderer != null)
        {
            var endTime = Time.time + invincibilityDuration;

            while (Time.time < endTime)
            {
                playerRenderer.enabled = !playerRenderer.enabled;
                yield return new WaitForSeconds(hitBlinkRate);
            }

            playerRenderer.enabled = true;
        }
        else
        {
            yield return new WaitForSeconds(invincibilityDuration);
            Debug.Log("No renderer found for blinking effect.");
        }
        isInvincible = false;
    }
    private void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        isInvincible = false;

        if (playerRenderer != null)
            playerRenderer.enabled = true;

        UpdateHealthUI();
    }
    private IEnumerator Die()
    {
        isDead = true;
        var playerController = GetComponent<PlayerController>();
        playerController.StopMovement();
        animator.SetBool(IsDying, true);
        GameManager.Instance.StopDistance = true;
        //waiting for die animation to complete 
        yield return new WaitForSeconds(3f);
        GameManager.Instance.GameOver();
    }

    #endregion


}