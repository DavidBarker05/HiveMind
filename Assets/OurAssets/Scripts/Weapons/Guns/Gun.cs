using UnityEngine;

public abstract class Gun : Weapon
{
    [field: Header("Gun")]
    [field: SerializeField]
    public Transform Barrel { get; private set; }
    [field: SerializeField, Min(0.01f)]
    public float Recoil { get; private set; }
    [field: SerializeField, Min(1)]
    public int MaxReserveAmmo { get; private set; }
    [field: SerializeField, Min(1)]
    public int MagazineSize { get; private set; }
    [field: SerializeField, Min(1)]
    public int StartingMagazines { get; private set; } = 3;
    [field: SerializeField, Min(0.01f)]
    public float AttackRange { get; private set; }

    public int MagazineAmmo { get; protected set; }
    public int ReserveAmmo { get; protected set; }

    public bool NeedsReloading => MagazineAmmo == 0;
    public bool OutOfAmmo => MagazineAmmo + ReserveAmmo == 0;

    void Start()
    {
        MagazineAmmo = MagazineSize;
        ReserveAmmo = (StartingMagazines - 1) * MagazineSize;
    }

    public void PickupMagazine() => ReserveAmmo = Mathf.Min(ReserveAmmo + MagazineSize, MaxReserveAmmo);

    public bool Reload()
    {
        if (ReserveAmmo == 0) return false;
        MagazineAmmo += Mathf.Min(MagazineSize - MagazineAmmo, ReserveAmmo);
        return true;
    }
}
