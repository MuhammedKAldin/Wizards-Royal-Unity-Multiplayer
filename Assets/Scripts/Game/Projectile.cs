using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public MagicType abilityType;
    public enum MagicType
    {
        Light,
        Ice,
        Dark
    }

    public GameObject VFXspell;
    private bool On;

    private void OnCollisionEnter(Collision collision)
    {
        // Prevent multiple spawnings
        if (On)
            return;

        //if(abilityType == MagicType.Light)
        //{
        if (collision.gameObject.tag == "Player")
        {
            On = true;
            var _projectile = Instantiate(VFXspell, new Vector3(transform.position.x, transform.position.y - 1.5F, transform.position.z), VFXspell.transform.rotation);
            _projectile.transform.localScale = new Vector3(0.42f, 0.42f, 0.42f);
            // Delay the destroy from here to Register it on the PlayerMovement RPC (More accuracy), after 1s
            GetComponent<MeshRenderer>().enabled = false;
            Destroy(gameObject,1f);
        }

        if (collision.gameObject.tag == "GroundHit")
        {
            On = true;
            var _projectile = Instantiate(VFXspell, new Vector3(transform.position.x, transform.position.y + 1F, transform.position.z), VFXspell.transform.rotation);
            _projectile.transform.localScale = new Vector3(0.42f, 0.42f, 0.42f);
            Destroy(gameObject, 1f);
        }
        //}

        Destroy(gameObject, 5f);
    }
}
