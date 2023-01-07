using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float m_Speed = 2;
    
    private Vector2 m_Input;

    private void Update()
    {
        m_Input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }
    
    private void FixedUpdate()
    {
        transform.position += new Vector3(m_Input.x, 0, m_Input.y) * m_Speed;
    }
}
