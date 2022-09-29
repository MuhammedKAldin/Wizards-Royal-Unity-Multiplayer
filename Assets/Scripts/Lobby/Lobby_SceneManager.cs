using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

public class Lobby_SceneManager : MonoBehaviourPunCallbacks
{
    [Tooltip("This gameobject is disabled by default, and it's visibility is handled through code.")]
    [SerializeField] public Button findMatchButton;

    [Tooltip("This gameobject is disabled by default, and it's visibility is handled through code.")]
    [SerializeField] public Button SearchPanel;

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        //base.OnConnectedToMaster();
        // Once we are online and connected (to/inside) Pun Server
        Debug.Log($"We are connected on {PhotonNetwork.CloudRegion} Server!!");
        PhotonNetwork.AutomaticallySyncScene = true;
        Menu_Default();
    }

    // summary : handles the default menu state, once we are inside the Server
    public void Menu_Default()
    {
        findMatchButton.gameObject.SetActive(true);
        SearchPanel.gameObject.SetActive(false);
    }

    // summary : handles the default menu state, once we are inside the Server
    public void FindMatch()
    {
        SearchPanel.gameObject.SetActive(true);
        findMatchButton.gameObject.SetActive(false);

        PhotonNetwork.JoinRandomRoom();
        Debug.Log("Searching for a Match ...");
    }

    // summary : handles the state of room joining failure and attempt to create one
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        //base.OnJoinRandomFailed(returnCode, message);
        Debug.Log("Couldn't Find Room - Creating a Room! ");
        MakeRoom();
    }

    // summary : creation of a room by the sole-player
    void MakeRoom()
    {
        int randomRoomName = Random.Range(0, 5000);
        RoomOptions roomOptions = new RoomOptions()
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = 2
        };
        PhotonNetwork.CreateRoom($"Wizards_{randomRoomName}", roomOptions);
        Debug.Log("Room Created, Waiting for players to join...");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //base.OnPlayerEnteredRoom(newPlayer);
        if(PhotonNetwork.CurrentRoom.PlayerCount == 2 && PhotonNetwork.IsMasterClient)
        {
            Debug.Log($"{PhotonNetwork.CurrentRoom.PlayerCount} /2 Starting Game");

            // Start Game
            PhotonNetwork.LoadLevel(1);
        }
    }

    // summary : leaving the room/match making procedure
    public void StopSearch()
    {
        findMatchButton.gameObject.SetActive(true);
        SearchPanel.gameObject.SetActive(false);
        PhotonNetwork.LeaveRoom();
    }

}
