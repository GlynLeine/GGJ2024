using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.VisualScripting;

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    [SerializeField] private AudioSource footAudioSource;
    [SerializeField]
    private AnimationCurve m_slowDownCurve;
    private Vector2 m_movementInput = Vector2.zero;
    public Vector2 MoveInput => m_movementInput;
    private float m_moveMultiplier = 100.0f;
    private float m_counterMovement = 0.3f;
    private float m_threshold = 0.01f;

    [Header("Debug Bools")]
    [SerializeField]
    private bool m_grounded = false;
    public bool Grounded { get { return m_grounded; } }
    [SerializeField]
    private bool m_jumping = false;
    [SerializeField]
    private bool m_crouching = false;
    [SerializeField]
    private bool m_running = false;
    [SerializeField]
    private bool m_cancellingGrounded = false;
    [SerializeField]
    private bool m_mouseLocked = true;
    [HideInInspector] public bool m_wallRunning = false;

    private bool m_isMoving = false;

    public static bool MouseLocked => !Cursor.visible && (Cursor.lockState == CursorLockMode.Locked || Cursor.lockState == CursorLockMode.Confined);

    private Rigidbody m_rb;
    private Vector3 m_normalVector = Vector3.up;
    private Vector3 m_wallNormalVector = Vector3.zero;
    private Vector3 m_jumpVelocity = Vector3.zero;

    [Header("Assignable")]
    [SerializeField] private Transform m_camera;

    [Header("Movement")]
    [SerializeField] private float m_moveSpeed = 20f;
    [SerializeField] private float m_maxSpeed = 50f;
    [SerializeField] public LayerMask m_whatIsGround;
    [SerializeField] private float m_maxSlopeAngle = 35f;
    //[SerializeField] private float m_deceleration_rate = .9f;
    [SerializeField] private float m_deccelerationTime = 1.0f;
    private float m_timeSinceLastMove = 0;


    [Header("Jumping")]
    [SerializeField] private float m_jumpCooldown = 0.25f;
    [SerializeField] private float m_jumpForce = 100f;
    [SerializeField] private float m_inAirMoveFactor = .7f;

    [Header("Crouch & Slide")]
    [SerializeField] private float m_crouchHeight = .5f;
    [SerializeField] private float m_slideForce = 40;
    [SerializeField] private float m_slideCounterMovement = 0.2f;
    private Vector3 m_velocity;

    private void Start()
    {
        m_rb = GetComponent<Rigidbody>();
        m_mouseLocked = true;
    }

    public void OnCrouch(InputValue value)
    {
        m_crouching = value.isPressed;

        if (m_crouching)
            m_camera.position = new Vector3(m_camera.position.x, m_camera.position.y - m_crouchHeight, m_camera.position.z);
        else
            m_camera.position = new Vector3(m_camera.position.x, m_camera.position.y + m_crouchHeight, m_camera.position.z);

    }

    public void OnRun(InputValue value)
    {
        m_running = value.isPressed;
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard.escapeKey.isPressed && !Cursor.visible)
            m_mouseLocked = false;

        if (Mouse.current.leftButton.isPressed && Cursor.visible)
            m_mouseLocked = true;

        if (m_mouseLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void UnlockMouse()
    {
        m_mouseLocked = false;
    }

    void FixedUpdate()
    {
        m_velocity = m_rb.velocity;

        Move();

        if (!m_isMoving && m_grounded && !m_crouching && m_timeSinceLastMove < m_deccelerationTime)
        {
            Debug.Log("Slowing Down");
            float fallspeed = m_rb.velocity.y;
            m_timeSinceLastMove += (Time.fixedDeltaTime / m_deccelerationTime);
            Vector3 n = new Vector3(m_velocity.x, 0, m_velocity.z) * m_slowDownCurve.Evaluate(m_deccelerationTime - m_timeSinceLastMove);
            m_velocity -= n;
            m_velocity.y = fallspeed;
        }

        if (!m_grounded)
        {
            float fallspeed = m_rb.velocity.y;
            float speed = new Vector3(m_velocity.x, 0, m_velocity.z).magnitude * m_inAirMoveFactor;
            Vector3 n = m_velocity.normalized * speed;
            m_rb.velocity += new Vector3(n.x, fallspeed, n.z);
        }
        else
        {
            m_rb.velocity = m_velocity;
        }

        if (footAudioSource != null && m_grounded && m_rb.velocity.magnitude > 0)
            footAudioSource.Play();
    }

    public void OnMove(InputValue value)
    {
        Vector2 inputValue = value.Get<Vector2>();
        m_movementInput = inputValue;
        if (m_wallRunning) m_movementInput.x = 0;
        m_isMoving = inputValue.x != 0 || inputValue.y != 0;

        if (m_isMoving && m_grounded)
            m_timeSinceLastMove = 0;
    }

    private void Move()
    {
        var forwardVec = transform.forward * .7f * m_movementInput.y;
        var rightVec = transform.right * m_movementInput.x;
        m_velocity += (forwardVec + rightVec).normalized * m_moveSpeed * Time.fixedDeltaTime * m_moveMultiplier;
        //m_rb.AddForce((forwardVec + rightVec).normalized * m_moveSpeed * Time.fixedDeltaTime * m_moveMultiplier, ForceMode.Acceleration);

        if (m_crouching && m_grounded && m_rb.velocity.magnitude > 0.5f)
        {
            m_velocity += -m_velocity.normalized * Time.fixedDeltaTime * m_slideCounterMovement;
            //rb.AddForce(moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideCounterMovement);
        }

        if (m_velocity.magnitude > m_maxSpeed)
        {
            float fallspeed = m_rb.velocity.y;
            Vector3 n = m_velocity.normalized * m_maxSpeed;
            m_velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    public void OnJump()
    {
        if (m_grounded || m_wallRunning)
        {
            if (m_isMoving)
                m_timeSinceLastMove = 0;

            if (m_grounded)
            {
                m_grounded = false;
            }
            if (m_wallRunning)
            {
                m_wallRunning = false;
            }

            var projectedVelocity = m_velocity.normalized * .3f;
            var force = (Vector3.up + (m_normalVector * .5f) + projectedVelocity).normalized * m_jumpForce;
            var lateralForce = Vector3.ProjectOnPlane(force, m_normalVector);
            m_rb.AddForce(lateralForce * 150.0f, ForceMode.Force);
            m_rb.AddForce(force - lateralForce, ForceMode.Impulse);

        }
    }

    private void counterMovement(Vector2 movementInput, Vector2 mag)
    {
        //Counter movement
        if (Mathf.Abs(mag.x) > m_threshold && Mathf.Abs(movementInput.x) < 0.05f || (mag.x < -m_threshold && movementInput.x > 0) || (mag.x > m_threshold && movementInput.x < 0))
        {
            m_velocity += transform.right * Time.fixedDeltaTime * -mag.x * m_counterMovement;
            //m_rb.AddForce(m_moveSpeed * transform.right * Time.deltaTime * -mag.x * m_counterMovement);
        }
        if (Mathf.Abs(mag.y) > m_threshold && Mathf.Abs(movementInput.y) < 0.05f || (mag.y < -m_threshold && movementInput.y > 0) || (mag.y > m_threshold && movementInput.y < 0))
        {
            m_velocity += transform.forward * Time.fixedDeltaTime * -mag.y * m_counterMovement;
            //m_rb.AddForce(m_moveSpeed * transform.forward * Time.deltaTime * -mag.y * m_counterMovement);
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
            sumNormals += normal;
            //FLOOR
            if (isFloor(normal))
            {
                m_grounded = true;
                m_cancellingGrounded = false;
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

    private void OnDrawGizmos()
    {
        //Gizmos.DrawRay(transform.position, m_normalVector.normalized * 5);
    }
}
