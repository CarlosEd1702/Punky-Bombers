using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawn : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] RandObjects;
    public Transform spawnPosition;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            int randomIndex = Random.Range(0, RandObjects.Length);
            Vector3 spawnPos = spawnPosition.position;

            Instantiate(RandObjects[randomIndex], spawnPos, Quaternion.identity);
        }
    }
}
