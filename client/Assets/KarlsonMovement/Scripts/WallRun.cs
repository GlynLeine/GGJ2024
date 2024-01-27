using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(Movement), typeof(BetterGravity), typeof(Rigidbody))]
public class WallRun : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform orientation;
    [SerializeField] private float minimumSpeed = 15f;
    private BetterGravity gravity;
    private Movement movement;

    [Header("Detection")]
    [SerializeField] private float wallDistance = .5f;
    [SerializeField] private float minimumJumpHeight = 1.5f;

    [Header("Wall Running")]
    [SerializeField] private float wallRunGravity;
    [SerializeField] private float wallRunJumpForce;

    [Header("Camera")]
    [SerializeField] private Camera cam;
    [SerializeField] private float fov;
    [SerializeField] private float wallRunfov;
    [SerializeField] private float wallRunfovTime;
    [SerializeField] private float camTilt;
    [SerializeField] private float camTiltTime;

    public float tilt { get; private set; }

    private bool wallLeft = false;
    private bool wallRight = false;

    RaycastHit leftWallHit;
    RaycastHit rightWallHit;

    private Rigidbody rb;
    public static bool canFov;

    bool CanWallRun()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minimumJumpHeight) && rb.velocity.magnitude > minimumSpeed && !movement.Grounded;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        movement = GetComponent<Movement>();
        gravity = GetComponent<BetterGravity> ();
    }

    void CheckWall()
    {
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallDistance);
        Debug.DrawRay(transform.position, -orientation.right * wallDistance, Color.red);
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallDistance);
        Debug.DrawRay(transform.position, orientation.right * wallDistance, Color.yellow);
    }

    private void Update()
    {
        CheckWall();

        if (CanWallRun())
        {
            if (wallLeft)
            {
                StartWallRun();
                Debug.Log("wall running on the left");
            }
            else if (wallRight)
            {
                StartWallRun();
                Debug.Log("wall running on the right");
            }
            else
            {
                StopWallRun();
            }
        }
        else
        {
            StopWallRun();
        }
    }

    void StartWallRun()
    {
        Debug.Log("Starting wall run");
        gravity.UseGravity = false;
        movement.m_wallRunning = true;
        rb.AddForce(Vector3.down * wallRunGravity, ForceMode.Force);

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, wallRunfov, wallRunfovTime * Time.deltaTime);

        if (wallLeft)
            StartCoroutine(TiltCamera(-camTilt));
        else if (wallRight)
            StartCoroutine(TiltCamera(camTilt));


    }

    public void OnJump()
    {
        if (wallLeft)
        {
            Vector3 wallRunJumpDirection = transform.up * .5f + leftWallHit.normal * 1.5f;
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(wallRunJumpDirection * wallRunJumpForce, ForceMode.Impulse);
        }
        else if (wallRight)
        {
            Vector3 wallRunJumpDirection = transform.up * .5f + rightWallHit.normal * 1.5f;
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(wallRunJumpDirection * wallRunJumpForce , ForceMode.Impulse);
        }
    }


    void StopWallRun()
    {
        Debug.Log("stopping wall run");
        gravity.UseGravity = true;
        movement.m_wallRunning = false;
        if (canFov)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fov, wallRunfovTime * Time.deltaTime);
        }

        StartCoroutine(TiltCamera(0));
    }

    IEnumerator TiltCamera(float endValue)
    {
        float timer = 0.0f;
        while (timer < camTiltTime)
        {
            timer += Time.deltaTime;
            tilt = Mathf.Lerp(tilt, endValue, 1 / camTiltTime * timer);
            yield return new WaitForEndOfFrame();
        }
        yield break;
    }
}