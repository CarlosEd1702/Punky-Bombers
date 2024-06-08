using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Teleport : NetworkBehaviour
{
    public Transform Destination;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
        {
            // Solo el servidor debe mover a los jugadores
            if (IsServer)
            {
                TeleportPlayerServerRpc(networkObject.NetworkObjectId, Destination.position);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TeleportPlayerServerRpc(ulong playerId, Vector3 destination)
    {
        NetworkObject playerNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerId];

        // Mover al jugador en el servidor
        playerNetworkObject.transform.position = destination;

        // Notificar a todos los clientes para que actualicen la posición del jugador
        TeleportPlayerClientRpc(playerId, destination);
    }

    [ClientRpc]
    private void TeleportPlayerClientRpc(ulong playerId, Vector3 destination)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerId, out NetworkObject playerNetworkObject))
        {
            playerNetworkObject.transform.position = destination;
        }
    }
}
