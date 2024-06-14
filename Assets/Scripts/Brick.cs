using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Brick : NetworkBehaviour
{
    [SerializeField] private GameObject[] itemPrefabs; // Array de prefabs de los ítems que pueden spawnar
    [SerializeField] private float spawnProbability = 0.25f; // Probabilidad de spawnar un ítem (25% en este caso)

    public override void OnDestroy()
    {
        base.OnDestroy(); // Llamar al método OnDestroy de la clase base para asegurarse de que se ejecute la lógica predeterminada
        
        if (IsServer && NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening) // Asegurarse de que solo el servidor maneje el spawn de ítems y que el NetworkManager esté escuchando
        {
            TrySpawnItem();
        }
        else
        {
            Debug.LogError("NetworkManager is not listening or this is not the server, unable to spawn items.");
        }
    }

    private void TrySpawnItem()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            float randomValue = Random.value;
            if (randomValue <= spawnProbability)
            {
                int randomIndex = Random.Range(0, itemPrefabs.Length);
                GameObject itemPrefab = itemPrefabs[randomIndex];
                Vector3 spawnPosition = transform.position; // Usar la posición del Brick como posición de spawn
                Quaternion spawnRotation = Quaternion.identity;

                // Instanciar el ítem y hacer que el objeto sea de la red
                GameObject itemInstance = Instantiate(itemPrefab, spawnPosition, spawnRotation);
                NetworkObject networkObject = itemInstance.GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    networkObject.Spawn();
                }
                else
                {
                    Debug.LogError("Spawned item does not have a NetworkObject component.");
                }
            }
        }
        else
        {
            Debug.LogError("NetworkManager is not listening, unable to spawn items.");
        }
    }
}
