using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastCollision : MonoBehaviour
{
    //public Rigidbody m_rigidbody;
    public LayerMask m_layer;

    private Ray ray;
    private RaycastHit hit;

    [SerializeField]
    private Vector3 m_position;

    [SerializeField]
    Vector3 m_direction;

    [SerializeField]
    private float m_remainingDistance;

    public Vector3 velocity;

    private float speed = 100f;

    private Queue<Vector3> prevPositions;

    [SerializeField]
    private float m_bounceAngle = 30;

    private void Start()
    {
        prevPositions = new Queue<Vector3>(20);
    }

    void Update()
    {
        m_position = transform.position;
        m_direction = velocity.normalized;
        m_remainingDistance = velocity.magnitude * Time.fixedDeltaTime;

        Tick(ref m_position, ref m_direction, ref m_remainingDistance);

        Debug.DrawLine(transform.position, m_position);

        transform.forward = m_direction;
        transform.position = m_position;
        velocity = m_direction * speed;
        if (prevPositions.Count > 20)
        {
            prevPositions.Dequeue();
        }
        prevPositions.Enqueue(transform.position);
        Debug.DrawLine(transform.position, transform.position + velocity * Time.fixedDeltaTime);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && prevPositions.Count > 20)
        {
            Vector3[] p = prevPositions.ToArray();
            for (int i = 0; i < p.Length - 1; i++)
            {
                Gizmos.DrawLine(p[i], p[i + 1]);
            }
        }
    }

    void Tick(ref Vector3 position, ref Vector3 direction, ref float remainingDistance)
    {
        ray.origin = position;
        ray.direction = direction;

        Physics.Raycast(ray, out hit, remainingDistance, m_layer);

        //Debug.DrawRay(ray.origin, ray.direction, Color.cyan);

        if (hit.transform != null)
        {
            float angle = Vector3.Angle(-direction, hit.normal);
            //Debug.Log(angle);

            if (angle < 90 - m_bounceAngle)
            {
                gameObject.SetActive(false);
                return;
            }
            position = hit.point;
            direction = Vector3.Reflect(direction, hit.normal).normalized;
            remainingDistance -= hit.distance;

            prevPositions.Enqueue(position);

            //Debug.DrawLine(position, position + (direction * remainingDistance));

            Tick(ref position, ref direction, ref remainingDistance);
        }
        else
        {
            position += direction * remainingDistance;
        }
    }
}
