using UnityEngine;
[
    RequireComponent(
        typeof(CapsuleCollider),
        typeof(Rigidbody),
        typeof(AnimatedMesh)
    ),
    RequireComponent(
        typeof(HealthComponent)
    )
]
public class Zombie : MonoBehaviour
{
    [SerializeField, Min(0.01f)]
    float m_Speed = 5f;

    FirstPersonPlayerCharacter m_Player;

    Rigidbody m_RB;

    void Awake() => m_RB = GetComponent<Rigidbody>();

    public void Init(FirstPersonPlayerCharacter player)
    {
        m_Player = player;
        GetComponent<AnimatedMesh>().Play("run");
        GetComponent<HealthComponent>().OnDeath.AddListener(() => EnemyManager.Instance.ZombieDied(this));
    }

    // Thank you vedinad for telling me to use rigid body
    public void ZombieFixedUpdate()
    {
        Vector3 dir = m_Player.transform.position - transform.position;
        dir.y = 0f;
        Vector3 nextPos = transform.position + dir * (m_Speed * Time.fixedDeltaTime);
        m_RB.MovePosition(nextPos);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // TODO: Damage Player
        }
    }
}
