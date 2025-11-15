using UnityEngine;

public class QuitApp : MonoBehaviour
{
    public void QuitGame()
    {
        #if UNITY_EDITOR

                    UnityEditor.EditorApplication.isPlaying = false;
        #else
       
                Application.Quit();
        #endif
    }
}
