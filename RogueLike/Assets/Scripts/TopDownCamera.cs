using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownCamera : MonoBehaviour
{
    //[SerializeField]
    //private Transform m_lookTargetTransform;

    //[SerializeField]
    //private Transform m_moveTargetTransform;

    //[SerializeField]
    //private LayerMask m_collisionLayer;

    //[SerializeField]
    //private Vector2 m_rotationSpeed = new Vector2(1.0f, 1.0f);

    //[SerializeField]
    //private float m_maxDistance = 10.0f;

    [SerializeField]
    private List<Transform> m_targetTransforms;

    [SerializeField]
    private Vector3 m_offset;

    private void Start()
    {
        m_offset = transform.position;
    }

    private void Update()
    {
        Vector3 averagePosition = Vector3.zero;

        for (int i = 0; i < m_targetTransforms.Count; i++)
        {
            averagePosition += m_targetTransforms[i].position;
        }

        if (m_targetTransforms.Count > 0)
        {
            averagePosition /= m_targetTransforms.Count;
            transform.position = averagePosition + m_offset;
        }
    }

    public void AddTarget(Transform target)
    {
        m_targetTransforms.Add(target);
    }

    //void Update()
    //{
    //    transform.position = m_moveTargetTransform.position;
    //    transform.LookAt(m_lookTargetTransform);
    //}

    //public void M(Vector2 inputVector)
    //{
    //    Vector3 checkDir = (m_moveTargetTransform.position - m_lookTargetTransform.position).normalized;
    //    Ray looktargetToMoveTargetRay = new Ray(m_lookTargetTransform.position, checkDir);

    //    RaycastHit obstacleHit;

    //    Physics.Raycast(looktargetToMoveTargetRay, out obstacleHit, m_maxDistance, m_collisionLayer.value);

    //    if (obstacleHit.transform != null && Vector3.Distance(obstacleHit.point, m_lookTargetTransform.position) < m_maxDistance)
    //    {
    //        Debug.DrawLine(m_lookTargetTransform.position, m_moveTargetTransform.position, Color.red);
    //        m_moveTargetTransform.transform.position = obstacleHit.point;
    //    }
    //    else
    //    {
    //        Debug.DrawLine(m_lookTargetTransform.position, m_moveTargetTransform.position, Color.green);
    //        m_moveTargetTransform.transform.position = m_lookTargetTransform.transform.position + (checkDir * m_maxDistance);
    //    }

    //    m_moveTargetTransform.transform.RotateAround(m_lookTargetTransform.position, Vector3.up, inputVector.x * m_rotationSpeed.x * Time.deltaTime); // X Rot

    //    m_moveTargetTransform.transform.RotateAround(m_lookTargetTransform.position, Vector3.Cross(m_lookTargetTransform.position - m_moveTargetTransform.position, Vector3.up), inputVector.y * m_rotationSpeed.y * Time.deltaTime); // Y Rot

    //    //TODO: Clamp rotation so you can invert the y
    //}
}