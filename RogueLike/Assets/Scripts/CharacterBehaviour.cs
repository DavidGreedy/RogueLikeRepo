using System.Collections;
using UnityEngine;

public class CharacterBehaviour : SolidMonoBehaviour
{
    [SerializeField]
    private float m_movementSpeed;

    [SerializeField]
    private float m_strafeSpeed;

    [SerializeField]
    private float m_rotationSpeed;

    [SerializeField]
    protected HealthBehaviour m_health;

    private Team m_team;

    [SerializeField]
    private bool m_isMovementLocked;

    [SerializeField]
    private bool m_canUseAction;

    [SerializeField]
    private bool m_isAlive = true;

    [SerializeField]
    private bool m_isStrafing;

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
    private LayerMask m_attackLayer;

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

    public float MovementSpeed
    {
        get
        {
            return m_movementSpeed;
        }
    }

    public bool IsAlive
    {
        get
        {
            return m_isAlive;
        }
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
        m_health.OnDeath += Death;

        m_team = new Team();

        //Debug.Log(string.Format("{0}: Team {1}", gameObject.name, m_team.ID));
    }

    private void LateUpdate()
    {
        if (!m_isAlive)
        {
            return;
        }
        m_moveVec.y = 0;
        Vector3 movement = m_moveVec.magnitude > 0.1f && m_moveVec.magnitude < 1.0f ? m_moveVec : m_moveVec.normalized;
        Vector3 lookDirection = m_lookVec.normalized;

        if (!m_isMovementLocked)
        {
            if (m_lookVec.magnitude > 0.1f)
            {
                RotateTowardsMovementDir(lookDirection);
            }
            else if (m_moveVec.magnitude > 0.1f)
            {
                RotateTowardsMovementDir(m_moveVec.normalized);
            }
        }

        if (!m_isMovementLocked && m_moveVec.magnitude > 0.1f)
        {
            m_isStrafing = m_lookVec.magnitude > 0.1f;//Vector3.Dot(transform.forward, movement.normalized) < 0.5f;
            m_rigidbody.velocity = movement * (m_isStrafing ? m_strafeSpeed : m_movementSpeed);
            m_animator.SetBool("Moving", true);

            Vector3 forwardVel = transform.InverseTransformVector(m_rigidbody.velocity); // Gets the velocity in the forward direction
            m_animator.SetFloat("Velocity X", forwardVel.x / m_strafeSpeed);
            m_animator.SetFloat("Velocity Z", forwardVel.z / m_movementSpeed);
        }
        else
        {
            m_isStrafing = false;
            m_rigidbody.velocity = Vector3.zero;
            m_animator.SetBool("Moving", false);
        }
        if (!m_isMovementLocked)
        {
            m_animator.SetBool("Strafing", m_isStrafing);
        }

        m_moveVec = Vector3.zero;
        m_lookVec = Vector3.zero;
    }

    public void Damage(int amount)
    {
        StartCoroutine(LockMovement(0f, 0.3f));
        m_health.Modify(-amount);
        Debug.Log(string.Format("{0} has been taken {1} hp damage", gameObject.name, amount));
        m_animator.SetTrigger("GetHit1Trigger");
    }

    public void Heal(int amount)
    {
        m_health.Modify(amount);
        Debug.Log(string.Format("{0} has been healed for {1} hp", gameObject.name, amount));
    }

    //public void Kill()
    //{
    //    Debug.Log(string.Format("{0} has been killed", gameObject.name));
    //    Destroy(this.gameObject);
    //}

    //public override void Hit(RaycastHit hit, ProjectileBehaviour projectile)
    //{
    //    base.Hit(hit, projectile);
    //    Damage(projectile.Damage);
    //}

    public void Attack()
    {
        if (m_canUseAction && m_isAlive)
        {
            StartCoroutine(LockMovement(0.1f, 0.5f));
            m_animator.SetTrigger("Attack6Trigger");
        }
    }

    public void Death()
    {
        m_isAlive = false;
        m_rigidbody.isKinematic = true;
        m_animator.SetTrigger("Death1Trigger");

        GetComponent<Collider>().enabled = false;
        this.enabled = false;
    }

    void Hit()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position + Vector3.up, transform.forward);
        Debug.DrawRay(ray.origin, ray.direction);

        Physics.Raycast(ray, out hit, m_attackLayer);

        if (hit.transform != null)
        {
            if (hit.distance < 0.7f)
            {
                print(string.Format("HIT: {0}", hit.collider.name));
                CharacterBehaviour hitCharacter = hit.collider.GetComponent<CharacterBehaviour>();

                if (hitCharacter)
                {
                    hitCharacter.Damage(40);
                }
            }
        }
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