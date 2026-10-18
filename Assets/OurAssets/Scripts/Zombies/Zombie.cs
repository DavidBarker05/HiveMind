using UnityEngine;
[
    RequireComponent(
        typeof(CapsuleCollider),
        typeof(Rigidbody),
        typeof(AnimatedMesh)
    ),
    RequireComponent(
        typeof(HealthComponent),
        typeof(HitFlashComponent)
    )
]
public class Zombie : MonoBehaviour
{
    [SerializeField, Min(0.01f)]
    float m_Speed = 5f;
    [SerializeField]
    float m_BaseDamage = 10f;

    float m_Damage;

    [Header("Climbing")]
    [SerializeField] LayerMask m_ClimbableMask;
    [SerializeField, Min(0f)] float m_WallCheckDistance = 0.3f;
    [SerializeField, Min(0.01f)] float m_ClimbSpeed = 3f;

    FirstPersonPlayerCharacter m_Player;

    Rigidbody m_RB;
    CapsuleCollider m_Capsule;
    HitFlashComponent m_HitFlash;

    void Awake()
    {
        m_RB = GetComponent<Rigidbody>();
        m_Capsule = GetComponent<CapsuleCollider>();
        m_HitFlash = GetComponent<HitFlashComponent>();
    }

    public void Init(FirstPersonPlayerCharacter player, float healthMult, float damageMult)
    {
        m_Player = player;
        m_Damage = m_BaseDamage * damageMult;
        GetComponent<HealthComponent>().InitHealth(healthMult);
        GetComponent<AnimatedMesh>().Play("run");
        HealthComponent healthComponent = GetComponent<HealthComponent>();
        healthComponent.OnDeath.AddListener(() =>
        {
            EnemyManager.Instance.ZombieDied(this);
            Destroy(gameObject);
        });
        healthComponent.OnDamageTaken.AddListener(() => m_HitFlash.Flash());
    }

    // Thank you vedinad for telling me to use rigid body
    public void ZombieFixedUpdate()
    {
        Vector3 toPlayer = m_Player.transform.position - m_RB.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.0001f) return;
        Vector3 dir = toPlayer.normalized;
        m_RB.MoveRotation(Quaternion.LookRotation(dir));
        Vector3 origin = m_RB.position + Vector3.up * 0.1f;
        bool wallAhead = Physics.Raycast(origin, dir, m_Capsule.radius + m_WallCheckDistance, m_ClimbableMask);
        m_RB.useGravity = !wallAhead;
        if (wallAhead)
        {
            m_RB.linearVelocity = Vector3.zero;
            m_RB.MovePosition(m_RB.position + Vector3.up * (m_ClimbSpeed * Time.fixedDeltaTime));
        }
        else m_RB.MovePosition(m_RB.position + dir * (m_Speed * Time.fixedDeltaTime));
    }

    // Has to be OnTriggerEnter for CharacterController so add an additional CapsuleCollider that is a trigger
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // TODO: Damage Player
        }
    }
}
