using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class zipline : MonoBehaviour
{
    public PlayerMovement pm;
    public WallRun wr;
    public Transform orientation;
    public ladder ladder;
    Rigidbody rb;

    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "zipline")
        {
            rb.velocity = other.transform.up * speed;
            pm.enabled = false;
            wr.enabled = false;
            ladder.enabled = false;
            rb.useGravity = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "zipline")
        {
            rb.velocity = Vector3.zero;
            pm.enabled = true;
           
            ladder.enabled = true;
           
        }
    }
}
