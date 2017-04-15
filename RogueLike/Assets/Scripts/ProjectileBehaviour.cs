using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class ProjectileBehaviour : MonoBehaviour
{
    [SerializeField]
    private float m_destroySpeedSqrd;

    [SerializeField]
    private FastCollision m_fastCollision;

    [SerializeField]
    private int m_damage;

    public int Damage { get { return m_damage; } }

    public void Launch(Vector3 position, Vector3 velocity, Quaternion rotation)
    {
        transform.position = position;
        m_fastCollision.Init(velocity.normalized, velocity.magnitude);
        transform.rotation = rotation;
        m_fastCollision.OnHit += HitObject;
    }

    public void Launch(Vector3 position, Vector3 velocity)
    {
        transform.position = position;
        m_fastCollision.Init(velocity.normalized, velocity.magnitude);
        transform.rotation = Quaternion.LookRotation(velocity);
        m_fastCollision.OnHit += HitObject;
    }

    private void HitObject(RaycastHit hit, SolidMonoBehaviour hitObject)
    {
        print("Hit " + hitObject.name);
        hitObject.Hit(hit, this);
    }
}