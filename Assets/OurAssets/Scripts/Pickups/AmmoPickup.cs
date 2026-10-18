using UnityEngine;

[System.Serializable]
public enum AmmoPickupGun
{
    Pistol,
    Rifle,
    GrenadeLauncher,
    Shotgun
}

public class AmmoPickup : Pickup
{
    [Header("Ammo Pickup")]
    [SerializeField]
    AmmoPickupGun m_GunToGiveAmmo;

    protected override void OnPickupCollected(Collider other)
    {
        WeaponHandlerComponent weaponHandler = other.gameObject.GetComponent<WeaponHandlerComponent>();
        if (!weaponHandler) return;
        switch (m_GunToGiveAmmo)
        {
            case AmmoPickupGun.Pistol:
                weaponHandler.PickupAmmo<Pistol>();
                break;
            case AmmoPickupGun.Rifle:
                weaponHandler.PickupAmmo<Rifle>();
                break;
            case AmmoPickupGun.GrenadeLauncher:
                weaponHandler.PickupAmmo<GrenadeLauncher>();
                break;
            case AmmoPickupGun.Shotgun:
                weaponHandler.PickupAmmo<Shotgun>();
                break;
            default:
                break;
        }
        OnCollectionCompleted?.Invoke();
    }
}
