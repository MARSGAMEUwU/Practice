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
    [SerializeField] private Material shader;
    [SerializeField] private float maxSaturation = 2f;
    [SerializeField] private float minSaturation = 0.5f;
    [SerializeField] private float maxContrast = 2f;
    [SerializeField] private float minContrast = 1f;
    public float AdrenalinePercentage => currentAdrenaline / maxAdrenaline;
    private float nextInjTime;
    private float currentSaturation;
    private float currentContrast;

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
            
            
        }
        if (useSyringe.IsPressed() && syringeAmount > 0 && Time.time >= nextInjTime) UseSyringe();
    }
    
    public void UseSyringe()
    {
        currentAdrenaline += injectionBoost;
        currentAdrenaline = Mathf.Clamp(currentAdrenaline, 1f, maxAdrenaline);
        syringeAmount --;
        Debug.Log($"+{injectionBoost} adrenaline");
        nextInjTime = Time.time + cooldown;
    }

    public void KillReward()
    {
        if (currentAdrenaline > 5)
        {
            currentAdrenaline += killReward;
            currentAdrenaline = Mathf.Clamp(currentAdrenaline, 1f, maxAdrenaline);
            Debug.Log($"+{killReward} adrenaline");
        }
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
    }

    public void TakeDamage(float damageAmount)
    {
        currentAdrenaline -= damageAmount;
        
        if (currentAdrenaline <= 0)
        {
            GameOver();
            Debug.Log("вы сдохли");
        }
    }
}
