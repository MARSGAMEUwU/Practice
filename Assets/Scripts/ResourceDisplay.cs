using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceDisplay : MonoBehaviour
{
    [Header("Слоты ресурсов (4 штуки)")]
    [SerializeField] private ResourceItem[] items = new ResourceItem[4];

    public void UpdateDisplay(int[] amounts, Sprite[] icons)
    {
        for (int i = 0; i < 4 && i < amounts.Length; i++)
        {
            if (items[i] != null)
            {
                Sprite icon = (icons != null && i < icons.Length) ? icons[i] : null;
                items[i].Setup(amounts[i], icon);
            }
        }
    }
}

[System.Serializable]
public class ResourceItem
{
    public Image icon;
    public TextMeshProUGUI countText;

    public void Setup(int count, Sprite iconSprite)
    {
        if (icon != null && iconSprite != null)
        {
            icon.sprite = iconSprite;
            icon.gameObject.SetActive(true);
        }
        if (countText != null)
        {
            countText.text = "x" + count;
        }
    }
}