using UnityEngine;

public abstract class Gun : Weapon
{
    [field: Header("Gun")]
    [field: SerializeField, Min(0.01f)]
    public float Recoil { get; private set; }
    [SerializeField, Min(1)]
    int m_BaseMaxReserveMagazines;
    [field: SerializeField, Min(1)]
    public int MagazineSize { get; private set; }
    [field: SerializeField, Min(1)]
    public int StartingMagazines { get; private set; } = 3;
    [field: SerializeField, Min(1)]
    public int AmmoGainedOnMelee { get; private set; }

    public int MaxReserveMagazines => m_BaseMaxReserveMagazines + UpgradeSystem.Instance.GetUpgradeLevel(UpgradeType.AmmoCapacity);

    public int MagazineAmmo { get; protected set; }
    public int ReserveAmmo { get; protected set; }

    public bool NeedsReloading => MagazineAmmo == 0;
    public bool OutOfAmmo => MagazineAmmo + ReserveAmmo == 0;
    public bool MagazineFull => MagazineAmmo == MagazineSize;

    protected virtual void Start()
    {
        MagazineAmmo = MagazineSize;
        AddAmmo((StartingMagazines - 1) * MagazineSize);
    }

    public void AddAmmo(int ammo)
    {
        if (ammo <= 0) return;
        ReserveAmmo = Mathf.Min(ReserveAmmo + ammo, MaxReserveMagazines * MagazineSize);
    }

    public void PickupMagazine() => AddAmmo(MagazineSize);

    public void Reload() => MagazineAmmo += Mathf.Min(MagazineSize - MagazineAmmo, ReserveAmmo);
}
