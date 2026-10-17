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

    readonly Dictionary<Type, Gun> m_Guns = new Dictionary<Type, Gun>();

    public Gun CurrentGun { get; private set; }
    public MeleeWeapon CurrentMeleeWeapon { get; private set; }

    public void Init()
    {
        AddGun(m_PistolPrefab);
        Equip<Pistol>();
        // AddMeleeWeapon(m_BatPrefab);
    }

    public void UpdateHandler(float deltaTime)
    {
        foreach (Gun gun in m_Guns.Values) gun.UpdateWeapon(deltaTime);
        if (CurrentMeleeWeapon) CurrentMeleeWeapon.UpdateWeapon(deltaTime);
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
    public bool MeleeAttack(Transform origin) => CurrentMeleeWeapon.Attack(origin);

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
        }
        CurrentGun = gun;
        CurrentGun.gameObject.SetActive(true);
        return true;
    }
}
