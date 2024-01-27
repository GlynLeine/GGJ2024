using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Grenade : MonoBehaviour
{
    [SerializeField] private float lifeSpan = 3;
    private int bounces = 0;
    private Voxelizer voxelizer;
    
    public void Initialize(int bounces, Vector3 direction, float force, Voxelizer v)
    {
        this.bounces = bounces;
        voxelizer = v;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.AddForce(direction *  force, ForceMode.Impulse);
    }

    private void Update()
    {
        lifeSpan -= Time.deltaTime;
        if(lifeSpan <= 0)
        {
            Explode();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        bounces -= 1;
        if(bounces < 0)
        {
            Explode();
        }
    }

    private void Explode()
    {
        Debug.Log("KABOOM");
        voxelizer.CreateSmoke(transform.position);
        Destroy(gameObject);
    }
}
