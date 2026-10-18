using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelDisplay : MonoBehaviour
{
    [SerializeField]
    Slider m_ExperienceBar;
    [SerializeField]
    TMP_Text m_LevelText;
    [SerializeField]
    PlayerLevelComponent m_PlayerLevelComponent;

    void Awake()
    {
        m_PlayerLevelComponent.OnXPChanged.AddListener(UpdateLevelDisplay);
        if (m_ExperienceBar)
        {
            m_ExperienceBar.wholeNumbers = false;
            m_ExperienceBar.minValue = 0f;
            m_ExperienceBar.maxValue = 1f;
        }
    }

    void OnEnable() => UpdateLevelDisplay();

    void UpdateLevelDisplay()
    {
        if (m_ExperienceBar) m_ExperienceBar.value = m_PlayerLevelComponent.Progress;
        if (m_LevelText) m_LevelText.text = $"{m_PlayerLevelComponent.Level}";
    }
}
