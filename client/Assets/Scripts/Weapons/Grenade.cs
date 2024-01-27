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
        Debug.Log("KABOOM");
        voxelizer.CreateSmoke(transform.position);
        Destroy(gameObject);
        parent.DestroyServerRpc();
    }


    [ServerRpc(RequireOwnership = false)]
    private void instantiateSmokeServerRpc()
    {
        //do this at some point
        //Gameobject smoke;
        //smoke.GetComponent<NetworkObject>().Spawn();
    }
}
