using UnityEngine;
using UnityEngine.Events;

public class PlayerLevelComponent : MonoBehaviour
{
    [SerializeField, Min(1)]
    int m_BaseXP = 100;
    [SerializeField, Min(1f)]
    float m_GrowthPerLevel = 8f;
    [SerializeField, Min(1f)]
    float m_GrowthExponent = 1.15f;
    [SerializeField]
    GameObject m_HUD;
    [SerializeField]
    GameObject m_LevelUpScreen;
    [SerializeField]
    FirstPersonPlayerCharacter m_FirstPersonCharacter;
    [SerializeField]
    MenuPlayerCharacter m_MenuCharacter;

    public int Level { get; private set; } = 1;
    public int CurrentXP { get; private set; }
    public int RequiredXP => XPForLevel(Level);
    public float Progress => (float)CurrentXP / RequiredXP;

    public UnityEvent OnLevelUp;
    public UnityEvent OnXPChanged;

    void Awake() => UpgradeSystem.Instance.ResetProgress();

    int XPForLevel(int level) => m_BaseXP + Mathf.RoundToInt(m_GrowthPerLevel * Mathf.Pow(level - 1, m_GrowthExponent));

    public void AddXP(int amount)
    {
        CurrentXP += amount;
        int levelsGained = 0;
        while (CurrentXP >= RequiredXP)
        {
            CurrentXP -= RequiredXP;
            ++Level;
            ++levelsGained;
        }
        OnXPChanged?.Invoke();
        if (levelsGained > 0)
        {
            OnLevelUp?.Invoke();
            for (int i = 0; i < levelsGained; ++i) UpgradeSystem.Instance.LevelUp();
            if (m_MenuCharacter && m_FirstPersonCharacter)
                m_MenuCharacter.OnMenuOpen(m_FirstPersonCharacter, m_HUD, m_LevelUpScreen);
        }
    }
}
