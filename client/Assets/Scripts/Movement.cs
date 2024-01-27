using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    private float m_xInput = 0;
    private float m_yInput = 0;
    private float m_moveMultiplier = 100.0f;
    private float m_counterMovement = 0.175f;
    private float m_threshold = 0.01f;

    private bool m_grounded = false;
    private bool m_readyToJump = false;
    private bool m_jumping = false;
    private bool m_crouching = false;
    private bool m_running = false;
    private bool m_cancellingGrounded = false;

    private Rigidbody m_rb;
    private Vector3 m_normalVector = Vector3.up;
    private Vector3 m_wallNormalVector = Vector3.zero;
    private Vector3 m_jumpVelocity = Vector3.zero;
    private Vector3 m_originalPos;

    [Header("Assignable")]
    [SerializeField] private Transform m_orientation;
    [SerializeField] private Transform m_camera;

    [Header("Movement")]
    [SerializeField] private float m_moveSpeed = 20f;
    [SerializeField] private float m_maxSpeed = 50f;
    [SerializeField] private LayerMask m_whatIsGround;
    [SerializeField] private float m_maxSlopeAngle = 35f;


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
        m_originalPos = m_camera.position;
    }
    void Start()
    {

    }

    void Update()
    {
        var keyboard = Keyboard.current;
        m_xInput = -keyboard.aKey.ReadValue() + keyboard.dKey.ReadValue();
        m_yInput = -keyboard.sKey.ReadValue() + keyboard.wKey.ReadValue();
        m_running = keyboard.leftShiftKey.isPressed;
        m_crouching = keyboard.leftCtrlKey.isPressed;
        m_jumping = keyboard.spaceKey.isPressed;

        if (m_crouching)
        {
            m_camera.position = new Vector3(m_camera.position.x, m_camera.position.y - 0.5f, m_camera.position.z);
            if (m_rb.velocity.magnitude > 0.5f)
            {
                if (m_grounded)
                {
                    m_rb.AddForce(m_orientation.transform.forward * m_slideForce);
                }
            }
        }
        else
            m_camera.position = new Vector3(m_camera.position.x, m_camera.position.y + 0.5f, m_camera.position.z);
    }

    void FixedUpdate()
    {
        move();
    }

    private void move()
    {
        Vector2 mag = findVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        if (m_grounded && !m_jumping)
            counterMovement(m_xInput, m_yInput, mag);

        if (m_readyToJump && m_jumping && m_grounded) jump();

        if (m_xInput > 0 && xMag > m_maxSpeed) m_xInput = 0;
        if (m_xInput < 0 && xMag < -m_maxSpeed) m_xInput = 0;
        if (m_yInput > 0 && yMag > m_maxSpeed) m_yInput = 0;
        if (m_yInput < 0 && yMag < -m_maxSpeed) m_yInput = 0;

        var forward = transform.forward * m_yInput;
        var right = transform.right * m_xInput;

        var moveDir = (forward + right).normalized;

        if (m_crouching)
            m_rb.velocity = moveDir * (m_moveSpeed / 2f) * m_moveMultiplier * Time.deltaTime;
        else
            m_rb.velocity = moveDir * m_moveSpeed * m_moveMultiplier * Time.deltaTime;
    }

    private void jump()
    {
        m_readyToJump = false;

        m_rb.AddForce(m_normalVector * m_jumpForce, ForceMode.Impulse);
        var vel = m_rb.velocity;
        //If jumping while falling, reset y velocity.
        if (m_rb.velocity.y < 0.5f)
            m_rb.velocity = new Vector3(vel.x, 0, vel.z);
        else if (m_rb.velocity.y > 0)
            m_rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);

        Invoke(nameof(resetJump), m_jumpCooldown);
    }
    private void counterMovement(float x, float y, Vector2 mag)
    {
        //Slow down sliding
        if (m_crouching)
        {
            m_rb.velocity = m_moveSpeed * Time.deltaTime * -m_rb.velocity.normalized * m_slideCounterMovement;
            //rb.AddForce(moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideCounterMovement);
            return;
        }

        //Counter movement
        if (Mathf.Abs(mag.x) > m_threshold && Mathf.Abs(x) < 0.05f || (mag.x < -m_threshold && x > 0) || (mag.x > m_threshold && x < 0))
        {
            m_rb.AddForce(m_moveSpeed * m_orientation.transform.right * Time.deltaTime * -mag.x * m_counterMovement);
        }
        if (Mathf.Abs(mag.y) > m_threshold && Mathf.Abs(y) < 0.05f || (mag.y < -m_threshold && y > 0) || (mag.y > m_threshold && y < 0))
        {
            m_rb.AddForce(m_moveSpeed * m_orientation.transform.forward * Time.deltaTime * -mag.y * m_counterMovement);
        }

        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(m_rb.velocity.x, 2) + Mathf.Pow(m_rb.velocity.z, 2))) > m_maxSpeed)
        {
            float fallspeed = m_rb.velocity.y;
            Vector3 n = m_rb.velocity.normalized * m_maxSpeed;
            m_rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    private void resetJump()
    {
        m_readyToJump = true;
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
