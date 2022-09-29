using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonBoss : MonoBehaviourPun
{
    private bool Aim;
    private bool FireAtTarget;
    private bool FlyAway;
    private Animator anim;
    public GameObject FireVFX;
    public GameObject head;
    public Transform target;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        anim = GetComponent<Animator>();

        // Look at the Player whom failed the Game, and Shower him with flames
        yield return new WaitForSeconds(2f);
        Aim = true;

        yield return new WaitForSeconds(2f);
        FireAtTarget = true;

        // Small Delay before actual fire 
        yield return new WaitForSeconds(0.4f);
        FireVFX.SetActive(true);

        yield return new WaitForSeconds(3f);
        FlyAway = true;
    }

    private void Update()
    {
        // Checking if we are allowed to do the following behaviours
        if (!Aim)
            return;

        LookAtTarget();
        FireBreath();

        photonView.RPC(nameof(EndAndFlyAway), RpcTarget.All);
    }

    void LookAtTarget()
    {
        // Look at and dampen the rotation
        try
        {
            var rotation = Quaternion.LookRotation(head.transform.position - target.position);
            head.transform.rotation = Quaternion.Slerp(head.transform.rotation, rotation, Time.deltaTime * 6.0f);
        }
        catch 
        {
            Debug.Log("Boss Dragon is staring at the target...");
        }

	}

    void FireBreath()
    {
        if (!FireAtTarget)
            return;

        anim.SetBool("Fire", true);
    }

    [PunRPC]
    void EndAndFlyAway()
    {
        if (!FlyAway)
            return;

        anim.SetBool("Fire", false);
        FireVFX.SetActive(false);
        if (target.GetComponent<PlayerMovement>() != null)
        {
            target.GetComponent<PlayerMovement>().isDead = true;
        }
    }
}
