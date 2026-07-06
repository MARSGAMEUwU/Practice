using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Adrenaline : MonoBehaviour
{
    [SerializeField] private float maxAdrenaline = 100f;
    [SerializeField] private float currentAdrenaline = 0f;
    [SerializeField] private float decayRate = 1f;
    [SerializeField] private float killReward = 30f;
    [SerializeField] private float injectionBoost = 50f;
    [SerializeField] private int syringeAmount = 1;
    [SerializeField] private float cooldown = 5f;
    [SerializeField] private InputAction useSyringe;
    [Header("Effects")]
    [SerializeField] private Material shader;
    [SerializeField] private float maxSaturation = 2f;
    [SerializeField] private float minSaturation = 0.5f;
    [SerializeField] private float maxContrast = 2f;
    [SerializeField] private float minContrast = 1f;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float minFov = 30f;
    [SerializeField] private float maxFov = 100f;
    [Header("Music")]
    [SerializeField] private AudioSource track1;
    [SerializeField] private AudioSource track2;
    [SerializeField] private AudioSource track3;
    [SerializeField] private AudioSource track4;
    [SerializeField] private float volume = 0.3f;

    public float AdrenalinePercentage => currentAdrenaline / maxAdrenaline;
    private float nextInjTime;
    private float currentSaturation;
    private float currentContrast;
    private float currentFov;

    private void Awake()
    {
        currentSaturation = 0.5f;
        currentContrast = 1f;
        shader.SetFloat("_Saturation", currentSaturation);
        shader.SetFloat("_Contrast", currentContrast);
    }

    private void OnEnable()
    {
        if (useSyringe != null) {useSyringe.Enable();}
    }
    private void OnDisable()
    {
        if (useSyringe != null) { useSyringe.Disable();}
    }

    void Update()
    {
        if (currentAdrenaline > 0)
        {
            currentAdrenaline -= decayRate * Time.deltaTime;
            currentAdrenaline = Mathf.Clamp(currentAdrenaline, 0f, maxAdrenaline);
            
            currentSaturation = Mathf.Lerp(minSaturation, maxSaturation, AdrenalinePercentage);
            currentContrast = Mathf.Lerp(minContrast, maxContrast, AdrenalinePercentage);
            shader.SetFloat("_Saturation", currentSaturation);
            shader.SetFloat("_Contrast", currentContrast);
            currentFov = Mathf.Lerp(minFov, maxFov, AdrenalinePercentage);
            mainCamera.fieldOfView = currentFov;
        }
        if (useSyringe.IsPressed() && syringeAmount > 0 && Time.time >= nextInjTime) UseSyringe();

        if (track1 != null) track1.volume = volume;
        if (track2 != null) track2.volume = Mathf.InverseLerp(10f, 30f, currentAdrenaline) * volume;
        if (track3 != null) track3.volume = Mathf.InverseLerp(30f, 60f, currentAdrenaline) * volume;
        if (track4 != null) track4.volume = Mathf.InverseLerp(60f, 90f, currentAdrenaline) * volume;
    }

    // Сама корутина для плавного прибавления
    private System.Collections.IEnumerator SmoothHealRoutine(float amountToHeal)
    {
        // 1. Вычисляем, до какой отметки нужно дойти (не превышая maxAdrenaline)
        float targetAdrenaline = Mathf.Clamp(currentAdrenaline + amountToHeal, 0f, maxAdrenaline);

        // 2. Пока текущий адреналин меньше целевого...
        while (currentAdrenaline < targetAdrenaline)
        {
            UnityEngine.Debug.Log(targetAdrenaline);
            // Mathf.MoveTowards плавно двигает значение от текущего к целевому с заданной скоростью
            currentAdrenaline = Mathf.MoveTowards(currentAdrenaline, targetAdrenaline + 1, 50f * Time.deltaTime);

            // 3. САМОЕ ВАЖНОЕ: Ждем один кадр, чтобы игра не зависла
            yield return null;
        }

        // На всякий случай жестко фиксируем значение в конце, чтобы не было дробных погрешностей
        currentAdrenaline = targetAdrenaline;
    }
    public void UseSyringe()
    {
        StartCoroutine(SmoothHealRoutine(injectionBoost));
        syringeAmount --;
        UnityEngine.Debug.Log($"+{injectionBoost} adrenaline");
        nextInjTime = Time.time + cooldown;
    }

    public void KillReward()
    {
        if (currentAdrenaline > 5)
        {
            currentAdrenaline += killReward;
            currentAdrenaline = Mathf.Clamp(currentAdrenaline, 1f, maxAdrenaline);
            UnityEngine.Debug.Log($"+{killReward} adrenaline");
        }
    }

    public void GameOver()
    {
        //Time.timeScale = 0f;
        UnityEngine.Debug.Log("вы сдохли");
    }

    public void TakeDamage(float damageAmount)
    {
        currentAdrenaline -= damageAmount;
        currentAdrenaline = Mathf.Clamp(currentAdrenaline, 0f, maxAdrenaline);
        if (currentAdrenaline <= 0)
        {
            GameOver();
            
        }
    }

    public void GetSyringe()
    {
        syringeAmount++;
        UnityEngine.Debug.Log("+ syringe");
    }
}
