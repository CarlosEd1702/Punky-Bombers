using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class RandomSpawn : NetworkBehaviour
{
    // Start is called before the first frame update
/*    private bool isDestroyed = false;*/
    public GameObject BrickCube;
    public GameObject SpawnerItem;
    public GameObject KeyObjects;
    public GameObject ShieldObjects;
    public GameObject FlameObjects;
    public Transform spawnPosition;

    public void Start()
    {
        BrickCube.SetActive(true);
        SpawnerItem.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Solo el servidor se encarga de inicializar el spawn de objetos
            SpawnItems();
            Debug.Log("Is Server");
        }
        else
        {
            Debug.Log("Is not Server");
        }
    }
    public void SpawnItems()
    {
        int randomIndex = Random.Range(0, 5);
        Vector3 spawnPos = spawnPosition.position;

        switch (randomIndex)
        {
            case 1:
                Instantiate(KeyObjects, spawnPos, Quaternion.identity);
                NetworkObject keyNetwork = KeyObjects.GetComponent<NetworkObject>();
                keyNetwork.Spawn();
                break;

            case 2:
                Instantiate(ShieldObjects, spawnPos, Quaternion.identity);
                NetworkObject shieldNetwork = ShieldObjects.GetComponent<NetworkObject>(); 
                shieldNetwork.Spawn(); 
                break;

            case 3:
                Instantiate(FlameObjects, spawnPos, Quaternion.identity);
                NetworkObject flameNetwork = FlameObjects.GetComponent<NetworkObject>();
                flameNetwork.Spawn();
                break;

            case 4:
                Debug.Log("Empty");
                break;

            case 5:
                Debug.Log("Empty");
                break;
                
            case 6:
                Debug.Log("Empty");
                break;

        }


    }
}
