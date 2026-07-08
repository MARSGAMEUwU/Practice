using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Диапазоны значений")]
    [SerializeField] private float minSensitivity = 0.1f;
    [SerializeField] private float maxSensitivity = 5f;

    // Статические поля, чтобы любой скрипт (например, камера) мог быстро получить значение
    public static float MouseSensitivity { get; private set; } = 1f;
    public static float MasterVolume { get; private set; } = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Делаем синглтон вечным, чтобы настройки не сбрасывались при переходе между сценами
        DontDestroyOnLoad(gameObject);

        LoadSettings();
    }

    public void SetSensitivity(float value)
    {
        MouseSensitivity = Mathf.Clamp(value, minSensitivity, maxSensitivity);
        PlayerPrefs.SetFloat("MouseSensitivity", MouseSensitivity);
        ApplySensitivity();
    }

    public void SetVolume(float value)
    {
        MasterVolume = Mathf.Clamp(value, 0f, 1f);
        PlayerPrefs.SetFloat("MasterVolume", MasterVolume);
        ApplyVolume();
    }

    private void ApplyVolume()
    {
        // Глобальное изменение громкости всех AudioSource в игре
        AudioListener.volume = MasterVolume;
    }

    private void ApplySensitivity()
    {
        // Здесь мы просто логируем. 
        // Твой скрипт камеры/контроллера сам прочитает статическое поле SettingsManager.MouseSensitivity.
        Debug.Log($"[Settings] Sensitivity updated: {MouseSensitivity}");
    }

    private void LoadSettings()
    {
        // Читаем из памяти. Если значений нет, берем дефолтные (1.0)
        MouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);

        ApplyVolume();
    }

    // Геттеры для UI
    public float GetSensitivity() => MouseSensitivity;
    public float GetVolume() => MasterVolume;
    public float GetMinSensitivity() => minSensitivity;
    public float GetMaxSensitivity() => maxSensitivity;
}