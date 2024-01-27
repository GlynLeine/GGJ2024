using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KatanaManager : MonoBehaviour
{
    public GrapplinGun grapplinGun;
    public Transform endPosition;
    public Transform startPosition;
    public GameObject grappleTrans;
    void Start()
    {
        
    }
    public float speed;
    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButton(grapplinGun.grappleNbrKey))
        {
            grappleTrans.transform.position = Vector3.Lerp(transform.position, endPosition.position, Time.deltaTime * speed);
            
        }
        else
        {
            grappleTrans.transform.position = Vector3.Lerp(transform.position, startPosition.position, Time.deltaTime * speed);

        }
    }

}
