using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamepadInput;

public class Fire : MonoBehaviour {

    public Light light;
    public bool isLit = true;
    private float m_intesity;
    private float m_Strength;
    public GameObject fireObj;
    private float m_time = 0.2f;
    // Use this for initialization
    void Start ()
    {
        LightFire();
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (Input.GetKeyDown(KeyCode.F) && !isLit)
        {
            LightFire();
        }
        else if(Input.GetKeyDown(KeyCode.F) && isLit)
        {
            ExtinguishFire();
        }
    }

    void LightFire()
    {
        fireObj.SetActive(true);
        isLit = true;
        light.enabled = true;
        InvokeRepeating("StartFire", .1f, m_time);
    }

    void ExtinguishFire()
    {
        CancelInvoke();
        fireObj.SetActive(false);
        light.enabled = false;
        isLit = false;
    }

    void StartFire()
    {
        m_time = Random.Range(0.1f, 0.5f);
        if (isLit)
        {
            m_intesity = Random.Range(0.8f, 2f);
            m_Strength = Random.Range(0.5f, 1f);
            light.intensity = Mathf.Lerp(light.intensity, m_intesity, m_time * Time.time);
            light.range = Mathf.Lerp(light.range, m_intesity * 7, m_time * Time.time);
            light.shadowStrength = Mathf.Lerp(light.shadowStrength, m_Strength, m_time * Time.time);
        }
    }
}
