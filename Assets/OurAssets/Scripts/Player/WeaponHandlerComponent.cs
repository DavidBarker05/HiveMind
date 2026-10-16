using UnityEngine;

public class WeaponHandlerComponent : MonoBehaviour
{
    [SerializeField]
    Transform m_WeaponSlot;
    [Header("Prefabs")]
    [SerializeField]
    Pistol m_PistolPrefab;

    Pistol m_Pistol;

    public void Init()
    {
        m_Pistol = Instantiate(m_PistolPrefab);
        m_Pistol.transform.SetParent(m_WeaponSlot, true);
        m_Pistol.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        CurrentGun = m_Pistol;
    }

    public Gun CurrentGun { get; private set; }
    public MeleeWeapon CurrentMeleeWeapon { get; private set; }

    public bool Shoot(Transform origin) => CurrentGun.Attack(origin);
    public bool MeleeAttack(Transform origin) => CurrentMeleeWeapon.Attack(origin);
}
