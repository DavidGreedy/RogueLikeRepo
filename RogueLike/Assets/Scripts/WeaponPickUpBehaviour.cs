using UnityEngine;
using GamepadInput;

public class WeaponPickUpBehaviour : MonoBehaviour
{
    public WeaponBehaviour m_weapon;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform != null)
        {
            InventoryBehaviour targetInventory = other.GetComponent<InventoryBehaviour>();

            if (targetInventory)
            {
                targetInventory.SwapActiveWeapon(m_weapon);
                gameObject.SetActive(false);
            }
        }
    }
}