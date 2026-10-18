using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandlerComponent : MonoBehaviour
{
    [SerializeField]
    Transform m_WeaponSlot;
    [Header("Prefabs")]
    [SerializeField]
    Pistol m_PistolPrefab;
    [SerializeField]
    Rifle m_RiflePrefab;
    [SerializeField]
    GrenadeLauncher m_GrenadeLauncherPrefab;
    [SerializeField]
    Shotgun m_ShotgunPrefab;
    [SerializeField]
    Bat m_BatPrefab;
    [SerializeField]
    Katana m_KatanaPrefab;

    [Header("Crosshairs (HUD objects, hidden until their gun is equipped)")]
    [SerializeField]
    Crosshair m_PistolCrosshair;
    [SerializeField]
    RifleCrosshair m_RifleCrosshair;
    [SerializeField]
    Crosshair m_GrenadeLauncherCrosshair;
    [SerializeField]
    Crosshair m_ShotgunCrosshair;

    readonly Dictionary<Type, Gun> m_Guns = new Dictionary<Type, Gun>();
    readonly Dictionary<Type, Crosshair> m_Crosshairs = new Dictionary<Type, Crosshair>();

    public Gun CurrentGun { get; private set; }
    public MeleeWeapon CurrentMeleeWeapon { get; private set; }

    public void Init()
    {
        m_Crosshairs[typeof(Pistol)] = m_PistolCrosshair;
        m_Crosshairs[typeof(Rifle)] = m_RifleCrosshair;
        m_Crosshairs[typeof(GrenadeLauncher)] = m_GrenadeLauncherCrosshair;
        m_Crosshairs[typeof(Shotgun)] = m_ShotgunCrosshair;
        foreach (Crosshair crosshair in m_Crosshairs.Values)
            if (crosshair) crosshair.SetVisible(false);
        AddGun(m_PistolPrefab);
        Equip<Pistol>();
        AddMeleeWeapon(m_BatPrefab);
        UpgradeSystem.Instance.OnUpgradePurchased += OnUpgradePurchased;
    }

    public void UpdateHandler(float deltaTime)
    {
        foreach (Gun gun in m_Guns.Values) gun.UpdateWeapon(deltaTime);
        if (CurrentMeleeWeapon) CurrentMeleeWeapon.UpdateWeapon(deltaTime);
        if (CurrentGun is Rifle rifle && m_RifleCrosshair) m_RifleCrosshair.SetSpread(rifle.SpreadNormalised);
    }

    void SetCrosshairVisible(Gun gun, bool bVisible)
    {
        if (gun && m_Crosshairs.TryGetValue(gun.GetType(), out Crosshair crosshair) && crosshair)
            crosshair.SetVisible(bVisible);
    }

    void AddGun<T>(T prefab) where T : Gun
    {
        T gun = Instantiate(prefab);
        gun.transform.SetParent(m_WeaponSlot, true);
        gun.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        gun.gameObject.SetActive(false);
        m_Guns.Add(typeof(T), gun);
    }

    void AddMeleeWeapon<T>(T prefab) where T : MeleeWeapon
    {
        if (CurrentMeleeWeapon) Destroy(CurrentMeleeWeapon.gameObject);
        CurrentMeleeWeapon = Instantiate(prefab);
        CurrentMeleeWeapon.transform.SetParent(m_WeaponSlot, true);
        CurrentMeleeWeapon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        CurrentMeleeWeapon.gameObject.SetActive(false);
    }

    public bool Shoot(Transform origin) => CurrentGun.Attack(origin);
    public bool MeleeAttack(Transform origin)
    {
        bool bDidSwing = CurrentMeleeWeapon.Attack(origin);
        if (bDidSwing && CurrentMeleeWeapon.DidHitEnemy) CurrentGun.AddAmmo(CurrentGun.AmmoGainedOnMelee);
        return bDidSwing;
    }

    public void Purchase<T>() where T : Weapon
    {
        Type t = typeof(T);
        if (t == typeof(Pistol) || t == typeof(Bat)) return; // Can't purchase starter items
        if (t == typeof(Rifle)) AddGun(m_RiflePrefab);
        else if (t == typeof(GrenadeLauncher)) AddGun(m_GrenadeLauncherPrefab);
        else if (t == typeof(Shotgun)) AddGun(m_ShotgunPrefab);
        else if (t == typeof(Katana)) AddMeleeWeapon(m_KatanaPrefab);
    }

    public bool Equip<T>() where T : Gun => Equip(typeof(T));

    public bool Equip(Type t)
    {
        if ((CurrentGun && t == CurrentGun.GetType()) || !m_Guns.TryGetValue(t, out Gun gun)) return false;
        if (CurrentGun)
        {
            if (CurrentGun is Rifle rifle) rifle.StopAttacking();
            CurrentGun.gameObject.SetActive(false);
            SetCrosshairVisible(CurrentGun, false);
        }
        CurrentGun = gun;
        CurrentGun.gameObject.SetActive(true);
        SetCrosshairVisible(CurrentGun, true);
        return true;
    }

    public void PickupAmmo<T>() where T : Gun
    {
        Type t = typeof(T);
        if (m_Guns.ContainsKey(t))
            m_Guns[t].PickupMagazine();
    }

    void OnUpgradePurchased(UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case UpgradeType.Rifle:
                Purchase<Rifle>();
                break;
            case UpgradeType.GrenadeLauncher:
                Purchase<GrenadeLauncher>();
                break;
            case UpgradeType.Shotgun:
                Purchase<Shotgun>();
                break;
            case UpgradeType.Katana:
                Purchase<Katana>();
                break;
            default:
                break;
        }
    }
}
