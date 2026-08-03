using UnityEngine;

public class TargetAdjustor : MonoBehaviour
{
    public GameObject target;
    public GameObject playerTarget;

    public ProceduralWeaponMotion weaponMotion;

    public float range = 5;

    private Vector3 initialPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialPos = target.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(this.transform.position, playerTarget.transform.position);

        if (distance < range)
        {
            target.transform.position = playerTarget.transform.position;
        }
        else
        {
            target.transform.localPosition = initialPos;
        }
    }

    public void ShootOnce()
    {
        weaponMotion.Fire();
    }
}
