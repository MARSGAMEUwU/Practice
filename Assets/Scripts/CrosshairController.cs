using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    [Header("Визуальные элементы")]
    [SerializeField] private CircleOutline spreadCircle;
    [SerializeField] private RectTransform centerDot;
    [SerializeField] private RectTransform crossLine1;
    [SerializeField] private RectTransform crossLine2;

    [Header("Настройки размера (Базовые)")]
    [SerializeField] private float dotSize = 4f;
    [SerializeField] private float crossSize = 15f;
    [SerializeField] private float crossThickness = 2f;
    [SerializeField] private float smoothSpeed = 15f;

    [Header("Проецирование на экран (Математика)")]
    [Tooltip("Минимальный радиус круга в пикселях, чтобы он не схлопывался в точку при идеальной точности")]
    [SerializeField] private float minPixelRadius = 3f;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Canvas uiCanvas;

    [Header("Цвета")]
    [SerializeField] private Color circleColor = Color.white;
    [SerializeField] private Color dotColor = Color.white;
    [SerializeField] private Color hitColor = Color.green;
    [SerializeField] private Color killColor = Color.red;

    [Header("Таймеры")]
    [SerializeField] private float hitDisplayTime = 0.2f;
    [SerializeField] private float killDisplayTime = 0.3f;

    [Header("Ссылки")]
    [SerializeField] private WeaponController weaponController;

    private Image dotImage;
    private Image crossLine1Image;
    private Image crossLine2Image;
    private float currentCircleSize;
    private float hitTimer;
    private float killTimer;

    private void Awake()
    {
        dotImage = centerDot.GetComponent<Image>();
        crossLine1Image = crossLine1.GetComponent<Image>();
        crossLine2Image = crossLine2.GetComponent<Image>();

        centerDot.sizeDelta = new Vector2(dotSize, dotSize);
        crossLine1.sizeDelta = new Vector2(crossThickness, crossSize);
        crossLine2.sizeDelta = new Vector2(crossThickness, crossSize);
        crossLine1.localRotation = Quaternion.Euler(0, 0, 45);
        crossLine2.localRotation = Quaternion.Euler(0, 0, -45);

        Color transparent = new Color(0, 0, 0, 0);
        crossLine1Image.color = transparent;
        crossLine2Image.color = transparent;

        spreadCircle.color = circleColor;
        dotImage.color = dotColor;

        currentCircleSize = minPixelRadius * 2f;

        if (playerCamera == null) playerCamera = Camera.main;
        if (uiCanvas == null) uiCanvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        UpdateCircleSize();
        UpdateCrossVisibility();
    }

    private void UpdateCircleSize()
    {
        if (weaponController == null || playerCamera == null) return;

        float currentSpreadDeg = weaponController.GetCurrentSpread();

        float spreadRad = currentSpreadDeg * Mathf.Deg2Rad;
        float halfFovRad = (playerCamera.fieldOfView * 0.5f) * Mathf.Deg2Rad;

        float pixelRadius = (Mathf.Tan(spreadRad) / Mathf.Tan(halfFovRad)) * (Screen.height * 0.5f);

        float canvasScale = uiCanvas != null ? uiCanvas.scaleFactor : 1f;
        float uiRadius = pixelRadius / canvasScale;

        float targetSize = Mathf.Max(uiRadius, minPixelRadius) * 2f;

        currentCircleSize = Mathf.Lerp(currentCircleSize, targetSize, smoothSpeed * Time.deltaTime);

        spreadCircle.rectTransform.sizeDelta = new Vector2(currentCircleSize, currentCircleSize);
    }

    private void UpdateCrossVisibility()
    {
        if (killTimer > 0)
        {
            killTimer -= Time.deltaTime;
            SetCrossColor(killColor);
            if (killTimer <= 0) HideCross();
        }
        else if (hitTimer > 0)
        {
            hitTimer -= Time.deltaTime;
            SetCrossColor(hitColor);
            if (hitTimer <= 0) HideCross();
        }
    }

    private void SetCrossColor(Color color)
    {
        crossLine1Image.color = color;
        crossLine2Image.color = color;
    }

    private void HideCross()
    {
        Color transparent = new Color(0, 0, 0, 0);
        crossLine1Image.color = transparent;
        crossLine2Image.color = transparent;
    }

    public void OnShoot() { }

    public void OnHit()
    {
        hitTimer = hitDisplayTime;
        killTimer = 0;
    }

    public void OnHitKill()
    {
        killTimer = killDisplayTime;
        hitTimer = 0;
    }
}