using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MagicStaff : MonoBehaviourPun
{
    public int Damage;
    public List<Projectile> projectileList;

    private void Start()
    {
        /// No longer assigned from here...
        //public void ChangeAppearance()
        //{
        //    magicalProjectile = projectileList.Where(t => t.abilityType.ToString() == abilityType.ToString()).FirstOrDefault().GetComponent<Rigidbody>();
        //}
    }
}
