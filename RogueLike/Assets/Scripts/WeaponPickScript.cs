using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamepadInput;

public class WeaponPickScript : MonoBehaviour {

    RangedWeaponProperties thisWeapon;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider col)
    {
        if (col.GetComponent<Collider>().gameObject.GetComponent<CharacterController>()) //Appearantly the old way is deprecated as this is how you do it now...
        {
            TwinStickCrosshairBehaviour twinStick = col.GetComponent<Collider>().gameObject.GetComponent<TwinStickCrosshairBehaviour>();
            if (twinStick.m_weapon.m_weaponProperties._weaponName == thisWeapon._weaponName)
            {
                print("More ammo");
            }
            else if(twinStick.m_weapon.m_weaponProperties._weaponName != thisWeapon._weaponName && GamePad.GetButton(GamePad.Button.X, GamePad.Index.One))
            {
                print("Pick up new weapon");
            }
            print("Yes");
        }
    }
}
