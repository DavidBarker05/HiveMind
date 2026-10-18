using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    [SerializeField]
    HealthComponent m_HealthComponent;
    [SerializeField]
    Slider m_Slider;
    [SerializeField]
    TMP_Text m_Text;
    [SerializeField, Tooltip("{0} is the current health, {1} is the max health.")]
    string m_Format = "{0} / {1}";

    int m_LastCurrent = -1;
    int m_LastMax = -1;

    void OnEnable() => Refresh(true);

    void Update() => Refresh(false);

    void Refresh(bool bForce)
    {
        if (!m_HealthComponent) return;
        int current = m_HealthComponent.CurrentHealth;
        int max = m_HealthComponent.MaxHealth;
        if (!bForce && current == m_LastCurrent && max == m_LastMax) return;
        m_LastCurrent = current;
        m_LastMax = max;
        if (m_Slider)
        {
            m_Slider.minValue = 0f;
            m_Slider.maxValue = Mathf.Max(max, 1f);
            m_Slider.value = current;
        }
        if (m_Text) m_Text.text = string.Format(m_Format, current, max);
    }
}
