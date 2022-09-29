using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Level_SceneManager : MonoBehaviourPun
{
    [Header("Game Settings")]
    [SerializeField] GameObject[] spawnPoints;
    public int playerId;

    [Header("Players")]
    // Find Players in scene
    [SerializeField] public GameObject redPlayer;
    [SerializeField] public GameObject bluePlayer;

    //[SerializeField] public NexusBook RedBook;
    //[SerializeField] public NexusBook BlueBook;

    [Header("Core Objectives")]
    [SerializeField] public Text redNexus_HP;
    [SerializeField] public Text blueNexus_HP;

    public int redHP = -1;
    public int blueHP = -1;

    public DragonBoss Warlock_Dragon;
    public GameObject Seperator;

    public AudioSource audioSource;
    public AudioClip game;
    public AudioClip victory;

    // Start is called before the first frame update
    void Start()
    {
        // Reseting to Default
        //Seperator.SetActive(true);
        //Warlock_Dragon.gameObject.SetActive(false);

        // Checking on Network Connecting, Perform Player methods
        if ( PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }

    void FixedUpdate()
    {
        FindPlayers();
        Update_Objectives_Score();
    }

    // Assign players
    private void FindPlayers()
    {
        // Unless we Find all the required players, we will continue
        if (redPlayer != null && bluePlayer != null)
            return;

        GameObject[] gos = (GameObject[])FindObjectsOfType(typeof(GameObject));
        for (int x = 0; x < gos.Length; x++)
        {
            if (gos[x].name == ("Red(Clone)"))
            {
                redPlayer = gos[x].gameObject;
            }

            if (gos[x].name == ("Blue(Clone)"))
            {
                bluePlayer = gos[x].gameObject;
            }
        }
    }

    public void Update_Objectives_Score()
    {
        // Printing
        if (redPlayer == null && bluePlayer == null)
            return;

        try
        {
            redHP = redPlayer.GetComponent<PlayerMovement>().health;
            blueHP = bluePlayer.GetComponent<PlayerMovement>().health;

            /// UI presenting
            redNexus_HP.GetComponent<Text>().text = redHP.ToString();
            blueNexus_HP.GetComponent<Text>().text = blueHP.ToString();
        }
        catch {
            Debug.Log("Syncing player's health values");
        }

    }

    private void SpawnPlayer()
    {
        // Room Creator will get the first Character by Default !
        int player = 0;

        if (!PhotonNetwork.IsMasterClient)
        {
            player = 1;
        }

        // Changing player prefab per connection
        playerId = StaticDataManager.characterId = player;
        GameObject Player;
        if (playerId < 1)
        {
            // Pun works in a way that you need to specify the required prefab as string in "Resources" Dir, Tweaked Dir for Players into "Network Prefabs".
            Player = PhotonNetwork.Instantiate("Red", spawnPoints[player].transform.position, Quaternion.identity);
        }
        else
        {
            Player = PhotonNetwork.Instantiate("Blue", spawnPoints[player].transform.position, Quaternion.identity);
        }

    }


    // Winning Method
    public void EndGame(bool blue, bool red)
    {
        if (blue)
        {
            Debug.Log("blue has lost first");
            Warlock_Dragon.target = bluePlayer.transform;
            StartCoroutine(Red_Wins());
        }

        if (red)
        {
            Debug.Log("red has lost first");
            Warlock_Dragon.target = redPlayer.transform;
            StartCoroutine(Blue_Wins());
        }

        Seperator.SetActive(false);
        Warlock_Dragon.gameObject.SetActive(true);
        redNexus_HP.gameObject.SetActive(false);
        blueNexus_HP.gameObject.SetActive(false);
    }

    IEnumerator Red_Wins()
    {
        // Hold the Winner
        redPlayer.GetComponent<PlayerMovement>().CanMove = false;

        yield return new WaitForSeconds(5f);
        bluePlayer.GetComponent<PlayerMovement>().CanMove = false;


        yield return new WaitForSeconds(5f);
        redPlayer.GetComponent<PlayerMovement>().winCam.gameObject.SetActive(true);

        // Ending Level
        StartCoroutine(ReturnToMainMenu());
    }

    IEnumerator Blue_Wins()
    {
        // Hold the Winner
        bluePlayer.GetComponent<PlayerMovement>().CanMove = false;

        yield return new WaitForSeconds(5f);
        redPlayer.GetComponent<PlayerMovement>().CanMove = false;


        yield return new WaitForSeconds(10f);
        bluePlayer.GetComponent<PlayerMovement>().winCam.gameObject.SetActive(true);

        // Ending Level
        StartCoroutine(ReturnToMainMenu());
    }

    IEnumerator ReturnToMainMenu()
    {
        audioSource.clip = victory;
        audioSource.enabled = false;
        yield return new WaitForSeconds(1f);

        audioSource.enabled = true;
        yield return new WaitForSeconds(10f);
        if (PhotonNetwork.IsConnected)
        {
            StartCoroutine(Disconnect());
        }
    }

    IEnumerator Disconnect()
    {
        PhotonNetwork.Disconnect();
        while (PhotonNetwork.IsConnected)
        {
            yield return null;
            Debug.Log("Disconnecting. . .");
        }

        Debug.Log("DISCONNECTED!");
        PhotonNetwork.LoadLevel(0);
    }
}
