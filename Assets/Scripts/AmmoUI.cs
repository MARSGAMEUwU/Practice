using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AmmoUI : MonoBehaviour
{
    [Header("—сылки на UI")]
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Image reloadProgressImage;

    private void Start()
    {
        if (reloadProgressImage != null)
        {
            reloadProgressImage.type = Image.Type.Filled;
            reloadProgressImage.fillMethod = Image.FillMethod.Horizontal;
            reloadProgressImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            reloadProgressImage.fillAmount = 0f;
            reloadProgressImage.gameObject.SetActive(false);
        }
    }

    public void UpdateAmmo(int current, int max)
    {
        if (ammoText != null)
        {
            ammoText.text = $"[ {current} / {max} ]";
        }
    }


    public void SetReloadProgress(float progress)
    {
        if (reloadProgressImage != null)
        {
            reloadProgressImage.gameObject.SetActive(true);
            reloadProgressImage.fillAmount = Mathf.Clamp01(progress);
        }
    }

    public void HideReloadProgress()
    {
        if (reloadProgressImage != null)
        {
            reloadProgressImage.fillAmount = 0f;
            reloadProgressImage.gameObject.SetActive(false);
        }
    }
    public void SetAmmoUIActive(bool isActive)
    {
        if (ammoText != null)
        {
            ammoText.gameObject.SetActive(isActive);
        }

        if (reloadProgressImage != null)
        {
            reloadProgressImage.gameObject.SetActive(isActive);
        }
    }
}