using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    public Clock clock;
    public GameObject brickCubePrefab;
    public GameObject spikesPrefab;
    public List<Transform> emptyBricksObjectList; // Lista de objetos vac�os en la escena
    public float spawnProbability;
    public float spikeSpawnInterval = 1.0f; // Intervalo de tiempo entre la aparici�n de spikes

/*    private int currentSpikeIndex = 0; // �ndice del spike actual en la lista de objetos vac�os
    private bool spawningEnabled = false;*/

    void Start()
    {
        // Encuentra el objeto que tiene el script Clock adjunto
        clock = FindFirstObjectByType<Clock>();

        // Recorre la lista de objetos vac�os
        foreach (Transform emptyObjectTransform in emptyBricksObjectList)
        {
            // Genera un n�mero aleatorio entre 0 y 1
            float randomValue = Random.value;

            // Compara el n�mero aleatorio con la probabilidad de instanciar el prefab
            if (randomValue <= spawnProbability)
            {
                // Instancia el prefab en la posici�n del objeto vac�o actual
                Instantiate(brickCubePrefab, emptyObjectTransform.position, emptyObjectTransform.rotation);
            }
        }
    }
}
