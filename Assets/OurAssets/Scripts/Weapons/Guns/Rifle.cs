using System.Collections;
using UnityEngine;
using Util.UnityUtils;

public class Rifle : Gun
{
    [Header("Rifle")]
    [SerializeField, Min(0.01f)]
    float m_AttackRange;
    [SerializeField, Min(0.01f)]
    float m_AccuracyCooldownTime = 0.5f;
    [SerializeField, Min(0f)]
    float m_InnacuracyAngle = 1.5f;

    bool m_bAccurate;

    protected override void Start()
    {
        base.Start();
        m_bAccurate = true;
    }

    void Update()
    {
    }

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
        m_bCanAttack = false;
        --MagazineAmmo;
        if (Shoot(origin, out RaycastHit hitInfo))
        {
            HealthComponent healthComponent = hitInfo.GetComponent<HealthComponent>();
            if (healthComponent) healthComponent.TakeDamage(Damage);
        }
        StartCoroutine(AttackCooldown());
        m_bAccurate = false;
        return true;
    }

    public void StopAttacking()
    {
        m_bAccurate = true;
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(TimeBetweenAttacks);
        m_bCanAttack = true;
    }
}
