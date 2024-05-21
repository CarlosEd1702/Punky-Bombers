using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyToPortals : MonoBehaviour
{

    public GameObject TeleportActived;

    // Start is called before the first frame update
    void Start()
    {
        TeleportActived.SetActive(false);
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            Debug.Log("agarro key");
            
            Destroy(gameObject);
        }
    }
}
