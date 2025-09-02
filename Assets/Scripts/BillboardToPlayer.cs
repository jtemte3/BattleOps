using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardToPlayer : MonoBehaviour
{
    private GameObject playerObject;
    private void Start()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");
    }
    // Update is called once per frame
    void Update()
    {
        transform.LookAt(playerObject.transform.position);
        transform.rotation = transform.rotation * Quaternion.Euler(0, 180, 0);
    }
}
