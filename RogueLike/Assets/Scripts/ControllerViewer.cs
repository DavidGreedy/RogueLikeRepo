using GamepadInput;
using UnityEngine;
using UnityEngine.UI;

public class ControllerViewer : MonoBehaviour
{
    [SerializeField]
    GamePad.Index targetController;

    [SerializeField]
    LineRenderer lStick, rStick;

    [SerializeField]
    private Text[] info;

    Vector3[] lPositions, rPositions;

    void Start()
    {
        lPositions = new Vector3[2];
        rPositions = new Vector3[2];

        lPositions[0] = new Vector3(-0.5f, 0, 0);
        rPositions[0] = new Vector3(0.5f, 0, 0);
    }

    void Update()
    {
        GamepadState gamepadState = GamePad.GetState(targetController, true);

        SetText(0, string.Format("A {0}", gamepadState.A), gamepadState.A);
        SetText(1, string.Format("B {0}", gamepadState.B), gamepadState.B);
        SetText(2, string.Format("X {0}", gamepadState.X), gamepadState.X);
        SetText(3, string.Format("Y {0}", gamepadState.Y), gamepadState.Y);
        SetText(4, string.Format("UP {0}", gamepadState.Up), gamepadState.Up);
        SetText(5, string.Format("DOWN {0}", gamepadState.Down), gamepadState.Down);
        SetText(6, string.Format("LEFT {0}", gamepadState.Left), gamepadState.Left);
        SetText(7, string.Format("RIGHT {0}", gamepadState.Right), gamepadState.Right);
        SetText(8, string.Format("LB {0}", gamepadState.LeftShoulder), gamepadState.LeftShoulder);
        SetText(9, string.Format("RB {0}", gamepadState.RightShoulder), gamepadState.RightShoulder);
        SetText(10, string.Format("Start {0}", gamepadState.Start), gamepadState.Start);
        SetText(11, string.Format("Back {0}", gamepadState.Back), gamepadState.Back);
        SetText(12, string.Format("Start {0}", gamepadState.Start), gamepadState.Start);
        SetText(13, string.Format("Back {0}", gamepadState.Back), gamepadState.Back);

        lPositions[1] = lPositions[0] + (Vector3)gamepadState.LeftStickAxis.normalized * 0.1f;
        lStick.SetPositions(lPositions);

        rPositions[1] = rPositions[0] + (Vector3)gamepadState.RightStickAxis.normalized * 0.1f;
        rStick.SetPositions(rPositions);

        Debug.DrawRay(new Vector3(-0.5f, 0, 0), gamepadState.LeftStickAxis.normalized * 0.1f);
        Debug.DrawRay(new Vector3(0.5f, 0, 0), gamepadState.RightStickAxis.normalized * 0.1f);
    }

    void SetText(int index, string text, bool state)
    {
        info[index].text = text;
        info[index].color = state ? Color.green : Color.white;
    }
}