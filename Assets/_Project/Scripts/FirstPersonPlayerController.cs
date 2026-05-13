using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonPlayerController : MonoBehaviour
{
    [Header("Fall Reset")]
    public float fallResetY = -10f;
    public Vector3 resetPosition = Vector3.zero;

    [Header("References")]
    public Camera playerCamera;

    [Header("Movement")]
    public float walkSpeed = 4.5f;
    public float sprintSpeed = 7.5f;
    public float crouchSpeed = 2.5f;
    public float gravity = -20f;

    [Header("Jump")]
    public float jumpHeight = 1.2f;

    [Header("Crouch")]
    public float standingHeight = 2f;
    public float crouchingHeight = 1.1f;
    public float crouchTransitionSpeed = 10f;
    public float standingCameraHeight = 1.65f;
    public float crouchingCameraHeight = 0.9f;

    [Header("Look")]
    public float mouseSensitivity = 0.08f;
    public float minLookAngle = -80f;
    public float maxLookAngle = 80f;

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference lookAction;
    public InputActionReference jumpAction;
    public InputActionReference sprintAction;
    public InputActionReference crouchAction;

    private CharacterController controller;
    private float verticalLookRotation;
    private float verticalVelocity;
    private bool isCrouching;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        standingHeight = controller.height;
    }

    private void OnEnable()
    {
        EnableAction(moveAction);
        EnableAction(lookAction);
        EnableAction(jumpAction);
        EnableAction(sprintAction);
        EnableAction(crouchAction);
    }

    private void OnDisable()
    {
        DisableAction(moveAction);
        DisableAction(lookAction);
        DisableAction(jumpAction);
        DisableAction(sprintAction);
        DisableAction(crouchAction);
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera != null)
        {
            standingCameraHeight = playerCamera.transform.localPosition.y;
        }
    }

    private void Update()
    {
        Look();
        HandleCrouch();
        Move();
        CheckFallReset();

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Look()
    {
        if (lookAction == null || lookAction.action == null || playerCamera == null)
        {
            return;
        }

        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, minLookAngle, maxLookAngle);

        playerCamera.transform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);
    }

    private void Move()
    {
        if (moveAction == null || moveAction.action == null)
        {
            return;
        }

        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();

        Vector3 move =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        if (move.magnitude > 1f)
        {
            move.Normalize();
        }

        bool isGrounded = controller.isGrounded;

        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        if (isGrounded && jumpAction != null && jumpAction.action != null && jumpAction.action.WasPressedThisFrame())
        {
            if (isCrouching)
            {
                isCrouching = false;
            }
            else
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        verticalVelocity += gravity * Time.deltaTime;

        float currentSpeed = GetCurrentSpeed();

        Vector3 velocity = move * currentSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    private float GetCurrentSpeed()
    {
        if (isCrouching)
        {
            return crouchSpeed;
        }

        bool wantsToSprint =
            sprintAction != null &&
            sprintAction.action != null &&
            sprintAction.action.IsPressed();

        if (wantsToSprint)
        {
            return sprintSpeed;
        }

        return walkSpeed;
    }

    private void HandleCrouch()
    {
        if (crouchAction != null && crouchAction.action != null)
        {
            if (crouchAction.action.WasPressedThisFrame())
            {
                isCrouching = !isCrouching;
            }
        }

        float targetHeight = isCrouching ? crouchingHeight : standingHeight;
        float newHeight = Mathf.Lerp(controller.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);

        controller.height = newHeight;

        Vector3 center = controller.center;
        center.y = controller.height / 2f;
        controller.center = center;

        if (playerCamera != null)
        {
            float targetCameraHeight = isCrouching ? crouchingCameraHeight : standingCameraHeight;

            Vector3 cameraPosition = playerCamera.transform.localPosition;
            cameraPosition.y = Mathf.Lerp(cameraPosition.y, targetCameraHeight, crouchTransitionSpeed * Time.deltaTime);
            playerCamera.transform.localPosition = cameraPosition;
        }
    }

    private void EnableAction(InputActionReference actionReference)
    {
        if (actionReference != null && actionReference.action != null)
        {
            actionReference.action.Enable();
        }
    }

    private void DisableAction(InputActionReference actionReference)
    {
        if (actionReference != null && actionReference.action != null)
        {
            actionReference.action.Disable();
        }
    }

    private void CheckFallReset()
    {
        if (transform.position.y < fallResetY)
        {
            controller.enabled = false;

            transform.position = resetPosition;
            verticalVelocity = 0f;

            controller.enabled = true;
        }
    }
}