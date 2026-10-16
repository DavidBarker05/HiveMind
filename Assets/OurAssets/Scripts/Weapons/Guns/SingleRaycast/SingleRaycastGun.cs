using System.Collections;
using UnityEngine;
using Util.UnityUtils;

public class SingleRaycastGun : Gun
{
    [SerializeField, Min(0.01f)]
    float m_AttackRange;

    bool Shoot(Transform origin, out RaycastHit hitInfo) => Physics.Raycast(
        origin: origin.position,
        direction: origin.forward,
        maxDistance: m_AttackRange,
        hitInfo: out hitInfo,
        layerMask: HittableLayers,
        queryTriggerInteraction: QueryTriggerInteraction.Collide
    );

    public override bool Attack(Transform origin)
    {
        if (!m_bCanAttack || NeedsReloading) return false;
        if (!m_bCanAttack) return false;
        m_bCanAttack = false;
        --MagazineAmmo;
        if (Shoot(origin, out RaycastHit hitInfo))
        {
            HealthComponent healthComponent = hitInfo.GetComponent<HealthComponent>();
            if (healthComponent) healthComponent.TakeDamage(Damage);
        }
        StartCoroutine(AttackCooldown());
        return true;
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(TimeBetweenAttacks);
        m_bCanAttack = true;
    }
}
