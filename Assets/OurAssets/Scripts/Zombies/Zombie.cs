using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CapsuleCollider), typeof(NavMeshAgent))]
public class Zombie : MonoBehaviour
{
    [SerializeField, Min(0.01f)]
    float m_TargetPositionUpdateFrequency = 5f;

    FirstPersonPlayerCharacter m_Player;

    NavMeshAgent m_Agent;

    void OnEnable() => GetComponent<CapsuleCollider>().isTrigger = true;

    void Awake() => m_Agent = GetComponent<NavMeshAgent>();

    void Start()
    {
        m_Player = FindAnyObjectByType<FirstPersonPlayerCharacter>();
        StartCoroutine(TargetUpdate());
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // TODO: Damage Player
        }
    }

    IEnumerator TargetUpdate()
    {
        while (m_Player)
        {
            m_Agent.SetDestination(m_Player.transform.position);
            yield return new WaitForSeconds(1f / m_TargetPositionUpdateFrequency);
        }
    }
}
