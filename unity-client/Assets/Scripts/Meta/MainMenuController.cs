using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private int _gameSceneIndex = 1;

    public void GoToGame()
    {
        SceneManager.LoadScene(_gameSceneIndex);
    }

    public void OnExit()
    {
        Application.Quit();
    }
}
