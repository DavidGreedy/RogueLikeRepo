using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwinStickCrosshairBehaviour : MonoBehaviour
{
    public enum CrosshairType
    {
        NORMALIZED,
        UNCLAMPED
    }

    [SerializeField]
    private CrosshairType m_type;

    [SerializeField]
    private RectTransform m_canvasRect;

    [SerializeField]
    private RectTransform m_crosshairRect;

    private Vector2 m_direction;

    private Vector2 m_screenPosition = Vector2.zero;

    //[SerializeField]
    public RangedWeaponBehaviour m_weapon;

    [SerializeField]
    private float m_length;

    public void SetPosition(Vector2 inputPosition)
    {
        m_direction = inputPosition.magnitude > 0.1f ? inputPosition : m_direction;
        //Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, m_weapon.transform.position);

        switch (m_type)
        {
            case CrosshairType.NORMALIZED:
                {
                    Vector3 worldPosition = m_weapon.ForwardRaycastHit(m_length).point;
                    Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPosition);
                    Vector2 position = (screenPos - m_canvasRect.sizeDelta / 2f);
                    //Vector2 position = Camera.main.WorldToScreenPoint(worldPosition);
                    //m_crosshairRect.anchoredPosition = position - new Vector2(Screen.width, Screen.height);

                    m_crosshairRect.anchoredPosition = new Vector2(Mathf.Clamp(position.x, 0 - Screen.width / 2f, Screen.width / 2f), Mathf.Clamp(position.y, 0 - Screen.height / 2f, Screen.height / 2f));
                    break;
                }
                //case CrosshairType.UNCLAMPED:
                //    {
                //        m_screenPosition += screenPos.normalized * Time.deltaTime * m_length;
                //        m_crosshairRect.anchoredPosition = m_screenPosition;
                //        break;
                //    {
        }
    }
}