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

    public static LobbyManager Instance { get; private set; }

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

    public async void JoinLobby(string lobbyID, bool needPasword)
    {
        if(needPasword)
        {
            try
            {
                
            }
            catch(LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
        else
        {
            try
            {
                await LobbyService.Instance.JoinLobbyByIdAsync(lobbyID);

                joinLobbyID = lobbyID;
            }
            catch(LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    private async void ShowLobbies()
    {
       /* while(Application.isPlaying && lobbyCreationParent.activeInHierarchy)
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            foreach(Transform t in lobbyContentParent)
            {
                Destroy(t.gameObject);
            }

            foreach(Lobby lobby in queryResponse.Results)
            {
                Transform newLobbyItem = Instantiate(lobbyItemsPrefab, lobbyContentParent);
                newLobbyItem.GetChild(0).GetComponent<TextMeshProUGUI>().text = lobby.Name;
                newLobbyItem.GetChild(2).GetComponent<TextMeshProUGUI>().text = lobby.Players.Count + "/" + lobby.MaxPlayers;
            }

            await Task.Delay(1000); 
        }*/
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

    private async void LobbyHeartbeat(Lobby lobby)
    { 
        while (true)
        {
            if (lobby == null)
            {
                return;
            }

            await LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id);

            await Task.Delay(15 * 1000);
        }
    }

}
