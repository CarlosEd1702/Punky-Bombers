using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Teleport : NetworkBehaviour
{  
    public Transform Destination;
    public GameObject Player;

    private void Update()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
    }
    public void OnTriggerEnter(Collider other)
    {
       


        if (other.gameObject.tag == "Player")
        {
          Player.transform.position = Destination.transform.position;

          Debug.Log("Player on Tp!");
    
        }
    }  
}
