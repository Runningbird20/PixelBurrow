using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("MainScene"); // Replace with your game scene name
    }

    public void QuitGame()
    {
        Application.Quit(); // Only works in builds, not in the Editor
    }
}   