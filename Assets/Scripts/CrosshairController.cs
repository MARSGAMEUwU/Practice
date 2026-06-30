using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    [Header("Основные элементы")]
    [SerializeField] private CircleOutline spreadCircle; // Кольцо разброса (наш новый компонент)
    [SerializeField] private RectTransform centerDot;    // Точка в центре
    [SerializeField] private RectTransform crossLine1;   // Диагональная линия 1 (45°)
    [SerializeField] private RectTransform crossLine2;   // Диагональная линия 2 (-45°)

    [Header("Настройки размера")]
    [SerializeField] private float minCircleSize = 20f;
    [SerializeField] private float maxCircleSize = 300f;
    [SerializeField] private float dotSize = 4f;
    [SerializeField] private float crossSize = 15f;
    [SerializeField] private float crossThickness = 2f;
    [SerializeField] private float smoothSpeed = 15f;

    [Header("Цвета")]
    [SerializeField] private Color circleColor = Color.white;
    [SerializeField] private Color dotColor = Color.white;
    [SerializeField] private Color hitColor = Color.green;
    [SerializeField] private Color killColor = Color.red;

    [Header("Время отображения")]
    [SerializeField] private float hitDisplayTime = 0.2f;
    [SerializeField] private float killDisplayTime = 0.3f;

    [Header("Ссылки")]
    [SerializeField] private WeaponController weaponController;

    // Приватные переменные
    private Image dotImage;
    private Image crossLine1Image;
    private Image crossLine2Image;

    private float currentCircleSize;
    private float targetCircleSize;

    private float hitTimer;
    private float killTimer;

    private void Awake()
    {
        dotImage = centerDot.GetComponent<Image>();
        crossLine1Image = crossLine1.GetComponent<Image>();
        crossLine2Image = crossLine2.GetComponent<Image>();

        // Настраиваем размеры
        centerDot.sizeDelta = new Vector2(dotSize, dotSize);
        crossLine1.sizeDelta = new Vector2(crossThickness, crossSize);
        crossLine2.sizeDelta = new Vector2(crossThickness, crossSize);

        // Поворот линий на 45° и -45°
        crossLine1.localRotation = Quaternion.Euler(0, 0, 45);
        crossLine2.localRotation = Quaternion.Euler(0, 0, -45);

        // Скрываем перекрестие
        Color transparent = new Color(0, 0, 0, 0);
        crossLine1Image.color = transparent;
        crossLine2Image.color = transparent;

        // Начальный цвет кольца
        spreadCircle.color = circleColor;
        dotImage.color = dotColor;

        currentCircleSize = minCircleSize;
        targetCircleSize = minCircleSize;
    }

    private void Update()
    {
        UpdateCircleSize();
        UpdateCrossVisibility();
    }

    private void UpdateCircleSize()
    {
        if (weaponController == null) return;

        float currentSpread = weaponController.GetCurrentSpread();
        float maxSpread = weaponController.GetMaxSpread();
        float spreadNormalized = maxSpread > 0 ? currentSpread / maxSpread : 0f;

        targetCircleSize = Mathf.Lerp(minCircleSize, maxCircleSize, spreadNormalized);
        currentCircleSize = Mathf.Lerp(currentCircleSize, targetCircleSize, smoothSpeed * Time.deltaTime);

        spreadCircle.rectTransform.sizeDelta = new Vector2(currentCircleSize, currentCircleSize);
    }

    private void UpdateCrossVisibility()
    {
        // Приоритет: Kill > Hit
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