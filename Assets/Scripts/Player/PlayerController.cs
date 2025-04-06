using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Fields

    [Header("Player Movement")]
    [SerializeField] private AudioSource jumpSound;
    [SerializeField] private AudioSource obstacleSound;
    [SerializeField] private ParticleSystem jumpEffect;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float movementSpeed = 10f; 
    [SerializeField] private int laneDistance = 3; 

    [Header("Player Animation")]
    private Animator animator;
    private static readonly int IsRunning = Animator.StringToHash("IsRunning");
    private static readonly int IsJumping = Animator.StringToHash("IsJumping");

    [Header("Lane Movement")]
    private int targetLane = 1;

    private CharacterController controller;
    private HealthSystem healthSystem;

    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;

    private bool isJumping;
    private float verticalVelocity;
    private Vector3 moveDirection;
    private float swipeStartTime;

    private const float Gravity = -20f;

    #endregion
    
    private void Awake()
    {
    #if !UNITY_EDITOR
        Application.targetFrameRate = 60;
    #endif
        InitializeComponents();
    }
    private void Start()
    {
        StartRunning();
        transform.position = new Vector3(0, transform.position.y, transform.position.z);
    }
    private void FixedUpdate()
    {
        if (healthSystem.IsDead) return;
        MoveCharacter();
        HandleLaneMovement();
        
        if (controller.isGrounded)
        {
            HandleGroundedState();
        }
        else
        {
            ApplyGravity();
        }
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Obstacle") && !healthSystem.IsInvincible)
        {
            if (obstacleSound != null)
            {
                obstacleSound.PlayOneShot(obstacleSound.clip);
            }
            healthSystem.TakeDamage();
        }
    }
    private void InitializeComponents()
    {
        healthSystem = GetComponent<HealthSystem>();
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }
    private void StartRunning()
    {
        animator.SetBool(IsRunning, true);
    }
    
    #region Movement
    
    public void Jump()
    {
        if (controller.isGrounded)
        {
            verticalVelocity = jumpForce;
            isJumping = true;
            animator.SetBool(IsJumping, true);
            PlayJumpEffect();
        }
    }
    private void PlayJumpEffect()
    {
        if (jumpSound != null)
        {
            jumpSound.PlayOneShot(jumpSound.clip);
        }
        if (jumpEffect != null)
        {
            jumpEffect.Play();
            
        }
    }
    private void HandleGroundedState()
    {
        if (!isJumping)
        {
            verticalVelocity = -0.5f; // Slight downward force to keep grounded
        }
        else
        {
            isJumping = false;
            animator.SetBool(IsJumping, false);
        }
    }
    private void ApplyGravity()
    {
        verticalVelocity += Gravity * Time.deltaTime;
    }
    private void MoveCharacter()
    {
        // Calculate the target position based on the lane
        var targetHorizontalPos = (targetLane - 1) * laneDistance;
        var deltaX = targetHorizontalPos - transform.position.x; 
        var moveHorizontal = deltaX * movementSpeed;

        moveDirection = new Vector3(moveHorizontal, verticalVelocity, GameManager.Instance.GameSpeed);
        if (controller.enabled)
        {
            controller.Move(moveDirection * Time.deltaTime);
        }
    }
    public void MoveLane(int direction)
    {
        var newLane = targetLane + direction;
        //Ensure player can only move between 3 lanes (0, 1, 2)
        if (newLane is >= 0 and <= 2) 
        {
            targetLane = newLane;
            Debug.Log($"Moving to lane: {targetLane}" + $" with speed: {GameManager.Instance.GameSpeed}");
        }
    }
    private void HandleLaneMovement()
    {
        var targetHorizontalPos = (targetLane - 1) * laneDistance;
        var targetPosition = new Vector3(targetHorizontalPos, transform.position.y, transform.position.z);
        var movePosition = Vector3.Lerp(transform.position, targetPosition, movementSpeed * Time.deltaTime);
        transform.position = new Vector3(movePosition.x, transform.position.y, transform.position.z);
    }
    
    #endregion

    public void StopMovement()
    {
        controller.enabled = false;
        animator.SetBool(IsRunning, false);
    }
}