using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinLobbyButton : MonoBehaviour
{
    public bool needPasword;
    public string lobbyId;

    public void JoinLobbyButtonPressed()
    {
        LobbyManager.Instance.JoinLobby(lobbyId, needPasword);
    }
}
