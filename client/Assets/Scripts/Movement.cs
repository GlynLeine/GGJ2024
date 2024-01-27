using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private float m_moveMultiplier = 100.0f;
    private bool m_grounded = false;
    private bool m_crouching = false;
    private Rigidbody m_rb;
    private Vector3 m_normalVector = Vector3.up;
    private Vector3 m_wallNormalVector = Vector3.zero;
    private Vector3 m_jumpVelocity = Vector3.zero;

    [Header("Assignable")]
    [SerializeField] private Transform m_orientation;

    [Header("Movement")]
    [SerializeField] private float m_moveSpeed = 20f;
    [SerializeField] private float m_maxSpeed = 50f;
    [SerializeField] private LayerMask m_whatIsGround;
    [SerializeField] private float m_maxSlopeAngle = 35f;

    [Header("Jumping")]
    [SerializeField] private float m_jumpForce = 100f;

    private void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
    }
    void Start()
    {

    }

    void FixedUpdate()
    {
        move();
    }

    private void move()
    {
        //Find actual velocity relative to where player is looking
        Vector2 mag = findVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        float xInput = Input.GetAxis("Horizontal");
        float yInput = Input.GetAxis("Vertical");

        if (xInput > 0 && xMag > m_maxSpeed) xInput = 0;
        if (xInput < 0 && xMag < -m_maxSpeed) xInput = 0;
        if (yInput > 0 && yMag > m_maxSpeed) yInput = 0;
        if (yInput < 0 && yMag < -m_maxSpeed) yInput = 0;

        var forward = m_orientation.forward * yInput;
        var right = m_orientation.right * xInput;

        var moveDir = (forward + right).normalized;

        if (m_crouching)
            m_rb.velocity = moveDir * (m_moveSpeed / 2f) * m_moveMultiplier * Time.deltaTime;
        else
            m_rb.velocity = moveDir * m_moveSpeed * m_moveMultiplier * Time.deltaTime;
    }

    private void jump()
    {

    }

    /// <summary>
    /// Find the velocity relative to where the player is looking
    /// Useful for vectors calculations regarding movement and limiting movement
    /// </summary>
    /// <returns></returns>
    private Vector2 findVelRelativeToLook()
    {
        float lookAngle = m_orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(m_rb.velocity.x, m_rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = m_rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    private bool isFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < m_maxSlopeAngle;
    }
}
