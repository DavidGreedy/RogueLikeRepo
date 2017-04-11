using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RangedWeaponProperties", menuName = "ScriptableObject/RangedWeaponProperties", order = 0)]
public class RangedWeaponProperties : ScriptableObject
{
    public float muzzleVelocity;
    public float spread;
    public int maxAmmo;
    public int startingAmmo;
    public enum weaponName {M4, Thompson, Kar98k };
    public weaponName _weaponName; 

    public Vector3 projectileOrigin;

    public ProjectileBehaviour projectile;
}