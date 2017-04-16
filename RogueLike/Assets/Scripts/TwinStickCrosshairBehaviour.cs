using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TwinStickCrosshairBehaviour : MonoBehaviour
{
    public enum CrosshairType
    {
        FREE,
        NORMALIZED
    }

    [SerializeField]
    private CrosshairType m_type;

    [SerializeField]
    private RectTransform m_canvasRect;

    [SerializeField]
    private RectTransform m_crosshairRect;

    private Vector2 m_direction;

    [SerializeField]
    private RangedWeaponBehaviour m_weapon;

    [SerializeField]
    private float m_length;

    public void SetPosition(Vector2 inputPosition)
    {
        m_direction = inputPosition.magnitude > 0.1f ? inputPosition : m_direction;

        switch (m_type)
        {
            case CrosshairType.FREE:
                {
                    Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPosition);
                    Vector2 position = (screenPos - m_canvasRect.sizeDelta / 2f);

                    m_crosshairRect.anchoredPosition = new Vector2(Mathf.Clamp(position.x, 0 - Screen.width / 2f, Screen.width / 2f), Mathf.Clamp(position.y, 0 - Screen.height / 2f, Screen.height / 2f));
                    break;
                }
            case CrosshairType.NORMALIZED:
                {
                    RaycastHit hit = m_weapon.ForwardRaycastHit(m_length);
                    Vector3 worldPosition = hit.transform != null ? hit.point : m_weapon.WorldOrigin + (m_weapon.transform.forward * m_length);
                    Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPosition);
                    Vector2 position = (screenPos - m_canvasRect.sizeDelta / 2f);

                    m_crosshairRect.anchoredPosition = new Vector2(Mathf.Clamp(position.x, 0 - Screen.width / 2f, Screen.width / 2f), Mathf.Clamp(position.y, 0 - Screen.height / 2f, Screen.height / 2f));
                    break;
                }
        }
    }
}