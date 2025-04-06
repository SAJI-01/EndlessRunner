using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    #region Fields

    [Header("Spawn Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float collectableDetectionRadius = 1.5f;
    [SerializeField] private LayerMask CollectableLayerMask;
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private float minSpawnInterval = 1f;
    [SerializeField] private float maxSpawnInterval = 3f;
    [SerializeField] private float spawnDistance = 80f;
    [SerializeField] private int laneCount = 3;
    [SerializeField] private int laneWidth = 3;
    [SerializeField] private float obstacleLifetime = 10f;

    [Header("Difficulty Settings")]
    [SerializeField] private float difficultyIncreaseRate = 0.1f;
    [SerializeField] private float minPossibleInterval = 0.5f;
    [SerializeField] private float initialDelay = 2f;
    [SerializeField] private int maxObstaclesPerRow = 2; 

    // Object pooling
    private Queue<GameObject> obstaclePool = new Queue<GameObject>();
    private int poolSize = 20;

    private float currentDifficulty = 1f;

    #endregion

    
    private void Start()
    {
        InitializeObstaclePool();
        StartCoroutine(SpawnObstacleRoutine());
    }
    private void InitializeObstaclePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obstacle = Instantiate(obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)]);
            obstacle.SetActive(false);
            obstaclePool.Enqueue(obstacle);
        }
    }
    private IEnumerator SpawnObstacleRoutine()
    {
        //initial delay before starting to spawn obstacles
        yield return new WaitForSeconds(initialDelay);
        
        while (true)
        {
            // Apply difficulty to spawn interval
            float spawnInterval = Mathf.Clamp(Random.Range(minSpawnInterval, maxSpawnInterval) / currentDifficulty, minPossibleInterval, maxSpawnInterval);
            
            SpawnObstacleOnLane();
            
            // Slowly increase difficulty over time
            currentDifficulty += difficultyIncreaseRate * Time.deltaTime;
            
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    private void SpawnObstacleOnLane()
    {
        // Determine how many obstacles to spawn in this row (harder as game progresses)
        int obstacleCount = Mathf.Min(Random.Range(1, Mathf.FloorToInt(currentDifficulty) + 1), maxObstaclesPerRow);
        
        // Generate all possible lane positions
        List<int> availableLanes = new List<int>();
        for (int i = 0; i < laneCount; i++)
        {
            availableLanes.Add(i);
        }

        for (int i = 0; i < obstacleCount; i++)
        {
            // If we've run out of lanes, break
            if (availableLanes.Count == 0) break;

            int randomLane = Random.Range(0, availableLanes.Count);
            int lane = availableLanes[randomLane];
            availableLanes.RemoveAt(randomLane); // Remove this lane so we don't place another obstacle here
            
            float xPos = (lane - 1) * laneWidth;

            // Calculate spawn position ahead of the player
            Vector3 spawnPosition = new Vector3(xPos, 0, playerTransform.position.z + spawnDistance);

            if (!IsCollectableNearby(spawnPosition))
            {
                GameObject obstacle = GetObstacleFromPool();
                obstacle.transform.position = spawnPosition;
                obstacle.SetActive(true);
                
                StartCoroutine(ReturnToObstaclePoolAfterDelay(obstacle, obstacleLifetime));
                return;
            }

        }
    }
    private bool IsCollectableNearby(Vector3 position)
    {
        var colliders = Physics.OverlapSphere(position, collectableDetectionRadius, CollectableLayerMask);
        return colliders.Length > 0;
    }
    private GameObject GetObstacleFromPool()
    {
        // If the pool is empty, add more obstacles
        if (obstaclePool.Count == 0)
        {
            GameObject newObstacle = Instantiate(obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)]);
            newObstacle.SetActive(false);
            return newObstacle;
        }

        // Get an obstacle from the Existing pool
        GameObject obstacle = obstaclePool.Dequeue();
        return obstacle;
    }
    private IEnumerator ReturnToObstaclePoolAfterDelay(GameObject obstacle, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (obstacle != null)
        {
            obstacle.SetActive(false);
            obstaclePool.Enqueue(obstacle);
        }
    }
}