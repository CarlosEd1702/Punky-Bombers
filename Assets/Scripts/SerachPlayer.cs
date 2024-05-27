using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class SearchPlayer : NetworkBehaviour
{
    [SerializeField] private Button A_Button;
    private GameObject player;
    private PlayerControls playerC;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            FindPlayerAndSetupButton();
        }
    }

    private void FindPlayerAndSetupButton()
    {
        // Encuentra el jugador con el componente de red que sea el propietario
        foreach (var networkObject in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
        {
            if (networkObject.IsOwner && networkObject.CompareTag("Player"))
            {
                player = networkObject.gameObject;
                playerC = player.GetComponent<PlayerControls>();
                break;
            }
        }

        if (player != null && playerC != null && A_Button != null)
        {
            SetupButton();
        }
        else
        {
            Debug.LogError("Player, PlayerControls, or A_Button not found.");
        }
    }

    private void SetupButton()
    {
        A_Button.onClick.RemoveAllListeners(); // Limpiar cualquier listener anterior
        A_Button.onClick.AddListener(playerC.SpawnBomb);
        Debug.Log("Button listener assigned.");
    }
}
