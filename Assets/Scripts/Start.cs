using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonToScene1 : MonoBehaviour
{
    public void GoToScene1()
    {
        SceneManager.LoadScene("Scene 1");
    }
}
