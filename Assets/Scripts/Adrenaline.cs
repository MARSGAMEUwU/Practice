using UnityEngine;

public class Adrenaline : MonoBehaviour
{
    [SerializeField] private float maxAdrenaline = 100f;
    [SerializeField] private float currentAdrenaline = 1f;
    [SerializeField] private float decayRate = 1f;
    [SerializeField] private float killReward = 30f;
    [SerializeField] private float injectionBoost = 50f;
    [SerializeField] private float syringeAmount = 1f;
    public float AdrenalinePercentage => currentAdrenaline / maxAdrenaline;

    void Update()
    {
        if (currentAdrenaline <= 0)
        {
            Debug.Log("вы сдохли Update");
        }
        else if (currentAdrenaline > 0)
        {
            currentAdrenaline -= decayRate * Time.deltaTime;
            currentAdrenaline = Mathf.Clamp(currentAdrenaline, 1f, maxAdrenaline);
            Debug.Log(currentAdrenaline);
            
        }
    }
    
    public void UseSyringe()
    {
        currentAdrenaline += injectionBoost;
        currentAdrenaline = Mathf.Clamp(currentAdrenaline, 1f, maxAdrenaline);
        syringeAmount --;
        Debug.Log($"+{injectionBoost} adrenaline");
    }

    public void KillReward()
    {
        currentAdrenaline += killReward;
        currentAdrenaline = Mathf.Clamp(currentAdrenaline, 1f, maxAdrenaline);
        Debug.Log($"+{killReward} adrenaline");
    }

    public void GameOver()
    {
        
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
