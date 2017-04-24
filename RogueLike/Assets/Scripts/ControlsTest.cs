using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsTest : MonoBehaviour
{
    [SerializeField]
    private ControlSettings controlSettings;

    void Update()
    {
        if (controlSettings.UpArrow)
        {
            print("UpArrow");
        }
        if (controlSettings.DownArrow)
        {
            print("DownArrow");
        }
        if (controlSettings.LeftArrow)
        {
            print("LeftArrow");
        }
        if (controlSettings.RightArrow)
        {
            print("RightArrow");
        }
    }
}