using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string introSceneName = "IntroScene";
    
    public void StartGame()
    {
        // Use transition manager if available for cinematic fade
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadSceneWithFade(introSceneName);
        }
        else
        {
            SceneManager.LoadScene(introSceneName);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
