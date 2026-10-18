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
                DontDestroyOnLoad(go);
                s_Instance = go.AddComponent<UpgradeSystem>();
            }
            return s_Instance;
        }
    }

    public static UpgradeSystem InstanceOrNull => s_Instance ? s_Instance : null;

    public UnityEvent OnLevelPointsChanged = new UnityEvent();
    public System.Action<UpgradeType> OnUpgradePurchased;

    public int LevelPoints { get; private set; } = 0;
    readonly Dictionary<UpgradeType, int> m_UpgradesPurchased = new Dictionary<UpgradeType, int>();

    public void ResetProgress()
    {
        LevelPoints = 0;
        m_UpgradesPurchased.Clear();
        OnLevelPointsChanged?.Invoke();
    }

    public void LevelUp()
    {
        ++LevelPoints;
        OnLevelPointsChanged?.Invoke();
    }

    public bool CanPurchaseUpgrade(Upgrade upgrade) =>
        upgrade.UpgradeType != UpgradeType.None
        && LevelPoints >= upgrade.PointsCost
        && (upgrade.RequiredPreviousUpgrade == UpgradeType.None || m_UpgradesPurchased.ContainsKey(upgrade.RequiredPreviousUpgrade))
        && (!UpgradeHelper.IsUpgradeOneTimePurchase(upgrade) || !m_UpgradesPurchased.ContainsKey(upgrade.UpgradeType));

    public void PurchaseUpgrade(Upgrade upgrade)
    {
        if (!CanPurchaseUpgrade(upgrade)) return;
        LevelPoints -= upgrade.PointsCost;
        if (!m_UpgradesPurchased.ContainsKey(upgrade.UpgradeType)) m_UpgradesPurchased.Add(upgrade.UpgradeType, 1);
        else ++m_UpgradesPurchased[upgrade.UpgradeType];
        OnLevelPointsChanged?.Invoke();
        OnUpgradePurchased?.Invoke(upgrade.UpgradeType);
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
