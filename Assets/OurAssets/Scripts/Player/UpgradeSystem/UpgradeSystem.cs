using UnityEngine;

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
}
