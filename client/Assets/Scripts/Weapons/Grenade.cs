using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
public class Grenade : NetworkBehaviour
{
    [HideInInspector] public GrenadeLauncher parent;
    [SerializeField] private float lifeSpan = 3;
    [HideInInspector] public int bounces = 0;
    [HideInInspector] public Voxelizer voxelizer;


    private void Update()
    {
        if (!IsOwner) return;

        lifeSpan -= Time.deltaTime;
        if (lifeSpan <= 0)
        {
            explode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner) return;

        bounces -= 1;
        if (bounces < 0)
            explode();
    }

    private void explode()
    {
        //Debug.Log("KABOOM");
        if (IsOwner)
            FindObjectOfType<Voxelizer>().CreateSmoke(transform.position);
        else
            instantiateSmokeServerRpc();

        parent.DestroyServerRpc();
        Destroy(gameObject);
    }


    [ServerRpc(RequireOwnership = false)]
    private void instantiateSmokeServerRpc()
    {
        FindObjectOfType<Voxelizer>().CreateSmoke(transform.position);
    }
}
