using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCrate : MonoBehaviour {

    public GameObject WeaponPickUp;
    private MeshRenderer meshRenderer;

	// Use this for initialization
	void Start ()
    {
        meshRenderer = GetComponent<MeshRenderer>();	
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    void CrateDestroy()
    {
        meshRenderer.enabled = false;
        WeaponPickUp.SetActive(true);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.collider.gameObject.GetComponent<ProjectileBehaviour>())
        {
            CrateDestroy();
        }
    }
}
