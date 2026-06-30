using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapon System/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Основное")]
    public string weaponName = "New Weapon";
    public Sprite weaponIcon;
    public GameObject weaponPrefab;
    public WeaponType weaponType = WeaponType.Pistol;

    [Header("Характеристики по редкостям (Common / Rare / Epic / Legendary)")]
    public RarityStats[] statsByRarity = new RarityStats[4];

    [Header("Визуал")]
    public GameObject muzzleFlashPrefab;
    public Sprite[] impactDecals;

    [Header("Цвета редкостей")]
    public Color[] rarityColors = new Color[4]
    {
        Color.white,
        new Color(0.2f, 0.6f, 1f),
        new Color(0.7f, 0.2f, 1f),
        new Color(1f, 0.6f, 0f)
    };

    [Header("Названия редкостей")]
    public string[] rarityNames = new string[4] { "Common", "Rare", "Epic", "Legendary" };

    public RarityStats GetStatsForRarity(WeaponRarity rarity)
    {
        int index = (int)rarity;
        if (index < 0 || index >= statsByRarity.Length) return statsByRarity[0];
        return statsByRarity[index];
    }

    public Color GetRarityColor(WeaponRarity rarity)
    {
        int index = (int)rarity;
        if (index < 0 || index >= rarityColors.Length) return Color.white;
        return rarityColors[index];
    }

    public string GetRarityName(WeaponRarity rarity)
    {
        int index = (int)rarity;
        if (index < 0 || index >= rarityNames.Length) return "Unknown";
        return rarityNames[index];
    }
}

[System.Serializable]
public class RarityStats
{
    public float damage = 10f;
    public float fireRate = 0.15f;
    public float range = 100f;
    public int magazineSize = 30;
    public float reloadTime = 2f;
    public float baseRecoil = 1f;
    public float recoilPerShot = 0.3f;
    public float maxRecoil = 8f;
    public float recoilRecovery = 5f;
    public float baseSpread = 0.5f;
    public float spreadPerShot = 0.2f;
    public float maxSpread = 5f;
    public float spreadRecovery = 3f;
}

public enum WeaponRarity { Common = 0, Rare = 1, Epic = 2, Legendary = 3 }
public enum WeaponType { Pistol, SMG, Shotgun, Rifle }