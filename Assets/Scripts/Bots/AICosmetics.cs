using System.Collections.Generic;
using UnityEngine;

public class AICosmetics : MonoBehaviour, IAIBehaviour
{
    public List<GameObject> Cosmetics;

    public void Initialize(AIEntity entity)
    {
        if (Cosmetics.Count > 0)
        {
            foreach (GameObject item in Cosmetics)
            {
                item.SetActive(false);
            }

            if (Random.Range(0, 1) == 0)
            {
                Cosmetics[Random.Range(0, Cosmetics.Count)].SetActive(true);
            }
        }
    }
}
