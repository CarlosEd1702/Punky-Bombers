using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CollectItems : NetworkBehaviour
{
    public GameObject TeleportController;
    public GameObject KeyImage;
    public GameObject ShieldImage;

    public bool shieldIsActive = false;

    public int Boomb;
    public int Flame;

    // Start is called before the first frame update
    public void Start()
    {
        if (IsServer)
        {
            // Buscar y desactivar los objetos TeleportController y KeyImage
            SearchAndDeactivateObjects();
        }
    }

    private void SearchAndDeactivateObjects()
    {
        TeleportController = GameObject.FindGameObjectWithTag("TeleportController");
        KeyImage = GameObject.FindGameObjectWithTag("Key Image");
        ShieldImage = GameObject.FindGameObjectWithTag("Shield Image");

        if (TeleportController != null)
        {
            TeleportController.SetActive(false);
        }
        else
        {
            Debug.LogError("TeleportController not found in the scene!");
        }

        if (KeyImage != null)
        {
            KeyImage.SetActive(false);
        }
        else
        {
            Debug.LogError("KeyImage not found in the scene!");
        }

        if (ShieldImage != null)
        {
            ShieldImage.SetActive(false);
        }
        else
        {
            Debug.LogError("ShieldImage not found in the scene!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner)
            return;

        if (other.gameObject.CompareTag("Boomb"))
        {
            Boomb++;
            var networkObject = other.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                CollectItemServerRpc(networkObject.NetworkObjectId, "Boomb");
            }
        }
        else if (other.gameObject.CompareTag("Flame"))
        {
            Flame++;
            var networkObject = other.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                CollectItemServerRpc(networkObject.NetworkObjectId, "Flame");
            }
        }
        else if (other.gameObject.CompareTag("Key"))
        {
            var networkObject = other.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                CollectItemServerRpc(networkObject.NetworkObjectId, "Key");
            }
        }
        else if (other.gameObject.CompareTag("Shield"))
        {
            var networkObject = other.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                CollectItemServerRpc(networkObject.NetworkObjectId, "Shield");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CollectItemServerRpc(ulong networkObjectId, string itemType)
    {
        var networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
        if (networkObject != null)
        {
            CollectItemClientRpc(itemType);
            networkObject.Despawn(true);
        }
    }

    [ClientRpc]
    private void CollectItemClientRpc(string itemType)
    {
        if (itemType == "Boomb")
        {
            ItemsCounter.instancie.IncreaseBooms(Boomb);
        }
        else if (itemType == "Flame")
        {
            ItemsCounter.instancie.IncreaseFlame(Flame);
        }
        else if (itemType == "Key")
        {
            Debug.Log("Portales activados");
            if (TeleportController != null)
            {
                TeleportController.SetActive(true);
            }
            if (KeyImage != null)
            {
                KeyImage.SetActive(true);
            }
        }
        else if (itemType == "Shield")
        {
            Debug.Log("Display Shield");
            shieldIsActive = true;
            if (ShieldImage != null)
            {
                ShieldImage.SetActive(true);
            }
        }
    }
}
