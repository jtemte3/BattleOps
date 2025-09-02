using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class MH6Rotor : MonoBehaviour
{
    public GameObject rotor;
    public float rotationSpeed;
    public Axis axis;
    // Update is called once per frame
    void FixedUpdate()
    {
        switch (axis.ToString())
        {
            case ("X"):
                {
                    rotor.transform.Rotate(Vector3.right, Time.deltaTime * rotationSpeed);
                    break;
                }
            case ("Y"):
                {
                    rotor.transform.Rotate(Vector3.up, Time.deltaTime * rotationSpeed);
                    break;
                }
            case ("Z"):
                {
                    rotor.transform.Rotate(Vector3.forward, Time.deltaTime * rotationSpeed);
                    break;
                }
        }
    }
}
