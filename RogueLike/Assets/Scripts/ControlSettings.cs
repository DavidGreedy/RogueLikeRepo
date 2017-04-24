using GamepadInput;
using UnityEngine;

[CreateAssetMenu(fileName = "ControlSettings", menuName = "ScriptableObject/ControlSettings", order = 0)]
public class ControlSettings : ScriptableObject
{
    public GamePad.Index index;
    public GamePad.Button roll;
    public GamePad.Button jump;
    public GamePad.Button useWeapon;
    public GamePad.Button swapWeapon;
    public GamePad.Button switchItem;
    public GamePad.Button useItem;

    public GamePad.Axis move;
    public GamePad.Axis look;

    public bool UpArrow { get { return GamePad.GetAxis(GamePad.Axis.Dpad, index, true).y == 1; } }
    public bool DownArrow { get { return GamePad.GetAxis(GamePad.Axis.Dpad, index, true).y == -1; } }
    public bool LeftArrow { get { return GamePad.GetAxis(GamePad.Axis.Dpad, index, true).x == 1; } }
    public bool RightArrow { get { return GamePad.GetAxis(GamePad.Axis.Dpad, index, true).x == -1; } }
}