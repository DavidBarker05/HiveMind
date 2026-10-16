using UnityEngine;

public class WeaponHandlerComponent : MonoBehaviour
{

    public Gun CurrentGun { get; private set; }
    public MeleeWeapon CurrentMeleeWeapon { get; private set; }

    public bool Shoot(Transform origin) => CurrentGun.Attack(origin);
    public bool MeleeAttack(Transform origin) => CurrentMeleeWeapon.Attack(origin);
}
