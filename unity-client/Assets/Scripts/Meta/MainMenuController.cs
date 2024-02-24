using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private int _gameSceneIndex = 1;
    [SerializeField] private int _creditsSceneIndex = 2;

    public void GoToGame()
    {
        SceneManager.LoadScene(_gameSceneIndex);
    }

    public void GoToCredits()
    {
        SceneManager.LoadScene(_creditsSceneIndex);
    }

    public void OnExit()
    {
        Application.Quit();
    }
}
