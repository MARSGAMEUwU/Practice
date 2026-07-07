using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AmmoUI : MonoBehaviour
{
    [Header("Ссылки на UI")]
    [SerializeField] private TextMeshProUGUI ammoText;       // Текст "патроны / магазин"
    [SerializeField] private Image reloadProgressImage;      // Заполняющийся прямоугольник

    private void Start()
    {
        // Настраиваем шкалу прогресса на заполнение слева направо
        if (reloadProgressImage != null)
        {
            reloadProgressImage.type = Image.Type.Filled;
            reloadProgressImage.fillMethod = Image.FillMethod.Horizontal;
            reloadProgressImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            reloadProgressImage.fillAmount = 0f;
            reloadProgressImage.gameObject.SetActive(false); // Скрываем в начале
        }
    }

    /// <summary>
    /// Обновляет текст с патронами
    /// </summary>
    public void UpdateAmmo(int current, int max)
    {
        if (ammoText != null)
        {
            ammoText.text = $"[ {current} / {max} ]";
        }
    }

    /// <summary>
    /// Устанавливает прогресс перезарядки (от 0 до 1)
    /// </summary>
    public void SetReloadProgress(float progress)
    {
        if (reloadProgressImage != null)
        {
            reloadProgressImage.gameObject.SetActive(true);
            reloadProgressImage.fillAmount = Mathf.Clamp01(progress);
        }
    }

    /// <summary>
    /// Скрывает шкалу прогресса перезарядки
    /// </summary>
    public void HideReloadProgress()
    {
        if (reloadProgressImage != null)
        {
            reloadProgressImage.fillAmount = 0f;
            reloadProgressImage.gameObject.SetActive(false);
        }
    }
}