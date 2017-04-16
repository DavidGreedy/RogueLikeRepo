using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightBehaviour : MonoBehaviour
{
    public GameObject m_sunLight;

    public float m_dayCycle; // Time in seconds

    public int m_currentDay;

    public Vector3 m_axis;

    private float m_daySpeed;

    [SerializeField]
    private float startTime;

    [SerializeField, Range(0f, 1f)]
    private float m_dayNight;

    [SerializeField]
    private float time;

    [SerializeField]
    private bool isDay;

    [SerializeField]
    private string timeString;

    [SerializeField]
    private float timeFloat;

    void Start()
    {
        m_currentDay = 0;
        isDay = true;
    }

    public string GetTimeOfDay()
    {
        TimeSpan t = TimeSpan.FromSeconds(time * 60f * 60f * 24f);
        return string.Format("{0:D2}:{1:D2}", t.Hours, t.Minutes);
    }

    void Update()
    {
        time += Time.deltaTime / m_dayCycle;
        isDay = time < m_dayNight;

        timeFloat = isDay ? time / m_dayNight : (time - m_dayNight) / (1.0f - m_dayNight);
        m_sunLight.transform.rotation = Quaternion.AngleAxis(isDay ? Mathf.Lerp(0, 180, time / m_dayNight) : Mathf.Lerp(180, 360, (time - m_dayNight) / (1.0f - m_dayNight)), m_axis.normalized);
        if (time > 1.0f)
        {
            m_currentDay++;
            time = 0;
        }
        timeString = GetTimeOfDay();
    }
}
