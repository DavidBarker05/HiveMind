using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UpgradeSystem : MonoBehaviour
{
    static UpgradeSystem s_Instance;
    public static UpgradeSystem Instance
    {
        get
        {
            if (!s_Instance)
            {
                GameObject go = new GameObject(nameof(UpgradeSystem));
                s_Instance = go.AddComponent<UpgradeSystem>();
            }
            return s_Instance;
        }
    }

    public UnityEvent OnLevelPointsChanged;

    int m_LevelPoints = 0;
    readonly Dictionary<UpgradeType, int> m_UpgradesPurchased = new Dictionary<UpgradeType, int>();

    public void LevelUp()
    {
        ++m_LevelPoints;
        OnLevelPointsChanged?.Invoke();
    }

    public bool CanPurchaseUpgrade(Upgrade upgrade) =>
        upgrade.UpgradeType != UpgradeType.None
        && m_LevelPoints >= upgrade.PointsCost
        && (upgrade.RequiredPreviousUpgrade == UpgradeType.None || m_UpgradesPurchased.ContainsKey(upgrade.RequiredPreviousUpgrade))
        && (!UpgradeHelper.IsUpgradeOneTimePurchase(upgrade) || !m_UpgradesPurchased.ContainsKey(upgrade.UpgradeType));

    public void PurchaseUpgrade(Upgrade upgrade)
    {
        if (!CanPurchaseUpgrade(upgrade)) return;
        m_LevelPoints -= upgrade.PointsCost;
        if (!m_UpgradesPurchased.ContainsKey(upgrade.UpgradeType)) m_UpgradesPurchased.Add(upgrade.UpgradeType, 1);
        else ++m_UpgradesPurchased[upgrade.UpgradeType];
        OnLevelPointsChanged?.Invoke();
    }

    public bool HasBeenPurchased(Upgrade upgrade) => m_UpgradesPurchased.ContainsKey(upgrade.UpgradeType);

    public int GetUpgradeLevel(Upgrade upgrade)
    {
        if (upgrade.UpgradeType == UpgradeType.None) throw new System.ArgumentException("Upgrade type cannot be none");
        else if (!m_UpgradesPurchased.ContainsKey(upgrade.UpgradeType)) return 0;
        else return m_UpgradesPurchased[upgrade.UpgradeType];
    }

    public int GetUpgradeLevel(UpgradeType upgrade)
    {
        if (upgrade == UpgradeType.None) throw new System.ArgumentException("Upgrade type cannot be none");
        else if (!m_UpgradesPurchased.ContainsKey(upgrade)) return 0;
        else return m_UpgradesPurchased[upgrade];
    }
}
