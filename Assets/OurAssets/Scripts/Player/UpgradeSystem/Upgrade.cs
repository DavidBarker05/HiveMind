[System.Serializable]
public enum UpgradeType
{
    None,
    Rifle,
    GrenadeLauncher,
    Shotgun,
    Katana,
    Health,
    Damage,
    ExperienceGain,
    AmmoCapacity
}

[System.Serializable]
public struct Upgrade
{
    public int PointsCost;
    public UpgradeType UpgradeType;
    public UpgradeType RequiredPreviousUpgrade;
}

public static class UpgradeHelper
{
    public static bool IsUpgradeOneTimePurchase(Upgrade upgrade) => upgrade.UpgradeType switch
    {
        UpgradeType.None => throw new System.ArgumentException("Upgrade type cannot be none"),
        UpgradeType.Rifle or UpgradeType.GrenadeLauncher or UpgradeType.Shotgun or UpgradeType.Katana => true,
        UpgradeType.Health or UpgradeType.Damage or UpgradeType.ExperienceGain or UpgradeType.AmmoCapacity => false,
        _ => throw new System.NotImplementedException($"{upgrade.UpgradeType} is not implemented")
    };

    public static string UpgradeDisplayName(Upgrade upgrade) => upgrade.UpgradeType switch
    {
        UpgradeType.None => throw new System.ArgumentException("Upgrade type cannot be none"),
        UpgradeType.Rifle => "Unlock Rifle",
        UpgradeType.GrenadeLauncher => "Unlock Grenade Launcher",
        UpgradeType.Shotgun => "Unlock Shotgun",
        UpgradeType.Katana => "Unlock Katana",
        UpgradeType.Health => "Increase Health",
        UpgradeType.Damage => "Increase Damage",
        UpgradeType.ExperienceGain => "Increase Experience Gained",
        UpgradeType.AmmoCapacity => "Increase Ammo Capacity",
        _ => throw new System.NotImplementedException($"{upgrade.UpgradeType} is not implemented")
    };
}
