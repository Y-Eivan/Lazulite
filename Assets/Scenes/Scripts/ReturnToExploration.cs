using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToExploration : MonoBehaviour
{
    public void GoBack()
    {
        SceneManager.LoadScene("ExplorationScene");
    }
}