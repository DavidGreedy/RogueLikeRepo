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
        m_fastCollision.velocity = velocity;
        transform.rotation = rotation;
    }

    public void Launch(Vector3 position, Vector3 velocity)
    {
        transform.position = position;
        m_fastCollision.velocity = velocity;
        transform.rotation = Quaternion.LookRotation(velocity);
    }

    private void Update()
    {
        //if (m_fastCollision.velocity.sqrMagnitude < m_destroySpeedSqrd)
        //{
        //    this.gameObject.SetActive(false);
        //}
    }

    private void OnCollisionEnter(Collision other)
    {
        //Debug.Log("Collision with " + LayerMask.LayerToName(other.gameObject.layer) + " Layer");

        switch (other.gameObject.layer)
        {

        }
        CharacterBehaviour c = other.collider.GetComponent<CharacterBehaviour>();
        if (c != null)
        {
            c.Damage(Damage);
            this.gameObject.SetActive(false);
        }
        //switch (other.collider.tag)
        //{
        //    case "Character":
        //        {
        //            CharacterBehaviour c = other.collider.GetComponent<CharacterBehaviour>();
        //            c.Damage(Damage);
        //            Destroy(this.gameObject);
        //            break;
        //        }
        //}
    }
}