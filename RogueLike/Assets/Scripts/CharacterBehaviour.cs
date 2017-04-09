using UnityEngine;

public class CharacterBehaviour : MonoBehaviour
{
    [SerializeField]
    private float m_movementSpeed;

    [SerializeField]
    private float m_maxMovementSpeed;

    private float m_acceleration;

    [SerializeField]
    protected HealthBehaviour m_health;

    [SerializeField]
    protected WeaponBehaviour m_equippedWeapon;

    [SerializeField]
    private ParticleSystem m_takeDamageEffect;

    private bool isRooted;

    private Team m_team;

    public WeaponBehaviour EquippedWeapon
    {
        get { return m_equippedWeapon; }
    }

    public Vector3 Accelerate(Vector3 accelDir, Vector3 prevVelocity, float accelerate)
    {
        float yVel = prevVelocity.y;
        prevVelocity.y = 0f;

        float accelVel = accelerate * Time.deltaTime; // Accelerated velocity in direction of movment

        if (accelVel > m_maxMovementSpeed)
        {
            accelVel = m_maxMovementSpeed;
        }

        Vector3 newVel = prevVelocity + accelDir * accelVel;
        newVel.y = yVel;

        return newVel;
    }

    public Vector3 Move(Vector3 dir, Vector3 prevVelocity, float friction, float acceleration)
    {
        // Apply Friction
        float yVel = prevVelocity.y;
        prevVelocity.y = 0f;

        float speed = prevVelocity.magnitude;
        if (speed != 0f) // To avoid divide by zero errors
        {
            float drop = speed * friction * Time.deltaTime;
            prevVelocity *= Mathf.Max(speed - drop, 0f) / speed; // Scale the velocity based on friction.
        }
        prevVelocity.y = yVel;
        return Accelerate(dir, prevVelocity, acceleration);
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

        if (!m_takeDamageEffect)
        {
            Debug.LogError(string.Format("No DamageEffect ParticleSystem attached to {0}", gameObject.name));
            gameObject.SetActive(false);
            return;
        }
        m_takeDamageEffect.Stop();

        m_team = new Team();

        //Debug.Log(string.Format("{0}: Team {1}", gameObject.name, m_team.ID));
    }

    public void SetLookDirection(Vector3 dir)
    {
        if (dir == Vector3.zero) { return; }

        transform.forward = dir.normalized;
    }

    public void Damage(int amount)
    {
        m_health.Modify(-amount);
        Debug.Log(string.Format("{0} has been taken {1} hp damage", gameObject.name, amount));
        m_takeDamageEffect.Play();
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


}