using System.Collections;
using UnityEngine;

public class HitMarker : MonoBehaviour
{
    [SerializeField]
    GameObject m_Panel;
    [SerializeField, Min(0.01f)]
    float m_ShowDuration = 0.15f;

    Coroutine m_Routine;

    void Awake()
    {
        if (m_Panel) m_Panel.SetActive(false);
    }

    void OnEnable() => HealthComponent.OnAnyDamageTaken += OnDamageTaken;

    void OnDisable()
    {
        HealthComponent.OnAnyDamageTaken -= OnDamageTaken;
        if (m_Routine != null) StopCoroutine(m_Routine);
        m_Routine = null;
        if (m_Panel) m_Panel.SetActive(false);
    }

    void OnDamageTaken(HealthComponent target)
    {
        if (!m_Panel || !target || !target.TryGetComponent(out Zombie _)) return;
        m_Routine = StartCoroutine(Show());
    }

    IEnumerator Show()
    {
        m_Panel.SetActive(true);
        yield return new WaitForSecondsRealtime(m_ShowDuration);
        m_Panel.SetActive(false);
        m_Routine = null;
    }
}
