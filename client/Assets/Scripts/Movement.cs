using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    private Vector2 m_movementInput = Vector2.zero;
    private float m_moveMultiplier = 100.0f;
    private float m_counterMovement = 0.175f;
    private float m_threshold = 0.01f;

    private bool m_grounded = false;
    private bool m_jumping = false;
    private bool m_crouching = false;
    private bool m_running = false;
    private bool m_cancellingGrounded = false;

    private bool m_isMoving = false;

    private Rigidbody m_rb;
    private Vector3 m_normalVector = Vector3.up;
    private Vector3 m_wallNormalVector = Vector3.zero;
    private Vector3 m_jumpVelocity = Vector3.zero;

    [Header("Assignable")]
    [SerializeField] private Transform m_camera;

    [Header("Movement")]
    [SerializeField] private float m_moveSpeed = 20f;
    [SerializeField] private float m_maxSpeed = 50f;
    [SerializeField] private LayerMask m_whatIsGround;
    [SerializeField] private float m_maxSlopeAngle = 35f;
    [SerializeField] private float m_deceleration_rate = .9f;

    [Header("Jumping")]
    [SerializeField] private float m_jumpCooldown = 0.25f;
    [SerializeField] private float m_jumpForce = 100f;

    [Header("Crouch & Slide")]
    [SerializeField] private float m_crouchHeight = .5f;
    [SerializeField] private float m_slideForce = 40;
    [SerializeField] private float m_slideCounterMovement = 0.2f;

    private void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
    }

    public void OnCrouch(InputValue value)
    {
        Debug.Log("Crouch " + value.isPressed.ToString());

        m_crouching = value.isPressed;

        if (m_crouching)
        {
            m_camera.position = new Vector3(m_camera.position.x, m_camera.position.y - 0.5f, m_camera.position.z); if (m_rb.velocity.magnitude > 0.5f)
                if (m_grounded && m_rb.velocity.magnitude > 0.5f)
                {
                    m_rb.AddForce(transform.forward * m_slideForce);
                }
        }
        else
            m_camera.position = new Vector3(m_camera.position.x, m_camera.position.y + 0.5f, m_camera.position.z);

    }

    public void OnRun(InputValue value)
    {
        Debug.Log("Run " + value.isPressed.ToString());
        m_running = value.isPressed;
    }

    void FixedUpdate()
    {
        Move();
        
    }

    public void OnMove(InputValue value)
    {
        Vector2 inputValue = value.Get<Vector2>();
        Debug.Log("Move " + inputValue.ToString());
        m_movementInput = inputValue;
        m_isMoving = inputValue.x != 0 || inputValue.y != 0;
    }

    private void Move()
    {
        Vector2 input = m_movementInput;
        if(!m_grounded)
        {
            input = Vector2.zero;
        }
        Vector2 mag = findVelRelativeToLook();

        //counterMovement(m_movementInput, mag);

        Vector2 movement = Vector2.Max(Vector2.Min(mag + m_movementInput, new Vector2(m_maxSpeed, m_maxSpeed)), new Vector2(-m_maxSpeed, -m_maxSpeed)) - mag;

        m_rb.AddForce((transform.forward * movement.y + transform.right * movement.x) * m_moveSpeed * Time.fixedDeltaTime * m_moveMultiplier, ForceMode.Acceleration);
        if(m_rb.velocity.magnitude > m_maxSpeed)
        {
            m_rb.velocity = m_rb.velocity.normalized * m_maxSpeed;
        }
        if (!m_isMoving && m_grounded && !m_crouching)
        {
            m_rb.velocity *= m_deceleration_rate;
        }

    }

    public void OnJump()
    {
        if (!m_grounded)
            return;

        m_grounded = false;

        m_rb.AddForce((Vector3.up * 1.5f + m_normalVector * 0.5f) * m_jumpForce);
    }
    private void counterMovement(Vector2 movementInput, Vector2 mag)
    {
        //Slow down sliding
        if (m_crouching)
        {
            m_rb.velocity = m_moveSpeed * Time.deltaTime * -m_rb.velocity.normalized * m_slideCounterMovement;
            //rb.AddForce(moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideCounterMovement);
            return;
        }

        //Counter movement
        if (Mathf.Abs(mag.x) > m_threshold && Mathf.Abs(movementInput.x) < 0.05f || (mag.x < -m_threshold && movementInput.x > 0) || (mag.x > m_threshold && movementInput.x < 0))
        {
            m_rb.AddForce(m_moveSpeed * transform.right * Time.deltaTime * -mag.x * m_counterMovement);
        }
        if (Mathf.Abs(mag.y) > m_threshold && Mathf.Abs(movementInput.y) < 0.05f || (mag.y < -m_threshold && movementInput.y > 0) || (mag.y > m_threshold && movementInput.y < 0))
        {
            m_rb.AddForce(m_moveSpeed * transform.forward * Time.deltaTime * -mag.y * m_counterMovement);
        }

        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(m_rb.velocity.x, 2) + Mathf.Pow(m_rb.velocity.z, 2))) > m_maxSpeed)
        {
            float fallspeed = m_rb.velocity.y;
            Vector3 n = m_rb.velocity.normalized * m_maxSpeed;
            m_rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    private void stopGrounded()
    {
        m_grounded = false;
    }

    /// <summary>
    /// Handle ground detection
    /// </summary>
    private void OnCollisionStay(Collision other)
    {
        //Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (m_whatIsGround != (m_whatIsGround | (1 << layer))) return;

        Vector3 sumNormals = new Vector3(0, 0, 0);
        //Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
            //FLOOR
            if (isFloor(normal))
            {
                m_grounded = true;
                m_cancellingGrounded = false;
                sumNormals += normal;
                CancelInvoke(nameof(stopGrounded));
            }
        }

        m_normalVector = sumNormals.normalized;

        //Invoke ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 3f;
        if (!m_cancellingGrounded)
        {
            m_cancellingGrounded = true;
            Invoke(nameof(stopGrounded), Time.deltaTime * delay);
        }
    }

    /// <summary>
    /// Find the velocity relative to where the player is looking
    /// Useful for vectors calculations regarding movement and limiting movement
    /// </summary>
    /// <returns></returns>
    private Vector2 findVelRelativeToLook()
    {
        float lookAngle = transform.eulerAngles.y;
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
