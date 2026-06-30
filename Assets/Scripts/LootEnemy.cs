using UnityEngine;

public class LootEnemy : MonoBehaviour
{
    [Header("UI")]
    public string interactionText = "Press E to loot";
    public UnityEngine.UI.Text uiText;   // можно заменить на TextMeshProUGUI

    private bool isPlayerInside = false;

    void Update()
    {
        if (isPlayerInside && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }

        // Обновляем текст подсказки
        if (uiText != null)
        {
            uiText.text = isPlayerInside ? interactionText : "";
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
        }
    }

    void Interact()
    {
        // Здесь должен быть вызов PlayerInventory.LootSpawn()
        // Например: PlayerInventory.Instance.LootSpawn();
        Debug.Log("Loot spawned from " + gameObject.name);

        // Уничтожаем верхний труп (этот объект)
        Destroy(gameObject);
    }
}