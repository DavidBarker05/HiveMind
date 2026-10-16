using UnityEngine;

public class WeaponHandlerComponent : MonoBehaviour
{

    public Weapon CurrentWeapon { get; private set; }

    public bool Attack(Transform origin) => CurrentWeapon.Attack(origin);
}
