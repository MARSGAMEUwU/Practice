using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Adrenaline : MonoBehaviour
{
    [Header("�������� ���������")]
    [SerializeField] private float maxAdrenaline = 100f;
    [SerializeField] public float currentAdrenaline = 4f;
    [SerializeField] private float decayRate = 2f;
    [SerializeField] private float killReward = 20f;

    [Header("������")]
    [SerializeField] private float injectionBoost = 50f;
    [SerializeField] private int syringeAmount = 4;
    [SerializeField] private float cooldown = 5f;
    [SerializeField] private InputAction useSyringe;

    [Header("3D �������� � �������� ��������")]
    [SerializeField] private Animator injectorAnimator;
    [SerializeField] private string injectTriggerName = "Inject";
    [SerializeField] private float injectionDelay = 0.6f;

    [Header("Effects")]
    [SerializeField] private Material shader;
    [SerializeField] private float maxSaturation = 1.7f;
    [SerializeField] private float minSaturation = 0.7f;
    [SerializeField] private float maxContrast = 1.7f;
    [SerializeField] private float minContrast = 0.7f;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float minFov = 70f;
    [SerializeField] private float maxFov = 90f;

    [Header("Music")]
    [SerializeField] public AudioSource track1;
    [SerializeField] public AudioSource track2;
    [SerializeField] public AudioSource track3;
    [SerializeField] public AudioSource track4;
    [SerializeField] public float volume = 0.3f;

    [Header("UI")]
    [SerializeField] private AdrenalineUI adrenalineUI;
    [SerializeField] private SyringeUI syringeUI;

    public float AdrenalinePercentage => currentAdrenaline / maxAdrenaline;

    private float nextInjTime;
    private float currentSaturation;
    private float currentContrast;
    private float currentFov;

    // === ��� ����������� ������� ===
    [Header("������� �������")]
    [Tooltip("�������� ������������ ������� (������ � �������)")]
    [SerializeField] private float healRate = 50f;
    private float pendingHealAmount = 0f;

    private void Awake()
    {
        currentSaturation = 0.5f;
        currentContrast = 1f;
        if (shader != null)
        {
            shader.SetFloat("_Saturation", currentSaturation);
            shader.SetFloat("_Contrast", currentContrast);
        }

        if (track1 != null) track1.volume = 0f;
        if (track2 != null) track2.volume = 0f;
        if (track3 != null) track3.volume = 0f;
        if (track4 != null) track4.volume = 0f;
    }

    private void Start()
    {
        if (InventoryManager.Instance != null)
        {
            currentAdrenaline = InventoryManager.Instance.savedAdrenaline;
            syringeAmount = InventoryManager.Instance.savedSyringes;
        }
        ApplyVisualEffects();
        ApplyMusicVolumes();
    }

    private void OnEnable() { if (useSyringe != null) { useSyringe.Enable(); } }
    private void OnDisable() { if (useSyringe != null) { useSyringe.Disable(); } }

    void Update()
    {
        if (Time.timeScale <= 0f) return;

        // 1. ������������ �������� (������ �� 1 HP)
        if (currentAdrenaline > 1f)
        {
            currentAdrenaline -= decayRate * Time.deltaTime;
            if (currentAdrenaline < 1f) currentAdrenaline = 1f;
        }

        // 2. === ����������� ������� (�� ����) ===
        if (pendingHealAmount > 0f)
        {
            // ������� ���� "�������" � ���� �����
            float poolConsumption = healRate * Time.deltaTime;

            // ��������� ��� ��� ������ (���� ���� �������� �� ���������)
            pendingHealAmount -= poolConsumption;
            if (pendingHealAmount < 0f) pendingHealAmount = 0f;

            // �������, ������� ������� ���������� � HP (�� ������ ���������)
            float actualHeal = Mathf.Min(poolConsumption, maxAdrenaline - currentAdrenaline);

            if (actualHeal > 0f)
            {
                currentAdrenaline += actualHeal;
            }
        }

        // 3. ������� � ������
        if (currentAdrenaline > 0f)
        {
            ApplyVisualEffects();
            volume = SettingsManager.MasterVolume * 0.3f;
            ApplyMusicVolumes();
        }

        // 4. ����
        if (useSyringe.IsPressed() && syringeAmount > 0 && Time.time >= nextInjTime)
        {
            UseSyringe();
        }

        SyncToGlobal();
    }

    private void ApplyVisualEffects()
    {
        currentSaturation = Mathf.Lerp(minSaturation, maxSaturation, AdrenalinePercentage);
        currentContrast = Mathf.Lerp(minContrast, maxContrast, AdrenalinePercentage);
        if (shader != null)
        {
            shader.SetFloat("_Saturation", currentSaturation);
            shader.SetFloat("_Contrast", currentContrast);
        }
        currentFov = Mathf.Lerp(minFov, maxFov, AdrenalinePercentage);
        if (mainCamera != null) mainCamera.fieldOfView = currentFov;
    }

    private void ApplyMusicVolumes()
    {
        if (track1 != null) track1.volume = volume;
        if (track2 != null) track2.volume = Mathf.InverseLerp(10f, 30f, currentAdrenaline) * volume;
        if (track3 != null) track3.volume = Mathf.InverseLerp(30f, 60f, currentAdrenaline) * volume;
        if (track4 != null) track4.volume = Mathf.InverseLerp(60f, 90f, currentAdrenaline) * volume;
    }

    private void SyncToGlobal()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.savedAdrenaline = currentAdrenaline;
            InventoryManager.Instance.savedSyringes = syringeAmount;
        }
    }

    private System.Collections.IEnumerator DelayedHealRoutine()
    {
        yield return new WaitForSecondsRealtime(injectionDelay);
        pendingHealAmount += injectionBoost;
    }

    public void UseSyringe()
    {
        if (injectorAnimator != null) injectorAnimator.SetTrigger(injectTriggerName);
        StartCoroutine(DelayedHealRoutine());
        syringeAmount--;
        nextInjTime = Time.time + cooldown;
        SyncToGlobal();
    }

    public void KillReward()
    {
        pendingHealAmount += killReward;
    }

    public void GameOver()
    {
        UnityEngine.Debug.Log("<color=red>����� �����. ������� � ����...</color>");

        // === ����� ���� ������ ������ ===
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ResetRunData();
        }

        // ��������� ����������, ����� ����� �� ��������
        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null) pc.LockControls();
        SceneManager.LoadScene("GameOver");
    }

    public void TakeDamage(float damageAmount)
    {
        currentAdrenaline -= damageAmount;

        if (adrenalineUI != null) adrenalineUI.TriggerShake();
        SyncToGlobal();

        if (currentAdrenaline <= 0f)
        {
            currentAdrenaline = 0f;
            GameOver();
        }
    }

    public void GetSyringe()
    {
        syringeAmount++;
        syringeAmount = Mathf.Clamp(syringeAmount, 0, 4);
        SyncToGlobal();
    }

    public int GetSyringeAmount() => syringeAmount;
}