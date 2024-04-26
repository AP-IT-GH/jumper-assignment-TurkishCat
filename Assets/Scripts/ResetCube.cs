using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetCube : MonoBehaviour
{
    public CubeAgentRays MainCube;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Wall")
        {
            Debug.Log("Wall passed the main cube");
            MainCube.Wall_Passed();
        }



        else if (collision.collider.tag == "Target")
        {
            Debug.Log("Target passed the main cube");
            MainCube.Target_Passed();
        }

    }
}
