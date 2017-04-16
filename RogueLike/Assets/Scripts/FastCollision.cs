using System;
using System.Collections.Generic;
using UnityEngine;

public class FastCollision : MonoBehaviour
{
    public LayerMask m_layer;

    private Ray ray;
    private RaycastHit hit;

    private Vector3 m_position;
    Vector3 m_direction;
    private float m_remainingDistance;

    private Vector3 velocity;

    private float speed = 10f;

    private Queue<Vector3> prevPositions;

    [SerializeField, Range(0, 90)]
    private float m_bounceAngle = 30f;

    [SerializeField]
    private int maxBounces;

    private int currentBounces;

    [SerializeField]
    private bool drawPath;

    [SerializeField, Range(1, 10)]
    private int pathPositionCount;

    public event Action<RaycastHit, SolidMonoBehaviour> OnHit;

    private void Start()
    {
        prevPositions = new Queue<Vector3>();
        currentBounces = maxBounces;
    }

    void Update()
    {
        m_position = transform.position;
        m_direction = velocity.normalized;
        m_remainingDistance = velocity.magnitude * Time.fixedDeltaTime;

        Tick(ref m_position, ref m_direction, ref m_remainingDistance, ref currentBounces);

        transform.forward = m_direction;
        transform.position = m_position;
        velocity = m_direction * speed;

        if (drawPath)
        {
            while (prevPositions.Count > pathPositionCount)
            {
                prevPositions.Dequeue();
            }
            prevPositions.Enqueue(transform.position);
        }

        Debug.DrawLine(transform.position, transform.position + velocity * Time.fixedDeltaTime, Color.blue);
    }

    private void OnDrawGizmosSelected()
    {
        if (drawPath && Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Vector3[] p = prevPositions.ToArray();
            for (int i = 0; i < p.Length - 1; i++)
            {
                Gizmos.DrawLine(p[i], p[i + 1]);
            }
        }
    }

    void Tick(ref Vector3 position, ref Vector3 direction, ref float remainingDistance, ref int remainingBounces)
    {
        ray.origin = position;
        ray.direction = direction;

        Physics.Raycast(ray, out hit, remainingDistance, m_layer);

        //Debug.DrawRay(ray.origin, ray.direction, Color.cyan);

        if (hit.transform != null)
        {
            SolidMonoBehaviour hitObject = hit.collider.GetComponent<SolidMonoBehaviour>();

            if (hitObject && OnHit != null)
            {
                OnHit.Invoke(hit, hitObject);
                gameObject.SetActive(false);
            }

            float angle = Vector3.Angle(-direction, hit.normal);
            //Debug.Log(angle);

            if (remainingBounces <= 0 || angle < 90 - m_bounceAngle)
            {
                gameObject.SetActive(false);
                return;
            }
            position = hit.point;
            direction = Vector3.Reflect(direction, hit.normal).normalized;
            remainingDistance -= hit.distance;

            prevPositions.Enqueue(position);

            //Debug.DrawLine(position, position + (direction * remainingDistance));
            remainingBounces -= 1;
            Tick(ref position, ref direction, ref remainingDistance, ref remainingBounces);
        }
        else
        {
            position += direction * remainingDistance;
        }
    }

    public void Init(Vector3 direction, float magnitude)
    {
        velocity = direction * magnitude;
        speed = magnitude;
    }
}
