using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightBehaviour : MonoBehaviour
{
    public Light m_sunLight;

    public float m_dayLength; // Time in seconds

    public int m_currentDay;

    public Vector3 m_axis;

    private float m_daySpeed;

    private float startTime;

    [SerializeField, Range(0f, 1f)]
    private float m_dayNight;

    [SerializeField]
    private float time;

    [SerializeField]
    private bool isDay;

    [SerializeField]
    private string s;

    [SerializeField]
    private float f;

    void Start()
    {
        m_currentDay = 0;
        isDay = true;
    }

    private string GetTimeOfDay()
    {
        TimeSpan t = TimeSpan.FromSeconds(time * 60f * 60f * 24f);
        return string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds);
    }

    void Update()
    {
        time += Time.deltaTime / m_dayLength;
        isDay = time < m_dayNight;

        f = isDay ? time / m_dayNight : (time - m_dayNight) / (1.0f - m_dayNight);
        m_sunLight.transform.rotation = Quaternion.AngleAxis(isDay ? Mathf.Lerp(0, 180, time / m_dayNight) : Mathf.Lerp(180, 360, (time - m_dayNight) / (1.0f - m_dayNight)), m_axis.normalized);
        if (time > 1.0f)
        {
            m_currentDay++;
            time = 0;
        }
        s = GetTimeOfDay();
    }
}
