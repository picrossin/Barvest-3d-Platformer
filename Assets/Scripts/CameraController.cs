using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
	[SerializeField] private Transform m_Player;
	[SerializeField] private float m_LookSensitivity = 1f;
	[SerializeField] private float m_CameraVerticalFollowOffset = 0.25f;
	[SerializeField] private float m_StartScreenSpinSpeed = 2f;

	private bool m_InvertX;
	private bool m_InvertY;
	
	public Transform FollowObject { get; set; }
	
	private void LateUpdate()
	{
		if (GameplayManager.Instance.BookOpen)
			return;
		
		Vector2 input = new Vector2((m_InvertX ? -1f : 1f) * Input.GetAxis("Mouse X"), (m_InvertY ? -1f : 1f) * Input.GetAxis("Mouse Y")) * 
		                (Time.fixedDeltaTime * m_LookSensitivity);

		if (!GameplayManager.Instance.Started)
		{
			input = new Vector2(1f, 0f) * (Time.deltaTime * m_StartScreenSpinSpeed);
		}

		Vector3 angles = transform.localEulerAngles;
		
		if (FollowObject == null)
		{
			// Horizontal rotation
			transform.localRotation *= Quaternion.AngleAxis(input.x, Vector3.up);

			// Vertical rotation
			transform.localRotation *= Quaternion.AngleAxis(-input.y, Vector3.right);
        
			angles = transform.localEulerAngles;
			angles.z = 0;

			float angle = transform.localEulerAngles.x;

			if (angle > 180f && angle < 360f)
			{
				angles.x = 360f;
			}
			else if (angle < 180f && angle > 65f)
			{
				angles.x = 65f;
			}
		}
		else
		{
			// Lock horizontal rotation to spider orientation about object
			transform.localRotation = quaternion.LookRotation((FollowObject.position - m_Player.position).normalized, Vector3.up);
			
			// Lock vertical rotation
			angles = transform.localEulerAngles;
			angles.z = 0;
			angles.x = 365f;
		}

		transform.localEulerAngles = angles;
	}

	private void FixedUpdate()
    {
	    if (GameplayManager.Instance.BookOpen)
		    return;
	    
	    if (m_Player != null && FollowObject == null)
	    {
		    transform.position = m_Player.position + Vector3.up * m_CameraVerticalFollowOffset;
	    }
	    else if (m_Player != null && FollowObject != null)
	    {
		    transform.position = FollowObject.position;
	    }
    }

	public void SetCameraSensitivity(Slider sensitivity)
	{
		Debug.Log($"Sensitivity: {sensitivity.value}");
		m_LookSensitivity = sensitivity.value;
	}

	public void InvertX(Toggle val)
	{
		m_InvertX = val.isOn;
	}
	
	public void InvertY(Toggle val)
	{
		m_InvertY = val.isOn;
	}
}
