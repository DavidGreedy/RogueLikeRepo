﻿using System.Collections;
using UnityEngine;

public class CharacterBehaviour : SolidMonoBehaviour
{
    [SerializeField]
    private float m_movementSpeed;

    [SerializeField]
    private float m_rotationSpeed;


    [SerializeField]
    private float m_friction;

    [SerializeField]
    private float m_acceleration;

    [SerializeField]
    protected HealthBehaviour m_health;

    private Team m_team;

    [SerializeField]
    private bool m_isMovementLocked;

    [SerializeField]
    private bool m_canUseAction;

    [SerializeField]
    private Vector3 m_moveVec;

    [SerializeField]
    private Vector3 m_lookVec;

    public Vector3 MoveVector
    {
        get { return m_moveVec; }
        set { m_moveVec = value; }
    }

    public Vector3 LookVector
    {
        get { return m_lookVec; }
        set { m_lookVec = value; }
    }

    [SerializeField]
    private InventoryBehaviour m_inventory;

    [SerializeField]
    private Animator m_animator;

    [SerializeField]
    private Rigidbody m_rigidbody;

    public WeaponBehaviour ActiveWeapon
    {
        get { return m_inventory.ActiveWeapon; }
    }

    public InventoryBehaviour Inventory
    {
        get { return m_inventory; }
    }

    public Vector3 Accelerate(Vector3 accelDir, Vector3 prevVelocity, float acceleration)
    {
        float yVel = prevVelocity.y;
        prevVelocity.y = 0f;

        float accelVel = acceleration * Time.deltaTime; // Accelerated velocity in direction of movment

        if (accelVel > m_movementSpeed)
        {
            accelVel = m_movementSpeed;
        }

        Vector3 newVel = prevVelocity + accelDir * accelVel;
        newVel.y = yVel;

        return newVel;
    }

    public Vector3 Move(Vector3 dir, float friction, float acceleration)
    {
        // Apply Friction
        Vector3 prevVel = m_rigidbody.velocity;
        float yVel = prevVel.y;
        prevVel.y = 0f;

        float speed = prevVel.magnitude;
        if (speed != 0f) // To avoid divide by zero errors
        {
            float drop = speed * friction * Time.deltaTime;
            prevVel *= Mathf.Max(speed - drop, 0f) / speed; // Scale the velocity based on friction.

        }
        prevVel.y = yVel;

        return Accelerate(dir, prevVel, acceleration);
    }

    void RotateTowardsMovementDir(Vector3 targetDirection)
    {
        if (targetDirection != Vector3.zero /*&& !isStrafing && !isRolling && !isBlocking*/)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetDirection), Time.deltaTime * m_rotationSpeed);
        }
    }

    public IEnumerator LockMovement(float delayTime, float lockTime)
    {
        yield return new WaitForSeconds(delayTime);
        m_isMovementLocked = true;
        m_canUseAction = false;
        m_animator.SetBool("Moving", false);
        m_rigidbody.velocity = Vector3.zero;
        m_rigidbody.angularVelocity = Vector3.zero;
        m_animator.applyRootMotion = true;
        yield return new WaitForSeconds(lockTime);
        m_canUseAction = true;
        m_isMovementLocked = false;
        m_animator.applyRootMotion = false;
    }

    void Start()
    {
        if (!m_health)
        {
            Debug.LogError(string.Format("No HealthBehaviour attached to {0}", gameObject.name));
            gameObject.SetActive(false);
            return;
        }
        m_health.OnDeath += Kill;

        m_team = new Team();

        //Debug.Log(string.Format("{0}: Team {1}", gameObject.name, m_team.ID));
    }

    private void LateUpdate()
    {
        Vector3 movement = m_moveVec.magnitude > 0.1f && m_moveVec.magnitude < 1.0f ? m_moveVec : m_moveVec.normalized;
        Vector3 lookDirection = m_lookVec.normalized;

        if (m_lookVec.magnitude > 0.1f)
        {
            RotateTowardsMovementDir(lookDirection);
        }
        else if (m_moveVec.magnitude > 0.1f)
        {
            RotateTowardsMovementDir(m_moveVec.normalized);
        }

        if (!m_isMovementLocked && m_moveVec.magnitude > 0.1f)
        {
            m_rigidbody.velocity = m_moveVec * m_movementSpeed;
            m_animator.SetBool("Moving", true);
            m_animator.SetFloat("Velocity X", Mathf.Abs(m_rigidbody.velocity.x));
            m_animator.SetFloat("Velocity Z", Mathf.Abs(m_rigidbody.velocity.z));
        }
        else
        {
            m_rigidbody.velocity = Vector3.zero;
            m_animator.SetBool("Moving", false);
        }
    }

    public void Damage(int amount)
    {
        m_health.Modify(-amount);
        Debug.Log(string.Format("{0} has been taken {1} hp damage", gameObject.name, amount));
    }

    public void Heal(int amount)
    {
        m_health.Modify(amount);
        Debug.Log(string.Format("{0} has been healed for {1} hp", gameObject.name, amount));
    }

    public void Kill()
    {
        Debug.Log(string.Format("{0} has been killed", gameObject.name));
        Destroy(this.gameObject);
    }

    public override void Hit(RaycastHit hit, ProjectileBehaviour projectile)
    {
        base.Hit(hit, projectile);
        Damage(projectile.Damage);
    }

    void Hit()
    {

    }

    void FootL()
    {

    }

    void FootR()
    {

    }

    void Jump()
    {

    }

    void Land()
    {

    }
}