using UnityEngine;
using UnityEngine.InputSystem;

public class CraftingMenuController : MonoBehaviour
{
    [Header("—сылки на UI")]
    [SerializeField] private GameObject backgroundPanel; // —сылка на BackgroundPanel

    private bool isMenuOpen = false;

    private void Start()
    {
        // ¬ начале игры меню крафта скрыто
        if (backgroundPanel != null)
            backgroundPanel.SetActive(false);
    }

    private void Update()
    {
        // ѕровер€ем нажатие Tab через новую Input System (как в остальных скриптах проекта)
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ToggleCraftingMenu();
        }
    }

    public void ToggleCraftingMenu()
    {
        if (backgroundPanel == null) return;

        isMenuOpen = !backgroundPanel.activeSelf;
        backgroundPanel.SetActive(isMenuOpen);

        // ”правление курсором: если меню открыто Ч освобождаем мышь, если закрыто Ч пр€чем
        if (isMenuOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
} 