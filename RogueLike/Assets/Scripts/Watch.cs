﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Watch : MonoBehaviour {

    public DayNightBehaviour dayNight;
    public HealthBehaviour health;
    [SerializeField]
    private bool watchActive;
    private Animator anim;

    public Image healthImage, thirstImage, hungerImage;

    public Text time;

	// Use this for initialization
	void Start ()
    {
        anim = GetComponent<Animator>();

	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            watchActive = !watchActive;
            if (watchActive)
            {
                anim.SetBool("Active", true);
            }
            else
            {
                anim.SetBool("Active", false);
            }
        }
        time.text = dayNight.GetTimeOfDay();
        healthImage.fillAmount = health.PercentHealth;

	}
}
