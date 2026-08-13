using UnityEngine;
using UnityEngine.Events;

public class HitBox : MonoBehaviour
{
    public AIEntity entity;
    public float spotDamage;

    [Tooltip("Events to trigger on Confirmed Hit")]
    public UnityEvent<float> OnHit;
    public UnityEvent<float> onHit => OnHit;

    /*private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<BulletData>())
        {
            BulletData data = other.gameObject.GetComponent<BulletData>();
            if (data.team != AITeam.Enemy)
            {
                float damage = spotDamage;
                OnHit.Invoke(damage);
                Debug.Log("Hit by: " + data.team.ToString() + ", Damage: " + damage);
            }
        }
    }*/

    //Raycast-based collision detection
    public void TakeHit(BulletData data)
    {
        if (data.sourceEntity.team != entity.team)
        {
            float damage = spotDamage;
            OnHit.Invoke(damage);
            Debug.Log("Hit by: " + data.sourceEntity.team.ToString() + ", Damage: " + damage);
        }
    }

    public void TakeHit(Grenade data, float damage)
    {
        if (data.sourceEntity.team != entity.team)
        {
            OnHit.Invoke(damage);
            Debug.Log("Grenade Hit by: " + data.sourceEntity.team.ToString() + ", Damage: " + damage);
        }
    }
}
