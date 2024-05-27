using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Teleport : NetworkBehaviour
{
    
    public Transform Destination;
    public GameObject Player;
    public void OnTriggerEnter(Collider other)
    {
       


        if (other.gameObject.tag == "Player")
        {
          Player.transform.position = Destination.transform.position;

          Debug.Log("Player on Tp!");
    
        }
    }  
}
