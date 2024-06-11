using UnityEngine;
using Unity.Netcode;

public class NetworkButtons : MonoBehaviour
{
    void OnGUI()
    {
        float buttonWidth = 200f;  // Ancho del botón
<<<<<<< Updated upstream
        float buttonHeight = 100f;  // Alto del botón
=======
        float buttonHeight = 50f;  // Alto del botón
>>>>>>> Stashed changes
        float buttonSpacing = 20f; // Espacio entre botones

        // Centro de la pantalla
        float centerX = (Screen.width - buttonWidth) / 2;
        float centerY = (Screen.height - (buttonHeight * 3 + buttonSpacing * 2)) / 2;

        // Área para los botones centrados en la pantalla
        GUILayout.BeginArea(new Rect(centerX, centerY, buttonWidth, buttonHeight * 3 + buttonSpacing * 2));

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons(buttonWidth, buttonHeight, buttonSpacing);
        }

        GUILayout.EndArea();

        // Área para los labels en la esquina superior izquierda
        GUILayout.BeginArea(new Rect(10, 10, 300, 100));

        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
        {
            StatusLabels();
        }

        GUILayout.EndArea();
    }

    private void StartButtons(float width, float height, float spacing)
    {
        GUILayout.BeginVertical();

        GUILayoutOption[] buttonOptions = { GUILayout.Width(width), GUILayout.Height(height) };

        if (GUILayout.Button("Host", buttonOptions)) NetworkManager.Singleton.StartHost();
        GUILayout.Space(spacing);
        if (GUILayout.Button("Client", buttonOptions)) NetworkManager.Singleton.StartClient();
        GUILayout.Space(spacing);
        if (GUILayout.Button("Server", buttonOptions)) NetworkManager.Singleton.StartServer();

        GUILayout.EndVertical();
    }

    private void StatusLabels()
    {
        GUILayout.BeginVertical();

        string mode = NetworkManager.Singleton.IsHost ? "Host" : NetworkManager.Singleton.IsClient ? "Client" : "Server";
        GUILayout.Label("Transport: " + NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);

        GUILayout.EndVertical();
    }
}

