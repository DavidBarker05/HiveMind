using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RestartButton : MonoBehaviour
{
    [SerializeField, Min(0)]
    int m_MainLevelScene = 1;
    [SerializeField]
    LoadingScreen m_LoadingScreen;

    void Awake() => GetComponent<Button>().onClick.AddListener(() =>
    {
        Time.timeScale = 1f;
        UpgradeSystem.Instance.ResetProgress();
        m_LoadingScreen.SceneIndexToLoad = m_MainLevelScene;
        m_LoadingScreen.gameObject.SetActive(true);
    });
}
