using UnityEngine;

public class CameraController : MonoBehaviour
{
	[SerializeField] private Transform m_Player;
	[SerializeField] private float m_LookSensitivity = 1f;
	[SerializeField] private float m_CameraVerticalFollowOffset = 0.25f;

	private void Start()
	{
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	private void LateUpdate()
	{
		Vector2 input = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * 
		                (Time.fixedDeltaTime * m_LookSensitivity);
        
		// Horizontal rotation
		transform.localRotation *= Quaternion.AngleAxis(input.x, Vector3.up);

		// Vertical rotation
		transform.localRotation *= Quaternion.AngleAxis(-input.y, Vector3.right);
        
		Vector3 angles = transform.localEulerAngles;
		angles.z = 0;

		float angle = transform.localEulerAngles.x;

		if (angle > 180f && angle < 360f)
		{
			angles.x = 360f;
		}
		else if (angle < 180f && angle > 40f)
		{
			angles.x = 40f;
		}

		transform.localEulerAngles = angles;
	}

	private void FixedUpdate()
    {
	    if (m_Player != null)
			transform.position = m_Player.position + Vector3.up * m_CameraVerticalFollowOffset;
    }
}
