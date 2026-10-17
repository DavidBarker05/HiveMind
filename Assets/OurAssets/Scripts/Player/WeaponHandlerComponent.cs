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

    readonly List<Gun> m_Guns = new List<Gun>();

    public Gun CurrentGun { get; private set; }
    public MeleeWeapon CurrentMeleeWeapon { get; private set; }

    public void Init()
    {
        AddGun(m_RiflePrefab);
        Equip(m_Guns[0]);
    }

    public void UpdateHandler(float deltaTime)
    {
        foreach (Gun gun in m_Guns) gun.UpdateWeapon(deltaTime);
        if (CurrentMeleeWeapon) CurrentMeleeWeapon.UpdateWeapon(deltaTime);
    }

    T AddGun<T>(T prefab) where T : Gun
    {
        T gun = Instantiate(prefab);
        gun.transform.SetParent(m_WeaponSlot, true);
        gun.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        gun.gameObject.SetActive(false);
        m_Guns.Add(gun);
        return gun;
    }

    public bool Shoot(Transform origin) => CurrentGun.Attack(origin);
    public bool MeleeAttack(Transform origin) => CurrentMeleeWeapon.Attack(origin);

    public void Reload() => CurrentGun.Reload();

    public void Equip(Gun gun)
    {
        if (CurrentGun == gun) return;
        if (CurrentGun)
        {
            if (CurrentGun is Rifle rifle) rifle.StopAttacking();
            CurrentGun.gameObject.SetActive(false);
        }
        CurrentGun = gun;
        CurrentGun.gameObject.SetActive(true);
    }
}
