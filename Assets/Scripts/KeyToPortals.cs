using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class KeyToPortals : NetworkBehaviour
{

    public GameObject TeleportActived;

    // Start is called before the first frame update
    void Start()
    {   
        SearchAndDeactivateObjects();

        TeleportActived.SetActive(false);
    }

    private void SearchAndDeactivateObjects()
    {
        TeleportActived = GameObject.FindGameObjectWithTag("TeleportController");
    }
    public void OnTriggerEnter(Collider other)
    {
            Destroy(gameObject);
 
    }
}
