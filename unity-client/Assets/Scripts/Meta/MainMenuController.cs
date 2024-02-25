using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private int[] _gameScenesIndexes = { 2, 3, 4 };
    [SerializeField] private int _creditsSceneIndex = 1;

    public void GoToGame()
    {
        int index = Random.Range(0, _gameScenesIndexes.Length);
        SceneManager.LoadScene(_gameScenesIndexes[index]);
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
