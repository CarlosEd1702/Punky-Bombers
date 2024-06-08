using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerSpikes : MonoBehaviour
{

    public GameObject spikePrefab;
    public Transform EmptySpikeController;

    public float EpaceBewtenSpikes = 3f;

    public int Rows = 2;
    public int Columns = 2;


    private int CurrentValue = 60;
    private int CurrentRow = 0;
    private int CurrentColumn = 0;

    private void Start()
    {
        GenerateNextSpike();
    }

    void GenerateNextSpike()
    {
        // Calcular la posici�n de generaci�n del bloque
        Vector3 spawnPosition = new Vector3(CurrentColumn * EpaceBewtenSpikes, 5f, CurrentRow * EpaceBewtenSpikes);

        Quaternion rotation = Quaternion.Euler(90,0,0);
        
        // Instanciar el bloque
        GameObject newObj = Instantiate(spikePrefab, spawnPosition, rotation);

        RaycastHit hit;
        if (Physics.Raycast(newObj.transform.position, Vector3.down, out hit))
        {
            if (hit.collider.CompareTag("Breakable")|| hit.collider.CompareTag("Brick")) // Comprueba si el objeto golpeado tiene el tag deseado
            {
                Destroy(hit.collider.gameObject);
            }
        }
        // Establecer el bloque como hijo del controlador de bloques vac�o
        /*        newObj.transform.parent = EmptySpikeController;
        */        // Imprimir mensaje de depuraci�n
        Debug.Log("Objeto instanciado en la posici�n (" + CurrentRow + ", " + CurrentColumn + ") con el valor " + CurrentValue);

        // Incrementar el valor actual
        CurrentValue++;

        // Calcular la siguiente posici�n en la matriz
        CurrentColumn++;
        if (CurrentColumn >= Columns)
        {
            CurrentColumn = 0;
            CurrentRow++;
        }

        // Verificar si se ha generado toda la matriz
        if (CurrentRow < Rows)
        {
            // Si no se ha generado toda la matriz, invocar el m�todo para generar el siguiente bloque despu�s de 1 segundo
            Invoke("GenerateNextSpike", 0.25f);
        }
    }
}
