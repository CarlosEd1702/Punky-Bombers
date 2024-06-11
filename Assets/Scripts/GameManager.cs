using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab; // Prefab del jugador

    void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // Instancia jugadores para cada cliente conectado
            NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
            {
                Vector3 spawnPosition = GetSpawnPositionForClient(clientId);
                GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
                playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            };
        }
    }

    private Vector3 GetSpawnPositionForClient(ulong clientId)
    {
        // Lógica para obtener la posición de aparición del jugador
        return Vector3.zero; // Reemplazar con la posición real
    }
}