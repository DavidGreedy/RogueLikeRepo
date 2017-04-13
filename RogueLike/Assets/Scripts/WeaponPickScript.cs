using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamepadInput;

public class WeaponPickScript : MonoBehaviour
{
    public RangedWeaponProperties thisWeapon;

    void Start()
    {

    }

    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform)
        {
            WeaponBehaviour weapon = other.GetComponent<CharacterBehaviour>().EquippedWeapon;
            if (weapon.gameObject.name == thisWeapon.name)
            {
                SameWeapon();
                print("More ammo");
                PickupAction();
            }
            else if (weapon.gameObject.name != thisWeapon.name && GamePad.GetButton(GamePad.Button.X, GamePad.Index.One))
            {
                PickupWeapon();
                print("Pick up new weapon");
            }
            print("Yes");
        }
    }

    void SameWeapon()
    {

    }

    void PickupWeapon()
    {

    }

    void PickupAction()
    {
        this.gameObject.SetActive(false);
    }
}