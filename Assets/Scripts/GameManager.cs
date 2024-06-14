using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab; // Prefab del jugador
    [SerializeField] private List<Transform> spawnTransforms; // Lista de posiciones de aparición

    private int nextSpawnIndex = 0; // Índice para la siguiente posición de aparición

    void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Transform spawnTransform = GetSpawnTransformForClient();
        GameObject playerInstance = Instantiate(playerPrefab, spawnTransform.position, spawnTransform.rotation);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    private Transform GetSpawnTransformForClient()
    {
        if (spawnTransforms.Count == 0)
        {
            Debug.LogError("No spawn positions defined!");
            return null; // Return null if there are no spawn positions defined
        }

        Transform spawnTransform = spawnTransforms[nextSpawnIndex];

        nextSpawnIndex = (nextSpawnIndex + 1) % spawnTransforms.Count;

        return spawnTransform;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}