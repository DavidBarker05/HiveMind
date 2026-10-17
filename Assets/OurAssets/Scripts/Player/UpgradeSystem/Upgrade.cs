[System.Serializable]
public enum UpgradeType
{
    None,
    Rifle,
    GrenadeLauncher,
    Shotgun,
    Health,
    Damage,
    ExperienceGain
}

[System.Serializable]
public struct Upgrade
{
    public int PointsCost;
    public UpgradeType UpgradeType;
    public UpgradeType RequiredPreviousUpgrade;
}
