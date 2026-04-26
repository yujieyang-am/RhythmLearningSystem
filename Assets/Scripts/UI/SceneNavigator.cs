using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    public void GoToChallenge()
    {
        SceneManager.LoadScene("SC_Challenge");
    }

    public void GoToLearn()
    {
        SceneManager.LoadScene("SC_Learn");
    }

    public void GoToProgress()
    {
        SceneManager.LoadScene("SC_Progress");
    }

    public void GoToSetting()
    {
        SceneManager.LoadScene("SC_Setting");
    }

    public void GoToGame()
    {
        SceneManager.LoadScene("SC_Game");
    }

    public void GoToResult()
    {
        SceneManager.LoadScene("SC_Result");
    }
}