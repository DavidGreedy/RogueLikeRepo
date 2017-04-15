using UnityEngine;
using GamepadInput;

public class WeaponPickUpBehaviour : MonoBehaviour
{
    public WeaponBehaviour m_weapon;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform != null)
        {
            CharacterBehaviour character = other.GetComponent<CharacterBehaviour>();

            if (character)
            {
                //character.ActiveWeapon = m_weapon;
            }
        }
    }
}