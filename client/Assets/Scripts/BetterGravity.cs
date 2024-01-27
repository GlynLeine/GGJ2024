using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BetterGravity : MonoBehaviour
{
   private Rigidbody rb;
   public float GravityScale = 1.0f;
    public bool UseGravity = true;
    public static float baseGravityRate = -9.8f;

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }
    private void FixedUpdate()
    {
        if (UseGravity)
        {
            Vector3 gravity = Vector3.up * baseGravityRate * GravityScale;
            rb.AddForce(gravity,ForceMode.Acceleration);
        }
    }
}
