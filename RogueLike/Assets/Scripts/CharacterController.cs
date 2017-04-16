using GamepadInput;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [SerializeField]
    private Camera m_camera;
    [SerializeField]
    private Rigidbody m_rigidbody;

    [SerializeField]
    private CharacterBehaviour m_character;

    [SerializeField]
    private Vector2 inputDirL;

    [SerializeField]
    private Vector2 inputDirR;

    [SerializeField]
    private GamePad.Index m_gamepadIndex;

    [SerializeField]
    private TwinStickCrosshairBehaviour m_crosshair;

    private void Start()
    {
        gameObject.name = string.Format("Player {0}", m_gamepadIndex);
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position - transform.right, inputDirL.normalized);
            Gizmos.DrawRay(transform.position + transform.right, inputDirR.normalized);
        }
    }

    void Update()
    {
        inputDirL = GamePad.GetAxis(GamePad.Axis.LeftStick, m_gamepadIndex);
        inputDirR = GamePad.GetAxis(GamePad.Axis.RightStick, m_gamepadIndex);

        Vector3 moveDir = m_camera.transform.TransformDirection(new Vector3(inputDirL.x, 0, inputDirL.y));
        Vector3 lookDir = m_camera.transform.TransformDirection(new Vector3(inputDirR.x, 0, inputDirR.y));

        lookDir.y = 0;
        m_character.SetLookDirection(lookDir);

        moveDir.y = 0;
        moveDir.Normalize();

        m_rigidbody.velocity = m_character.Move(moveDir, m_rigidbody.velocity, 10.0f, 50f);

        if (m_character.ActiveWeapon)
        {
            m_crosshair.SetPosition(inputDirR.normalized);
        }

        if (GamePad.GetButton(GamePad.Button.RightShoulder, m_gamepadIndex))
        {
            if (m_character.ActiveWeapon)
            {
                m_character.ActiveWeapon.UsePrimary();
            }
        }
    }
}
