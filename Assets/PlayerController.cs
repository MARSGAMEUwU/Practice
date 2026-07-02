using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Ссылки на компоненты")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Image staminaBarImage;

    [Header("Input Actions")]
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction lookAction;
    [SerializeField] private InputAction jumpAction;
    [SerializeField] private InputAction sprintAction;

    [Header("Настройки передвижения")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 1.8f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -15f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float groundCheckDistance = 0.4f;

    [Header("Настройки стамины")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 20f;
    [SerializeField] private float staminaRegenRate = 15f;
    [SerializeField] private float cooldownThreshold = 30f;

    [Header("Цвета стамины")]
    [SerializeField] private Color normalStaminaColor = Color.white;
    [SerializeField] private Color cooldownStaminaColor = Color.red;

    [Header("Настройки отдачи")]
    [SerializeField] private float recoilRecoverySpeed = 8f;

    // Приватные переменные
    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation;
    private float currentStamina;
    private float maxStaminaBarWidth;
    private bool isOnCooldown = false;

    private float currentRecoilX = 0f;
    private float currentRecoilY = 0f;

    // Флаг блокировки управления
    private bool isControlsEnabled = true;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (staminaBarImage != null)
        {
            maxStaminaBarWidth = staminaBarImage.rectTransform.rect.width;
            staminaBarImage.color = normalStaminaColor;
        }

        currentStamina = maxStamina;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
    }

    private void Update()
    {
        if (!isControlsEnabled) return;

        HandleLook();
        HandleStaminaAndSprint();
        HandleMovement();
        UpdateRecoil();
    }

    private void HandleLook()
    {
        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        xRotation -= lookInput.y * mouseSensitivity;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation + currentRecoilX, currentRecoilY, 0f);
        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);
    }

    public void AddRecoil(float vertical, float horizontal)
    {
        currentRecoilX += vertical;
        currentRecoilY += horizontal;

        currentRecoilX = Mathf.Clamp(currentRecoilX, -15f, 15f);
        currentRecoilY = Mathf.Clamp(currentRecoilY, -5f, 5f);
    }

    private void UpdateRecoil()
    {
        if (currentRecoilX != 0f || currentRecoilY != 0f)
        {
            currentRecoilX = Mathf.Lerp(currentRecoilX, 0f, recoilRecoverySpeed * Time.deltaTime);
            currentRecoilY = Mathf.Lerp(currentRecoilY, 0f, recoilRecoverySpeed * Time.deltaTime);

            if (Mathf.Abs(currentRecoilX) < 0.01f) currentRecoilX = 0f;
            if (Mathf.Abs(currentRecoilY) < 0.01f) currentRecoilY = 0f;
        }
    }

    private void HandleStaminaAndSprint()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        bool isMovingForward = moveInput.y >= 0f;

        bool wantsToSprint = sprintAction.IsPressed() && isMovingForward && !isOnCooldown && currentStamina > 0f;

        if (wantsToSprint)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;

            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isOnCooldown = true;
            }
        }
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;

            if (isOnCooldown && currentStamina >= cooldownThreshold)
            {
                isOnCooldown = false;
            }
        }

        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        UpdateStaminaUI();
    }

    private void UpdateStaminaUI()
    {
        if (staminaBarImage == null) return;

        float staminaPercent = currentStamina / maxStamina;
        float targetWidth = maxStaminaBarWidth * staminaPercent;

        staminaBarImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
        staminaBarImage.color = isOnCooldown ? cooldownStaminaColor : normalStaminaColor;
    }

    private void HandleMovement()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 moveDir = transform.right * moveInput.x + transform.forward * moveInput.y;

        if (gravity > 0) gravity = -Mathf.Abs(gravity);

        bool isMovingForward = moveInput.y > 0.01f;
        bool isSprinting = sprintAction.IsPressed() && isMovingForward && !isOnCooldown && currentStamina > 0f;
        float currentSpeed = isSprinting ? walkSpeed * sprintMultiplier : walkSpeed;

        if (controller.isGrounded)
        {
            velocity.y = -2f;

            if (jumpAction.WasPressedThisFrame())
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        velocity.y = Mathf.Max(velocity.y, -50f);

        Vector3 horizontalMove = moveDir.normalized * currentSpeed * Time.deltaTime;
        Vector3 verticalMove = new Vector3(0, velocity.y * Time.deltaTime, 0);

        controller.Move(horizontalMove + verticalMove);
    }

    // === Публичные методы для InventoryUI ===

    public void LockControls()
    {
        isControlsEnabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("[PlayerController] Управление заблокировано, курсор разблокирован");
    }

    public void UnlockControls()
    {
        isControlsEnabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("[PlayerController] Управление разблокировано, курсор спрятан");
    }

    public bool IsControlsEnabled => isControlsEnabled;
}