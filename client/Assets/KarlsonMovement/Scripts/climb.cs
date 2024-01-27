using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class climb : MonoBehaviour
{
    public Transform orientation;
    public float maxDistance;

    public Transform downRay;
    public Transform upRay;

    public bool up;
    public bool down;
    public PlayerMovement playerMove;
    Rigidbody rb;

    public int layer_mask;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
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

       if (playerMove.jumping)
       {
          check();
       }
    }

    public float force;
    void check()
    {
        if(down && up)
        {
            rb.AddForce(orientation.up * force, ForceMode.Impulse);
            rb.AddForce(orientation.forward * 0.08f, ForceMode.Impulse);
            //playerMove.cameraAnim.SetTrigger("climb");
        }
    }
}
