using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.VisualScripting;
using System.Text;
using NaughtyAttributes;

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    public static bool MouseLocked => !Cursor.visible && (Cursor.lockState == CursorLockMode.Locked || Cursor.lockState == CursorLockMode.Confined);
    public bool Grounded => m_grounded;
    public bool CanSlide => m_crouching && IsMoving && m_grounded && m_velocity.magnitude > m_slideThresholdSpeed;
    public bool Sliding => m_crouching && m_grounded && m_velocity.magnitude >= 0.01f && !IsMoving && m_allowSliding;
    public bool CanJump => m_grounded;
    public bool NeedToSlowDown => !IsMoving && !Sliding && m_grounded && m_velocity.magnitude >= 0.01f;
    public bool FreeSliding => m_grounded && isSurfaceFlat(m_normalVector) && m_velocity.magnitude > 0 && !IsMoving;
    public bool IsMoving => MoveInput.magnitude > Mathf.Epsilon;
    public float MaxSpeedAdjustment => (m_crouching && m_grounded ? m_crouchFactor : 1) * (!m_grounded ? m_inAirMoveFactor : 1) * (m_running ? 1 : 1);//gotta implement running at some point lol
    public float MaxSpeed => m_maxSpeed * MaxSpeedAdjustment;

    public Vector2 MoveInput => m_movementInput;

    private bool m_grounded = false;
    private bool m_jumping = false;
    private bool m_crouching = false;
    private bool m_running = false;
    private bool m_cancellingGrounded = false;
    private bool m_mouseLocked = true;
    private bool m_allowSliding = false;
    [HideInInspector] public bool m_wallRunning = false;

    private Rigidbody m_rb;
    private Vector3 m_normalVector = Vector3.up;
    private Vector3 m_wallNormalVector = Vector3.zero;
    private Vector3 m_jumpVelocity = Vector3.zero;
    private Vector2 m_movementInput = Vector2.zero;
    private Vector3 m_velocity = Vector2.zero;
    private Vector3 m_slideVec = Vector3.zero;

    [Header("Assignable")]
    [SerializeField] private Transform m_camera;
    [SerializeField] private AudioSource footAudioSource;

    [Header("Movement")]
    [SerializeField] private AnimationCurve m_slowDownCurve;
    [SerializeField, Tooltip("How fast our character should accelerate to the maxSpeed (units/sec/sec)")] private float m_acceleration = 50f;
    [SerializeField, Tooltip("Max velocity for our character (units/sec)")] private float m_maxSpeed = 15f;
    [SerializeField] public LayerMask m_whatIsGround;
    [SerializeField] private float m_maxSlopeAngle = 35f;
    [SerializeField] private float m_deccelerationTime = 1.0f;
    private float m_timeSinceLastMove = 0;

    [Header("Jumping")]
    [SerializeField] private float m_jumpCooldown = 0.25f;
    [SerializeField] private float m_jumpForce = 100f;
    [SerializeField] private float m_inAirMoveFactor = .7f;

    [Header("Crouch & Slide")]
    [SerializeField] private float m_crouchFactor = 0.5f;
    [SerializeField] private float m_crouchHeight = .5f;
    [SerializeField] private AnimationCurve m_slideSpeedCurve;
    [SerializeField] private float m_slideTime = 0.0f;
    [SerializeField] private float m_slideThresholdSpeed = 7.0f;

    private float m_timeSinceSlideStart = 0.0f;

    private void Start()
    {
        m_rb = GetComponent<Rigidbody>();
        m_mouseLocked = true;
    }

    public void OnCrouch(InputValue value)
    {
        m_crouching = value.isPressed;

        if (m_crouching)
            m_camera.parent.position = new Vector3(m_camera.parent.position.x, m_camera.parent.position.y - m_crouchHeight, m_camera.parent.position.z);
        else
            m_camera.parent.position = new Vector3(m_camera.parent.position.x, m_camera.parent.position.y + m_crouchHeight, m_camera.parent.position.z);

        if (!Sliding)
            m_timeSinceSlideStart = 0;

        m_allowSliding = CanSlide;
        if (m_allowSliding)
            m_slideVec = m_velocity.normalized;
        else
            m_slideVec = Vector3.zero;
    }

    public void OnRun(InputValue value)
    {
        m_running = value.isPressed && !m_crouching;
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

        if (NeedToSlowDown)
        {
            float fallspeed = m_rb.velocity.y;
            m_timeSinceLastMove += (Time.fixedDeltaTime);
            Vector3 n = new Vector3(m_velocity.x, 0, m_velocity.z) * (m_slowDownCurve.Evaluate(m_timeSinceLastMove) * m_deccelerationTime);
            m_velocity = n;
            m_velocity.y = fallspeed;

            if (m_velocity.magnitude < 0.1f)
            {
                m_timeSinceLastMove = 0;
                m_velocity = Vector3.zero;
            }
        }

        m_rb.velocity = m_velocity;

        if (footAudioSource != null && m_grounded && m_rb.velocity.magnitude > 0)
            footAudioSource.Play();
    }

    public void OnMove(InputValue value)
    {
        Vector2 inputValue = value.Get<Vector2>();
        m_movementInput = inputValue;
        if (m_wallRunning) m_movementInput.x = 0;

        if (IsMoving && m_grounded)
            m_timeSinceLastMove = 0;
    }

    private void Move()
    {
        if (Sliding && m_allowSliding)
        {
            m_timeSinceSlideStart += Time.fixedDeltaTime;
            var vel = m_slideVec * m_acceleration * Time.fixedDeltaTime;

            Vector3 n = new Vector3(vel.x, 0, vel.z) * (m_slideSpeedCurve.Evaluate(m_timeSinceSlideStart) * m_slideTime);
            float fallspeed = m_rb.velocity.y;
            m_velocity = n;
            m_velocity.y = fallspeed;

            if (m_velocity.magnitude < 0.1f)
            {
                m_timeSinceSlideStart = 0;
                m_velocity = Vector3.zero;
                m_allowSliding = false;
            }
        }
        else
        {
            var forwardVec = transform.forward * .7f * m_movementInput.y;
            var rightVec = transform.right * m_movementInput.x;
            var vel = (forwardVec + rightVec).normalized * m_acceleration * Time.fixedDeltaTime;
            m_timeSinceSlideStart = 0;

            if (!m_grounded)
            {
                m_velocity += vel * m_inAirMoveFactor;
            }
            else if (m_crouching)
            {
                m_velocity += vel * m_crouchFactor;
            }
            else
                m_velocity += vel;
        }

        if (m_velocity.magnitude > MaxSpeed && m_grounded && !Sliding)
        {
            float fallspeed = m_rb.velocity.y;
            Vector3 n = new Vector3(m_velocity.x, 0, m_velocity.z).normalized * MaxSpeed;
            m_velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    public void OnJump()
    {
        if (m_grounded /*|| m_wallRunning*/)
        {
            if (IsMoving)
                m_timeSinceLastMove = 0;

            if (m_grounded)
            {
                m_grounded = false;
            }
            //if (m_wallRunning)
            //{
            //    m_wallRunning = false;
            //}

            var projectedVelocity = m_velocity.normalized * .6f;
            var force = (Vector3.up + (m_normalVector * .5f) + projectedVelocity).normalized * m_jumpForce;
            var lateralForce = Vector3.ProjectOnPlane(force, m_normalVector);
            m_rb.AddForce(lateralForce * 15.0f, ForceMode.Force);
            m_rb.AddForce(force - lateralForce, ForceMode.Impulse);
        }
    }

    private void stopGrounded()
    {
        m_grounded = false;
    }

    private void OnCollisionStay(Collision other)
    {
        //Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (m_whatIsGround != (m_whatIsGround | (1 << layer))) return;

        //Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
            //FLOOR
            if (isSurfaceFlat(normal))
            {
                m_grounded = true;
                m_cancellingGrounded = false;
                m_normalVector = normal;
                CancelInvoke(nameof(stopGrounded));
            }
        }

        //Invoke ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 3f;
        if (!m_cancellingGrounded)
        {
            m_cancellingGrounded = true;
            Invoke(nameof(stopGrounded), Time.deltaTime * delay);
        }
    }

    private bool isSurfaceFlat(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < m_maxSlopeAngle;
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.textArea);
        style.fontSize = 18;
        style.normal.textColor = Color.black;
        var topRect = new Rect(10, 10, 300, 50);
        var rectDelta = new Rect(0, topRect.height + 5.0f, 0, 0);
        GUI.Label(topRect, $"CanSlide: {CanSlide}", style);
        //GUI.Label(topRect.Append(rectDelta), $"FreeSliding: {FreeSliding}", style);
        //GUI.Label(topRect.Append(rectDelta), $"NeedToSlowdown: {NeedToSlowDown}", style);
        GUI.Label(topRect.Append(rectDelta), $"SlideTime: {m_timeSinceSlideStart}", style);
        GUI.Label(topRect.Append(rectDelta), $"Sliding: {Sliding}", style);
        GUI.Label(topRect.Append(rectDelta), $"m_crouching: {m_crouching}", style);
        GUI.Label(topRect.Append(rectDelta), $"m_grounded: {m_grounded}", style);
        GUI.Label(topRect.Append(rectDelta), $"Velocity >= 0: {m_velocity.magnitude >= 0.01f}", style);
        GUI.Label(topRect.Append(rectDelta), $"!IsMoving: {!IsMoving}", style);

        GUI.Label(topRect.Append(rectDelta), $"Velocity: {m_velocity}", style);
        GUI.Label(topRect.Append(rectDelta), $"Velocity Magnitude: {m_velocity.magnitude}", style);

        //
        //
        //GUI.Label(topRect.Append(rectDelta), $"m_Crouching: {m_crouching}", style);
        //GUI.Label(topRect.Append(rectDelta), $"m_CrouchFactor: {m_crouchFactor}", style);
        //GUI.Label(topRect.Append(rectDelta), $"Normal Vector: {m_normalVector}", style);
    }
}
public static class RectExtensions
{
    public static Rect Add(this Rect rct, Rect other)
    {
        return new Rect(rct.x + other.x, rct.y + other.y, rct.width + other.width, rct.height + other.height);
    }
    public static Rect Append(this ref Rect rct, Rect other)
    {
        rct.x += other.x;
        rct.y += other.y;
        rct.width += other.width;
        rct.height += other.height;
        return rct;
    }
}