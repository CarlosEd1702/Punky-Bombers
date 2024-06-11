using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerLife : NetworkBehaviour
{
    [SerializeField] private GameObject ShieldPrefab; // Prefab del escudo
    private GameObject shieldInstance; // Instancia del escudo
    [SerializeField] private CollectItems collectItems;

    private void Start()
    {
        // Asegúrate de que el collectItems esté correctamente asignado
        if (collectItems == null)
        {
            collectItems = GetComponent<CollectItems>();
            if (collectItems == null)
            {
                Debug.LogError("CollectItems script not found on the player object.");
            }
        }

        // Probar activación de escudo al inicio (puedes remover esto después de verificar)
        ActivateShield();
    }

    // Método público para instanciar el escudo
    public void ActivateShield()
    {
        Debug.Log("ActivateShield called");

        if (IsOwner && collectItems.shieldIsActive)
        {   
            Debug.Log("Activate Shield - IsOwner and shieldIsActive");
            ShieldInstanceServerRpc();
        }
    }

    // Método público para destruir el escudo
    public void DeactivateShield()
    {
        Debug.Log("DeactivateShield called");

        if (IsOwner && shieldInstance != null)
        {
            DestroyShieldServerRpc();
        }
    }

    [ServerRpc]
    private void ShieldInstanceServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("ShieldInstanceServerRpc called");

        if (shieldInstance == null)
        {
            // Instanciar el escudo en el servidor
            shieldInstance = Instantiate(ShieldPrefab, transform.position, transform.rotation);
            NetworkObject shieldNetworkObject = shieldInstance.GetComponent<NetworkObject>();
            shieldNetworkObject.SpawnWithOwnership(OwnerClientId);

            // Hacer que el escudo siga al jugador
            ShieldFollowClientRpc(shieldNetworkObject.NetworkObjectId);
        }
    }

    [ServerRpc]
    private void DestroyShieldServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("DestroyShieldServerRpc called");

        if (shieldInstance != null)
        {
            NetworkObject shieldNetworkObject = shieldInstance.GetComponent<NetworkObject>();
            shieldNetworkObject.Despawn();
            Destroy(shieldInstance);
            shieldInstance = null;
        }
    }

    [ClientRpc]
    private void ShieldFollowClientRpc(ulong shieldNetworkObjectId)
    {
        Debug.Log("ShieldFollowClientRpc called");

        // Encontrar el objeto escudo en los clientes
        NetworkObject shieldNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[shieldNetworkObjectId];
        if (shieldNetworkObject != null)
        {
            shieldInstance = shieldNetworkObject.gameObject;
            // Hacer que el escudo siga al jugador
            shieldInstance.transform.SetParent(transform);
        }
    }
}
