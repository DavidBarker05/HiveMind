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

    public int Level { get; private set; } = 1;
    public int CurrentXP { get; private set; }
    public int RequiredXP => XPForLevel(Level);
    public float Progress => (float)CurrentXP / RequiredXP;

    public UnityEvent OnLevelUp;
    public UnityEvent OnXPChanged;

    int XPForLevel(int level) => m_BaseXP + Mathf.RoundToInt(m_GrowthPerLevel * Mathf.Pow(level - 1, m_GrowthExponent));

    public void AddXP(int amount)
    {
        CurrentXP += amount;
        bool bLevelUp = CurrentXP >= RequiredXP;
        while (CurrentXP >= RequiredXP)
        {
            CurrentXP -= RequiredXP;
            ++Level;
        }
        OnXPChanged?.Invoke();
        if (bLevelUp)
        {
            OnLevelUp?.Invoke();
            UpgradeSystem.Instance.LevelUp();
        }
    }
}
