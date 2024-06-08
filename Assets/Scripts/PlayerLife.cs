using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerLife : NetworkBehaviour
{
    [SerializeField] private GameObject Shield;
    [SerializeField] private CollectItems CollectItems;

    private void Start()
    {
        if(CollectItems.shieldIsActive == true)
        {
            ShieldInstance();
        }
    }

   //[ServerRpc]

    public void ShieldInstance()
    {
        if(IsOwner)
        {
            GameObject shieldInstance = Instantiate(Shield, transform.position, transform.rotation);
            NetworkObject shieldNetworkObject = shieldInstance.GetComponent<NetworkObject>();
            shieldNetworkObject.Spawn();
        }
    }
}
