using System;
using UnityEngine;

public class InputSystem : MonoBehaviour
{
    #region Fields

    [SerializeField] private PlayerController playerController;
    [SerializeField] private HealthSystem healthSystem;
    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private float swipeStartTime;
    private const float SwipeThreshold = 10f; // Minimum swipe distance in pixels
    private const float MaxSwipeTime = 0.5f; // Maximum time for a swipe gesture

    #endregion

    private void Awake()
    {
        if (playerController == null || healthSystem == null)
        {
            Debug.LogError("PlayerController or HealthSystem component is not assigned in the inspector.");
        }
    }

    private void Update()
    {
        if (healthSystem.IsDead) return;
        HandlePlayerInput();
    }

    #region Input Handling

    private void HandlePlayerInput()
    {
        HandleTouchInput();
        HandleKeyboardInput();
    }

    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            playerController.MoveLane(-1);
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            playerController.MoveLane(1);
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            playerController.Jump();
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    BeginTouch(touch);
                    break;

                case TouchPhase.Ended:
                    EndTouch(touch);
                    break;
            }
        }
    }

    private void BeginTouch(Touch touch)
    {
        startTouchPosition = touch.position;
        swipeStartTime = Time.time;
    }

    private void EndTouch(Touch touch)
    {
        endTouchPosition = touch.position;
        var xDifference = endTouchPosition.x - startTouchPosition.x;
        var yDifference = endTouchPosition.y - startTouchPosition.y;

        var elapsedTime = Time.time - swipeStartTime;

        if (elapsedTime <= MaxSwipeTime)
        {
            if (Mathf.Abs(xDifference) > Mathf.Abs(yDifference) && Mathf.Abs(xDifference) > SwipeThreshold)
            {
                HandleHorizontalSwipe(xDifference);
            }
            else if (Mathf.Abs(yDifference) > SwipeThreshold)
            {
                HandleVerticalSwipe(yDifference);
            }
        }
    }

    private void HandleHorizontalSwipe(float xDifference)
    {
        if (xDifference > 0)
            playerController.MoveLane(1); // Right
        else
            playerController.MoveLane(-1); // Left
    }

    private void HandleVerticalSwipe(float yDifference)
    {
        if (yDifference > 0)
            playerController.Jump(); // Up
    }

    #endregion
}