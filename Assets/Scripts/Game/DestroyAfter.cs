using UnityEngine;
using System.Collections;
public class DestroyAfter : MonoBehaviour {

    public float time;

	void Start () {
        Invoke("Disable", time);
	}

    void OnEnable() {
        Invoke("Disable", time);
    }

    public void Disable() {
        Destroy(gameObject);
    }
}
