using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private int _playerLobbySceneIndex = 1;

    public void GoToLobby()
    {
        SceneManager.LoadScene(_playerLobbySceneIndex);
    }

    public void OnExit()
    {
        Application.Quit();
    }
}
