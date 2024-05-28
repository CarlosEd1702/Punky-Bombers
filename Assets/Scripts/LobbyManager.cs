using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private GameObject lobbyCreationParent;
    [SerializeField] private TMP_InputField createLobbyName;
    [SerializeField] private TMP_InputField createLobbyMaxPlayerField;
    [SerializeField] private GameObject lobbyListParent;

    public string joinLobbyID;

    // Start is called before the first frame update
    private async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async void ShowLobbies()
    {
        while(Application.isPlaying && !lobbyCreationParent.activeInHierarchy)
        {
            /*QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            foreach(Transform t in lobbyContentparent)
            {
                Destroy(t.gameObject);
            }

            foreach(Lobby lobby in queryResponse.Results)
            {
                Transform newLobbyItem = Instantiate(lobbyItemsPrefab, lobbycontentparent);
            }*/

            await Task.Delay(1000); 
        }
    }

    public void ExitLobbyCreationButton()
    {
        lobbyCreationParent.SetActive(false);
        lobbyListParent.SetActive(true);
        ShowLobbies();
    }
    
    public async void CreateLobby()
    {
        if(!int.TryParse(createLobbyMaxPlayerField.text, out int maxplayer))
        {
            Debug.LogWarning("Incorrect player count");
            return;
        }

        Lobby createLobby = null;

        try
        {
            createLobby = await LobbyService.Instance.CreateLobbyAsync(createLobbyName.text, maxplayer);
            joinLobbyID = createLobby.Id;
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}
