using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public void Start()
    {
        Cursor.lockState = CursorLockMode.None;

        // Делаем курсор видимым
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
