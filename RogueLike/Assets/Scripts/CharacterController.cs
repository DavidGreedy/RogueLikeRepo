using GamepadInput;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [SerializeField]
    private Camera m_camera;

    [SerializeField]
    private CharacterBehaviour m_character;

    [SerializeField]
    private Vector2 inputDirL;

    [SerializeField]
    private Vector2 inputDirR;

    [SerializeField]
    private GamePad.Index m_gamepadIndex;

    private void Start()
    {
        gameObject.name = string.Format("Player {0}", m_gamepadIndex);
    }

    void Update()
    {
        inputDirL = GamePad.GetAxis(GamePad.Axis.LeftStick, m_gamepadIndex);
        inputDirR = GamePad.GetAxis(GamePad.Axis.RightStick, m_gamepadIndex);

        Vector3 moveDir = m_camera.transform.TransformDirection(new Vector3(inputDirL.x, 0, inputDirL.y));
        Vector3 lookDir = m_camera.transform.TransformDirection(new Vector3(inputDirR.x, 0, inputDirR.y));

        lookDir.y = 0;
        moveDir.y = 0;

        m_character.MoveVector = moveDir.normalized * inputDirL.magnitude;
        m_character.LookVector = lookDir.normalized * inputDirR.magnitude;

        if (GamePad.GetButtonDown(GamePad.Button.RightShoulder, m_gamepadIndex))
        {
            m_character.Attack();
        }
    }
}