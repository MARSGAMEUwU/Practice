using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Ссылки на компоненты")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Image staminaBarImage; // Изменено с RectTransform на Image

    [Header("Input Actions (Перетащите сюда действия из Input Action Asset)")]
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
    [SerializeField] private float cooldownThreshold = 30f; // Порог снятия кулдауна

    [Header("Цвета стамины")]
    [SerializeField] private Color normalStaminaColor = Color.white;
    [SerializeField] private Color cooldownStaminaColor = Color.red;

    // Приватные переменные
    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation;
    private float currentStamina;
    private float maxStaminaBarWidth;
    private bool isOnCooldown = false; // Флаг кулдауна

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (staminaBarImage != null)
        {
            maxStaminaBarWidth = staminaBarImage.rectTransform.rect.width;
            staminaBarImage.color = normalStaminaColor; // Устанавливаем начальный цвет
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
        HandleLook();
        HandleStaminaAndSprint();
        HandleMovement();
    }

    private void HandleLook()
    {
        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        xRotation -= lookInput.y * mouseSensitivity;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);
    }

    private void HandleStaminaAndSprint()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        bool isMovingForward = moveInput.y >= 0f;

        // Спринт возможен только если НЕ в кулдауне
        bool wantsToSprint = sprintAction.IsPressed() && isMovingForward && !isOnCooldown && currentStamina > 0f;

        if (wantsToSprint)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;

            // Проверяем, не закончилась ли стамина
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isOnCooldown = true; // Активируем кулдаун
            }
        }
        else
        {
            // Регенерация стамины
            currentStamina += staminaRegenRate * Time.deltaTime;

            // Если в кулдауне и стамина достигла порога - снимаем кулдаун
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

        // Вычисляем процент оставшейся стамины
        float staminaPercent = currentStamina / maxStamina;

        // Вычисляем новую ширину
        float targetWidth = maxStaminaBarWidth * staminaPercent;

        // Изменяем размер бара
        staminaBarImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);

        // Изменяем цвет в зависимости от состояния кулдауна
        staminaBarImage.color = isOnCooldown ? cooldownStaminaColor : normalStaminaColor;
    }

    private void HandleMovement()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 moveDir = transform.right * moveInput.x + transform.forward * moveInput.y;

        // Убеждаемся, что гравитация всегда отрицательная
        if (gravity > 0) gravity = -Mathf.Abs(gravity);

        // Определяем текущую скорость (с спринтом или без)
        bool isMovingForward = moveInput.y > 0.01f;
        bool isSprinting = sprintAction.IsPressed() && isMovingForward && !isOnCooldown && currentStamina > 0f;
        float currentSpeed = isSprinting ? walkSpeed * sprintMultiplier : walkSpeed;

        // Гравитация и прыжок
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

        // Разделяем горизонтальное и вертикальное движение
        Vector3 horizontalMove = moveDir.normalized * currentSpeed * Time.deltaTime;
        Vector3 verticalMove = new Vector3(0, velocity.y * Time.deltaTime, 0);

        controller.Move(horizontalMove + verticalMove);
    }
}