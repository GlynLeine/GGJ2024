using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dash : MonoBehaviour
{
    //value
    public float dashForce, dashAirForce;
    public float speed;
    //assignable
    public Transform orientation;
    public PlayerMovement playerMovement;
    Rigidbody rb;

    [Header("Keybinds")]
    [SerializeField] KeyCode dashkey = KeyCode.C;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(dashkey))
        {
            if(playerMovement.grounded)
            {
                rb.AddForce(orientation.forward * dashForce * speed * Time.deltaTime, ForceMode.Impulse);
            }
            else
            {
                rb.AddForce(orientation.forward * dashAirForce *speed * Time.deltaTime , ForceMode.Impulse);
            }
        }
    }
}
