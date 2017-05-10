using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class TopDownCamera : MonoBehaviour
{
    [SerializeField]
    private Transform m_targetTransform;

    [SerializeField]
    private float m_targetDistance;

    [SerializeField]
    private float m_viewAngle;

    private float m_currentAngle;

    private float adjacent,opposite;

    private Camera m_camera;

    private void Start()
    {
        m_camera = GetComponent<Camera>();


    }

    private void Update()
    {
        if (m_targetTransform)
        {
            if (m_camera.orthographic)
            {
                m_camera.orthographicSize = m_targetDistance;
                adjacent = Mathf.Cos(m_viewAngle * Mathf.Deg2Rad) * 10f;
                opposite = Mathf.Sin(m_viewAngle * Mathf.Deg2Rad) * 10f;
            }
            else
            {
                adjacent = Mathf.Cos(m_viewAngle * Mathf.Deg2Rad) * m_targetDistance;
                opposite = Mathf.Sin(m_viewAngle * Mathf.Deg2Rad) * m_targetDistance;

            }

            Rotate(10 * Time.deltaTime);
            transform.position = m_targetTransform.position + (Quaternion.AngleAxis(m_currentAngle, Vector3.up) * new Vector3(0, opposite, -adjacent));


            //transform.rotation = Quaternion.AngleAxis(m_viewAngle, transform.right) * Quaternion.AngleAxis(m_currentAngle, Vector3.up);
            transform.LookAt(m_targetTransform);
        }
    }

    public void SetTarget(Transform target)
    {
        m_targetTransform = target;
        transform.Translate(0, target.position.y + adjacent, 0);
    }

    public void Rotate(float angle)
    {
        m_currentAngle += angle;
    }
}