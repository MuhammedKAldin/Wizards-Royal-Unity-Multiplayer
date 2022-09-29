using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NexusBook : MonoBehaviourPun
{
    public Level_SceneManager sceneManager;
    public bool isRed;
    public bool isBlue;
    public int health = 100;

    private void Start()
    {
        sceneManager = GameObject.FindObjectOfType<Level_SceneManager>();
        sceneManager.Update_Objectives_Score();
    }

    private void OnTriggerEnter(Collider other)
    {
        photonView.RPC(nameof(NexusDamage), RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void NexusDamage()
    {
        if (health < 0)
        {
            if (isRed)
            {
                sceneManager.EndGame(false, true);
            }
            if (isBlue)
            {
                sceneManager.EndGame(true, false);
            }
            health = 0;
            return;
        }

        health -= 10;
        sceneManager.Update_Objectives_Score();
    }

}
