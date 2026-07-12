using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    [Header("Чувствительность")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TMP_InputField sensitivityInput;

    [Header("Громкость")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_InputField volumeInput;

    [Header("Панель")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button closeButton;

    private bool isUpdatingUI = false;

    private void Start()
    {
        if (closeButton != null) closeButton.onClick.AddListener(CloseSettings);

        if (SettingsManager.Instance != null)
        {
            sensitivitySlider.minValue = SettingsManager.Instance.GetMinSensitivity();
            sensitivitySlider.maxValue = SettingsManager.Instance.GetMaxSensitivity();

            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
        }

        sensitivitySlider.onValueChanged.AddListener(OnSensitivitySliderChanged);
        sensitivityInput.onEndEdit.AddListener(OnSensitivityInputChanged);

        volumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);
        volumeInput.onEndEdit.AddListener(OnVolumeInputChanged);

        settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        UpdateUIValues();
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    private void UpdateUIValues()
    {
        if (SettingsManager.Instance == null) return;

        isUpdatingUI = true;

        float sens = SettingsManager.Instance.GetSensitivity();
        sensitivitySlider.value = sens;
        sensitivityInput.text = sens.ToString("F2");

        float vol = SettingsManager.Instance.GetVolume();
        volumeSlider.value = vol;
        volumeInput.text = vol.ToString("F2");

        isUpdatingUI = false;
    }

    private void OnSensitivitySliderChanged(float value)
    {
        if (isUpdatingUI) return;
        isUpdatingUI = true;
        sensitivityInput.text = value.ToString("F2");
        isUpdatingUI = false;

        SettingsManager.Instance.SetSensitivity(value);
    }

    private void OnSensitivityInputChanged(string text)
    {
        if (isUpdatingUI) return;

        if (float.TryParse(text, out float value))
        {
            isUpdatingUI = true;
            value = Mathf.Clamp(value, sensitivitySlider.minValue, sensitivitySlider.maxValue);
            sensitivitySlider.value = value;
            sensitivityInput.text = value.ToString("F2");
            isUpdatingUI = false;

            SettingsManager.Instance.SetSensitivity(value);
        }
        else
        {
            UpdateUIValues();
        }
    }

    private void OnVolumeSliderChanged(float value)
    {
        if (isUpdatingUI) return;
        isUpdatingUI = true;
        volumeInput.text = value.ToString("F2");
        isUpdatingUI = false;

        SettingsManager.Instance.SetVolume(value);
    }

    private void OnVolumeInputChanged(string text)
    {
        if (isUpdatingUI) return;

        if (float.TryParse(text, out float value))
        {
            isUpdatingUI = true;
            value = Mathf.Clamp(value, volumeSlider.minValue, volumeSlider.maxValue);
            volumeSlider.value = value;
            volumeInput.text = value.ToString("F2");
            isUpdatingUI = false;

            SettingsManager.Instance.SetVolume(value);
        }
        else
        {
            UpdateUIValues();
        }
    }
}