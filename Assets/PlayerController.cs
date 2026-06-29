using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // ==================== НАСТРОЙКИ ====================
    [Header("Движение")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Обзор")]
    [SerializeField] private float mouseSensitivity = 2f;

    [Header("Гравитация")]
    [SerializeField] private float gravity = 20f;

    [Header("Прыжок")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;

    // ==================== ССЫЛКИ ====================
    private CharacterController controller;
    private Transform cameraTransform;

    // ==================== INPUT ====================
    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpPressed;

    // ==================== СОСТОЯНИЕ ====================
    private float xRotation;
    private Vector3 velocity;
    private float coyoteCounter;
    private float jumpBufferCounter;

    // ==================== ЖИЗНЕННЫЙ ЦИКЛ ====================
    private void Awake()
    {
        // Получаем CharacterController
        controller = GetComponent<CharacterController>();

        // Ищем камеру
        Camera mainCam = Camera.main;
        if (mainCam != null)
            cameraTransform = mainCam.transform;

        // Инициализация Input System
        inputActions = new PlayerInputActions();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        inputActions.Player.Jump.performed += ctx => jumpPressed = true;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        HandleLook();
        HandleMovement();
        HandleJumpAndGravity();
    }

    // ==================== ОБЗОР ====================
    private void HandleLook()
    {
        xRotation -= lookInput.y * mouseSensitivity;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);
    }

    // ==================== ДВИЖЕНИЕ ====================
    private void HandleMovement()
    {
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * moveSpeed * Time.deltaTime);
    }

    // ==================== ПРЫЖОК И ГРАВИТАЦИЯ ====================
    private void HandleJumpAndGravity()
    {
        // Проверяем, стоим ли на земле
        bool isGrounded = controller.isGrounded;

        // КЛЮЧЕВОЙ МОМЕНТ: если на земле и падаем — сбрасываем скорость
        // Это гарантирует, что Move() будет "давить" на пол каждый кадр,
        // и isGrounded будет стабильно возвращать true
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            coyoteCounter = coyoteTime; // Обновляем coyote time
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }

        // Jump buffer: запоминаем нажатие пробела
        if (jumpPressed)
        {
            jumpBufferCounter = jumpBufferTime;
            jumpPressed = false;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // Прыжок — срабатывает, если есть и coyote, и buffer
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * 2f * gravity);
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
        }

        // Применяем гравитацию
        velocity.y -= gravity * Time.deltaTime;

        // Двигаем персонажа — именно этот Move() с velocity.y "прижимает" нас к земле
        controller.Move(velocity * Time.deltaTime);
    }
}