using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LoadMenuButton : MonoBehaviour
{
    [SerializeField]
    int m_MainMenuSceneIndex = 0;
    [SerializeField]
    LoadingScreen m_LoadingScreen;

    void Awake() => GetComponent<Button>().onClick.AddListener(() =>
    {
        Time.timeScale = 1f;
        UpgradeSystem.Instance.ResetProgress();
        m_LoadingScreen.SceneIndexToLoad = m_MainMenuSceneIndex;
        m_LoadingScreen.gameObject.SetActive(true);
    });
}
