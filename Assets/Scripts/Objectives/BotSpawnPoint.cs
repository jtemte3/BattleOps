using UnityEngine;

public class BotSpawnPoint : MonoBehaviour
{
    [Header("Setup")]
    public GameObject botPrefab;
    public float timer;
    float spawnTime;
    public bool hasSpawned = false;

    [Header("Chance Spawning")]
    public bool chanceSpawn = true;
    public float spawnChance = 0;
    public float spawnThreshold = 50;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnChance = Random.Range(0, 100);
        if (spawnChance <= spawnThreshold)
        {
            spawnTime = Time.time + timer;
        }
        else
        {
            this.enabled = false;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > spawnTime && hasSpawned != true)
        {
            Instantiate(botPrefab, transform.position, transform.rotation);
            hasSpawned = true;

            this.enabled = false;
        }
    }
}
