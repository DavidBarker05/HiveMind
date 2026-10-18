using UnityEngine;

public class GrenadeLauncher : Gun
{
    [Header("Grenade Launcher")]
    [SerializeField]
    Transform m_Barrel;
    [SerializeField]
    LayerMask m_DamageableLayers;
    [SerializeField]
    Grenade m_GrenadePrefab;
    [SerializeField, Min(1f)]
    float m_GrenadeLaunchForce = 30f;

    public override bool Attack(Transform origin)
    {
        if (!CanAttack || NeedsReloading) return false;
        m_AttackCooldownRemaining = TimeBetweenAttacks;
        --MagazineAmmo;
        Grenade grenade = Instantiate(m_GrenadePrefab, m_Barrel.position, m_Barrel.rotation);
        grenade.Init(Damage, m_DamageableLayers, HittableLayers);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.AddForce(m_Barrel.forward * m_GrenadeLaunchForce, ForceMode.VelocityChange);
        return true;
    }

    public override void UpdateWeapon(float deltaTime)
    {
        if (m_AttackCooldownRemaining > 0f) m_AttackCooldownRemaining -= deltaTime;
    }
}
