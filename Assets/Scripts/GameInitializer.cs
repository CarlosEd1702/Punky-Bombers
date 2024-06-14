using Unity.Netcode;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    void Start()
    {
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.StartHost(); // o NetworkManager.Singleton.StartServer();
        }
    }
}