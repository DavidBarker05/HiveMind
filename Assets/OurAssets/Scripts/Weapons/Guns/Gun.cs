using UnityEngine;

public abstract class Gun : Weapon
{
    [field: Header("Gun")]
    [field: SerializeField, Min(0.01f)]
    public float Recoil { get; private set; }
    [field: SerializeField, Min(1)]
    public int MaxReserveAmmo { get; private set; }
    [field: SerializeField, Min(1)]
    public int MagazineSize { get; private set; }
    [field: SerializeField, Min(1)]
    public int StartingMagazines { get; private set; } = 3;

    public int MagazineAmmo { get; protected set; }
    public int ReserveAmmo { get; protected set; }

    public bool NeedsReloading => MagazineAmmo == 0;
    public bool OutOfAmmo => MagazineAmmo + ReserveAmmo == 0;
    public bool MagazineFull => MagazineAmmo == MagazineSize;

    protected virtual void Start()
    {
        MagazineAmmo = MagazineSize;
        ReserveAmmo = (StartingMagazines - 1) * MagazineSize;
    }

    public void PickupMagazine() => ReserveAmmo = Mathf.Min(ReserveAmmo + MagazineSize, MaxReserveAmmo);

    public void Reload() => MagazineAmmo += Mathf.Min(MagazineSize - MagazineAmmo, ReserveAmmo);
}
