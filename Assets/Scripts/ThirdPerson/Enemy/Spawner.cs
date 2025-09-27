using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private float timeBetweenSpawn = 3f;
    [SerializeField] private float spawnIncidence = 1f;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int maxEnemySpawned = 5;


    private Transform[] spawners;
    public int aliveEnemies = 0;
    private SpawnerLife spawnerLife;
    private void Awake()
    {
        spawners = GetComponentsInChildren<Transform>();
        spawnerLife = GetComponentInParent<SpawnerLife>();
    }

    private void Start()
    {
        StartCoroutine(SpawnCoroutine());
    }
    private void Update()
    {
        
    }
    private IEnumerator SpawnCoroutine()
    {
        
        while (true)
        {
            if (spawnerLife.hitlerIsDead)
            {
                spawnIncidence = 0f;
            }
            yield return new WaitForSeconds(timeBetweenSpawn / spawnIncidence);

            if (aliveEnemies < maxEnemySpawned)
            {
                int spawnNumber = Random.Range(0, spawners.Length);
                Vector3 spawnPos = spawners[spawnNumber].position;

                GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                aliveEnemies++;
            }
            
        }
    }

    private void OnEnable()
    {
        EnemyHealth.OnEnemyDeath += EnemyDied;
    }

    private void OnDisable()
    {
        EnemyHealth.OnEnemyDeath -= EnemyDied;
    }

    private void EnemyDied(EnemyHealth enemy)
    {
        aliveEnemies = Mathf.Max(0, aliveEnemies - 1);
    }
}
