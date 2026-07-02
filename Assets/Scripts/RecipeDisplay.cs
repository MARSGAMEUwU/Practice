using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeDisplay : MonoBehaviour
{
    [Header("Слоты рецепта (4 штуки)")]
    [SerializeField] private RecipeItem[] items = new RecipeItem[4];

    public void SetRecipe(int[] costs, InventoryManager manager)
    {
        if (costs == null || costs.Length != 4) return;

        for (int i = 0; i < 4; i++)
        {
            if (items[i] != null)
            {
                bool canAfford = manager != null && manager.GetResource(i) >= costs[i];
                items[i].Setup(costs[i], canAfford);
            }
        }
    }
}

[System.Serializable]
public class RecipeItem
{
    public Image icon;
    public TextMeshProUGUI countText;

    public void Setup(int count, bool canAfford)
    {
        if (icon != null) icon.gameObject.SetActive(count > 0);
        if (countText != null)
        {
            countText.text = "x" + count;
            countText.gameObject.SetActive(count > 0);
            countText.color = canAfford ? Color.white : Color.red;
        }
    }
}