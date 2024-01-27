using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ladder : MonoBehaviour
{
    public PlayerMovement playerMove;
    public WallRun wallRun;
    Rigidbody rb;

    public float multiplier;
    public bool isLaddering;
    public Transform cam;

    public Transform orientation;

    public Transform downRay;
    public Transform upRay;

    public bool up;
    public bool down;

    public float maxDistance;
    public int layer_mask;

    public float speed;
    float positiveSpeed;
   float negativeSpeed;
    public bool canLadder;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        canLadder = true;

        positiveSpeed = speed;
        negativeSpeed = -speed;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            speed = positiveSpeed;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            speed = negativeSpeed;
        }
        else
        {
            speed = 0;
        }

        if (!canLadder)
        {
            if (playerMove.grounded)
            {
                canLadder = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && isLaddering)
        {
            canLadder = false;
            rb.AddForce(-playerMove.orientation.forward * 10, ForceMode.Impulse);
        }

        ray();

        if (down && up)
        {


            if (canLadder)
            {
                rb.useGravity = false;
                playerMove.enabled = false;
                isLaddering = true;
                wallRun.enabled = false;






            }
            else
            {
                wallRun.enabled = true;
                isLaddering = false;
                rb.useGravity = true;
                playerMove.enabled = true;
            }
        }
        else
        {



            wallRun.enabled = true;
            isLaddering = false;
            rb.useGravity = true;
            playerMove.enabled = true;
        }
    }

    public float velocityLadder;

    private void FixedUpdate()
    {
        if (isLaddering)
        {
            velocityLadder = speed * multiplier;
            rb.velocity = new Vector3(0, velocityLadder, 0);

            
        }
        else
        {
           
        }

        if (down && !up)
        {
            rb.AddForce(playerMove.orientation.up * exitForce, ForceMode.Impulse);
            rb.AddForce(playerMove.orientation.forward * 1, ForceMode.Impulse);
        }
    }

    public float exitForce;
    void ray()
    {
        RaycastHit hit;

        if (Physics.Raycast(downRay.transform.position, transform.TransformDirection(orientation.transform.forward), out hit, maxDistance, ~layer_mask))
        {
            Debug.DrawRay(downRay.position, transform.TransformDirection(orientation.transform.forward) * hit.distance, Color.red);
            down = true;

        }
        else
        {
            Debug.DrawRay(downRay.position, transform.TransformDirection(orientation.transform.forward) * maxDistance, Color.white);
            down = false;
        }
        RaycastHit hit2;

        if (Physics.Raycast(upRay.position, transform.TransformDirection(orientation.transform.forward), out hit2, maxDistance, ~layer_mask))
        {
            Debug.DrawRay(upRay.position, transform.TransformDirection(orientation.transform.forward) * hit2.distance, Color.red);
            up = true;
        }
        else
        {
            Debug.DrawRay(upRay.position, transform.TransformDirection(orientation.transform.forward) * maxDistance, Color.white);
            up = false;
        }
    }


    



}

