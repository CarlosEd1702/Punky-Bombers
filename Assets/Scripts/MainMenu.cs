using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void LoadScene(string NameScene)
    {
        SceneManager.LoadScene(NameScene);
    }

    public void Exit()
    {
#if UNITY_EDITOR
        // Si estás en el editor, esto detendrá la ejecución del juego
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Si estás en la versión compilada, esto cerrará la aplicación
        Application.Quit();
#endif
    }
}