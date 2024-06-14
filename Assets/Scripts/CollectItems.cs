using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CollectItems : NetworkBehaviour
{
    private GameObject TeleportController;
    public GameObject KeyImage;
    public GameObject ShieldImage;

    public NetworkVariable<bool> shieldIsActive = new NetworkVariable<bool>(false);

    private ItemsCounter itemsCounter;

    private void Start()
    {
        itemsCounter = ItemsCounter.instancie;
        if (itemsCounter == null)
        {
            Debug.LogError("ItemsCounter instance not found in the scene!");
        }

        if (IsServer)
        {
            // Buscar y desactivar los objetos TeleportController y KeyImage
            SearchAndDeactivateObjects();
        }

        if (IsOwner)
        {
            TeleportController = GameObject.FindGameObjectWithTag("TeleportController");
            ShieldImage.SetActive(false);
        }

        shieldIsActive.OnValueChanged += OnShieldStateChanged;
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
            var networkObject = other.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                CollectItemServerRpc(networkObject.NetworkObjectId, "Boomb");
            }
        }
        else if (other.gameObject.CompareTag("Flame"))
        {
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
            itemsCounter.IncreaseBooms(1);
        }
        else if (itemType == "Flame")
        {
            itemsCounter.IncreaseFlame(1);
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
            shieldIsActive.Value = true;
            if (ShieldImage != null)
            {
                ShieldImage.SetActive(true);
            }
        }
    }

    private void OnShieldStateChanged(bool oldValue, bool newValue)
    {
        if (ShieldImage != null)
        {
            ShieldImage.SetActive(newValue);
        }
    }
}
