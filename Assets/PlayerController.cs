using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Ссылки на компоненты")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private RectTransform staminaBarUI; // Ссылка на RectTransform полоски стамины

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
    [SerializeField] private float staminaDrainRate = 20f; // Сколько убывает в секунду
    [SerializeField] private float staminaRegenRate = 15f; // Сколько прибавляется в секунду

    // Приватные переменные
    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation;
    private float currentStamina;
    private float maxStaminaBarWidth;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        // Блокируем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Запоминаем максимальную ширину полоски стамины при старте
        if (staminaBarUI != null)
        {
            maxStaminaBarWidth = staminaBarUI.rect.width;
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

        // Проверка: двигаемся ли мы вперед (от -90 до +90 градусов относительно камеры)
        // В локальных координатах ввода Y - это вперед, X - вправо.
        // Если Y > 0, значит угол находится в диапазоне от -90 до +90 градусов.
        bool isMovingForward = moveInput.y >= 0f;

        bool wantsToSprint = sprintAction.IsPressed() && isMovingForward && currentStamina > 0f;

        if (wantsToSprint)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
        }
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
        }

        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        UpdateStaminaUI();
    }

    private void UpdateStaminaUI()
    {
        if (staminaBarUI == null) return;

        // Вычисляем процент оставшейся стамины
        float staminaPercent = currentStamina / maxStamina;

        // Вычисляем новую ширину
        float targetWidth = maxStaminaBarWidth * staminaPercent;

        // Изменяем размер. Так как Pivot установлен в (0.5, 0.5), 
        // полоска будет уменьшаться симметрично с двух краев к центру.
        staminaBarUI.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
    }

    private void HandleMovement()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 moveDir = transform.right * moveInput.x + transform.forward * moveInput.y;

        // Определяем текущую скорость (с спринтом или без)
        bool isMovingForward = moveInput.y > 0.01f;
        bool isSprinting = sprintAction.IsPressed() && isMovingForward && currentStamina > 0f;
        float currentSpeed = isSprinting ? walkSpeed * sprintMultiplier : walkSpeed;

        // Применяем движение
        controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);

        // Гравитация и прыжок
        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f; // Небольшая сила вниз, чтобы персонаж не "соскальзывал" с рельефа
        }

        if (jumpAction.WasPressedThisFrame() && controller.isGrounded)
        {
            // Формула высоты прыжка: v = sqrt(2 * h * -g)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}