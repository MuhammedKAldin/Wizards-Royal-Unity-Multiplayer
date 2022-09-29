using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

// Spawning Prefabs assigned from ./Assets/NetworkPrefabs
// to be used in Game Scene from this script as auto-assign inside the Lobby Scene
public class PoolManager : MonoBehaviour
{
    public List<GameObject> Prefabs;

    void Start()
    {
        DefaultPool pool = PhotonNetwork.PrefabPool as DefaultPool;
        if (pool != null && this.Prefabs != null)
        {
            foreach (GameObject prefab in this.Prefabs)
            {
                try {
                    pool.ResourceCache.Add(prefab.name, prefab);
                }
                catch {
                    Debug.Log("Pooling into Scene");
                }
            }
        }
    }
}