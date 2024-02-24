using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsController : MonoBehaviour
{
    [SerializeField] private int _mainMenuSceneIndex = 0;

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(_mainMenuSceneIndex);
    }
}
