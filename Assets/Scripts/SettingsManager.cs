using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Диапазоны значений")]
    [SerializeField] private float minSensitivity = 0.1f;
    [SerializeField] private float maxSensitivity = 5f;

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
        AudioListener.volume = MasterVolume;
    }

    private void ApplySensitivity()
    {
        Debug.Log($"[Settings] Sensitivity updated: {MouseSensitivity}");
    }

    private void LoadSettings()
    {
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