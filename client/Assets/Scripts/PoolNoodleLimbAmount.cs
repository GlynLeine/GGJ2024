using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolNoodlePhysics : MonoBehaviour
{
    public GameObject NoodleLimb;
    public Transform NoodleLimbTransform;
    public int NoodleLimbAmount;
    GameObject LastNoodleLimbCopy;
    
    float x, y, z;
    public float xScale, yScale, zScale;
    


    // Start is called before the first frame update
    void Start()
    {
        x = transform.position.x;
        y = transform.position.y;
        z = transform.position.z;

        for (int i = 0; i < NoodleLimbAmount; i++)
        {

            GameObject NoodleLimbCopy = Instantiate(NoodleLimb, new Vector3(x, y, z), Quaternion.identity, NoodleLimbTransform);
            NoodleLimbCopy.transform.localScale = new Vector3(xScale, yScale, zScale);
            y += yScale*2f;
           
            if (i == 0)
            {
                Rigidbody FirstBody = GetComponent<Rigidbody>();
                CharacterJoint FirstJoint = NoodleLimbCopy.GetComponent<CharacterJoint>();
                FirstJoint.connectedBody = FirstBody;
                LastNoodleLimbCopy = NoodleLimbCopy;
                continue;
            }
           
            Rigidbody body = LastNoodleLimbCopy.GetComponent<Rigidbody>();
            CharacterJoint joint = NoodleLimbCopy.GetComponent<CharacterJoint>();
            joint.connectedBody = body;
            LastNoodleLimbCopy = NoodleLimbCopy;

        }




    }

}
