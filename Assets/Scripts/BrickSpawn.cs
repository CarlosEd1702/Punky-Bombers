using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CubeSpawner : NetworkBehaviour
{
    public Clock clock;
  
    public GameObject brickCubePrefab;
    public GameObject spikesPrefab;
    public List<Transform> emptyBricksObjectList; // Lista de objetos vacíos en la escena
    public float spawnProbability;
    public float spikeSpawnInterval = 1.0f; // Intervalo de tiempo entre la aparición de spikes


    public override void OnNetworkSpawn()
    {
        // Encuentra el objeto que tiene el script Clock adjunto
        clock = FindFirstObjectByType<Clock>();

        if (IsServer)
        {
            // Solo el servidor se encarga de inicializar el spawn de objetos
            InitializeSpawns();
            Debug.Log("Is Server");
        }
        else
        {
            Debug.Log("Is not Server");
        }
    }

    private void InitializeSpawns()
    {
        foreach (Transform emptyObjectTransform in emptyBricksObjectList)
        {
            // Genera un número aleatorio entre 0 y 1
            float randomValue = Random.value;

            // Compara el número aleatorio con la probabilidad de instanciar el prefab
            if (randomValue <= spawnProbability)
            {
                Vector3 position = emptyObjectTransform.position;
                Quaternion rotation = emptyObjectTransform.rotation;

                // Instancia el prefab en el servidor
                GameObject brickCubeInstance = Instantiate(brickCubePrefab, position, rotation);
                NetworkObject networkObject = brickCubeInstance.GetComponent<NetworkObject>();
                networkObject.Spawn();

                // Notifica a los clientes sobre la instancia
                SpawnBrickCubeClientRpc(position, rotation);
            }
        }
    }

    [ClientRpc]
    private void SpawnBrickCubeClientRpc(Vector3 position, Quaternion rotation)
    {
        // Instancia el prefab en el cliente
        if (!IsServer)
        {
            Instantiate(brickCubePrefab, position, rotation);
        }
    }
}
