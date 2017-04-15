using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class WorldSpaceHealthBar : MonoBehaviour
{
    [SerializeField]
    private Gradient m_gradient;

    [SerializeField]
    private Image m_fillImage;

    [SerializeField]
    private Slider m_slider;

    [SerializeField]
    private RectTransform m_rectTransform;

    [SerializeField]
    private HealthBehaviour m_health;

    [SerializeField]
    private Vector3 m_offset;

    [SerializeField]
    private RectTransform m_canvasRect;

    //TODO: MAKE THIS SCREEN SPACE NOT WORLD SPACE

    public void Start()
    {
        m_health.OnValueChange += UpdateValue;
        m_health.OnDeath += Disable;

        UpdateValue(m_health.PercentHealth);
        gameObject.name = m_health.name + "_HealthBar";

    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }

    public void Update()
    {
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, m_health.transform.position + m_offset);
        m_rectTransform.anchoredPosition = screenPos - m_canvasRect.sizeDelta / 2f;
    }

    public void UpdateValue(float percentage)
    {
        m_slider.value = percentage;
        m_fillImage.color = m_gradient.Evaluate(percentage / 100f);
    }
}