using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasTriggerSphere : MonoBehaviour
{
    private List<Health> targetHealth = new List<Health>();

    [SerializeField] private float timeBetweenDamageTicks = .3f;
    float damageTimer;
    [SerializeField] private int damagePerTick = 1;
    float lifeSpan;
    float timer = 0;
    Vector3 direction;
    float speed;
    public void Initialize(float lifeSpan, Vector3 direction, float speed, float size)
    {
        this.lifeSpan = lifeSpan;
        this.direction = direction;
        this.speed = speed;
        transform.localScale = Vector3.one * size;
    }
    private void Update()
    {
        timer += Time.deltaTime;
        if(timer >= lifeSpan)
        {
            Destroy(gameObject);
        }

        transform.position += (direction * speed * Time.deltaTime);
    }
    private void OnTriggerEnter(Collider other)
    {
        
        if( other.GetComponent<Health>() is Health health){
            if(health.isBeingDamaged) { 
                return;
            }
            if(!targetHealth.Contains(health))
            targetHealth.Add(health);
        }
    }

    private void OnTriggerStay(Collider other)
    {
            damageTimer += Time.deltaTime;
            if(damageTimer >= timeBetweenDamageTicks)
            {

                damageTimer = 0;
                targetHealth.ForEach(h => h.DamageHealth(damagePerTick));
            }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Health>() != null &&  targetHealth.Contains(other.GetComponent<Health>()))
        {
            targetHealth.Remove(other.GetComponent<Health>());
        }
    }

}
