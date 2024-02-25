using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private int[] _gameScenesIndexes = { 2, 3, 4 };
    [SerializeField] private int _creditsSceneIndex = 1;
    [SerializeField] private int _helpSceneIndex = 5;

    public void GoToGame()
    {
        Destroy(MainMenuMusic.Instance.gameObject);

        int index = Random.Range(0, _gameScenesIndexes.Length);
        SceneManager.LoadScene(_gameScenesIndexes[index]);
    }

    public void GoToCredits()
    {
        SceneManager.LoadScene(_creditsSceneIndex);
    }

    public void GoToHelp()
    {
        SceneManager.LoadScene(_helpSceneIndex);
    }

    public void OnExit()
    {
        Application.Quit();
    }
}
