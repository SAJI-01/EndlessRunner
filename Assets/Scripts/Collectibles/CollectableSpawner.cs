using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableSpawner : MonoBehaviour
{
    #region Fields

    [Header("Spawn Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameObject[] collectablePrefabs;
    [SerializeField] private float minSpawnInterval = 1.5f;
    [SerializeField] private float maxSpawnInterval = 4f;
    [SerializeField] private float spawnDistanceAhead = 50f;
    [SerializeField] private int numberOfLanes = 3;
    [SerializeField] private int laneWidth = 3;
    [SerializeField] private float collectableLifetime = 10f;
    [SerializeField] private float collectableHeight = 1f;
    [SerializeField] private float obstacleDetectionRadius = 1f;
    [SerializeField] private LayerMask obstacleLayerMask;

    [Header("Pattern Settings")]
    [SerializeField] private bool enablePatterns = true;
    [SerializeField] private float patternSpawnChance = 0.7f;
    [SerializeField] [Range(3, 10)] private int maxPatternLength = 7;

    private Queue<GameObject> collectablePool = new Queue<GameObject>();
    private const int initialPoolSize = 30;
    
    #endregion
    
    private void Start()
    {
        InitializeCollectablePool();
        StartCoroutine(SpawnCollectables());
    }
    private void InitializeCollectablePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject collectable = Instantiate(collectablePrefabs[Random.Range(0, collectablePrefabs.Length)]);
            collectable.SetActive(false);
            collectablePool.Enqueue(collectable);
        }
    }
    private IEnumerator SpawnCollectables()
    {
        while (true)
        {
            // Spawn either a single collectable or a pattern
            if (enablePatterns && Random.value < patternSpawnChance)
            {
                yield return StartCoroutine(SpawnPattern());
            }
            else
            {
                SpawnSingleCollectable();
                float spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
                yield return new WaitForSeconds(spawnInterval);
            }
        }
    }
    private void SpawnSingleCollectable()
    {
        
        for (int i = 0; i < numberOfLanes; i++)
        {
            int laneIndex = Random.Range(0, numberOfLanes);
            float spawnXPos = (laneIndex - 1) * laneWidth;

            Vector3 spawnPosition = new Vector3(spawnXPos, collectableHeight, playerTransform.position.z + spawnDistanceAhead);

            if (!IsObstacleNearby(spawnPosition))
            {
                GameObject collectable = GetCollectableFromPool();
                collectable.transform.position = spawnPosition;
                collectable.SetActive(true);

                StartCoroutine(ReturnToCollectablesPoolAfterDelay(collectable, collectableLifetime));
                return;
            }
        }
    }
    private bool IsObstacleNearby(Vector3 position)
    {
        var colliders = Physics.OverlapSphere(position, obstacleDetectionRadius, obstacleLayerMask);
        return colliders.Length > 0;
    }
    private IEnumerator SpawnPattern()
    {
        int patternType = Random.Range(0, 2);
        int patternLength = Random.Range(3, maxPatternLength + 1);
        float spacingBetweenCollectables = 2f;

        switch (patternType)
        {
            case 0:
                yield return StartCoroutine(SpawnLinePattern(patternLength, spacingBetweenCollectables));
                break;
            case 1:
                yield return StartCoroutine(SpawnZigzagPattern(patternLength, spacingBetweenCollectables));
                break;
        }

        yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));
    }
    private IEnumerator SpawnLinePattern(int count, float spacing)
    {
        int laneIndex = Random.Range(0, numberOfLanes);
        float spawnXPosition = (laneIndex - 1) * laneWidth;

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = new Vector3(spawnXPosition, collectableHeight, playerTransform.position.z + spawnDistanceAhead + (i * spacing));

            if (!IsObstacleNearby(spawnPosition))
            {
                GameObject collectable = GetCollectableFromPool();
                collectable.transform.position = spawnPosition;
                collectable.SetActive(true);

                StartCoroutine(ReturnToCollectablesPoolAfterDelay(collectable, collectableLifetime));
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }
    private IEnumerator SpawnZigzagPattern(int count, float spacing)
    {
        int currentLane = Random.Range(0, numberOfLanes);
        //Determine the direction of the zigzag pattern based on the current lane
        int direction = (currentLane == 0) ? 1 : ((currentLane == numberOfLanes - 1) ? -1 : Random.Range(0, 2) * 2 - 1);

        for (int i = 0; i < count; i++)
        {
            float spawnXPosition = (currentLane - 1) * laneWidth;

            Vector3 spawnPosition = new Vector3(spawnXPosition, collectableHeight, playerTransform.position.z + spawnDistanceAhead + (i * spacing));

            if (!IsObstacleNearby(spawnPosition))
            {
                GameObject collectable = GetCollectableFromPool();
                collectable.transform.position = spawnPosition;
                collectable.SetActive(true);

                StartCoroutine(ReturnToCollectablesPoolAfterDelay(collectable, collectableLifetime));
            }

            // Move to the next lane
            currentLane += direction;
            
            // Reverse direction if we hit the edge of the lanes
            if (currentLane <= 0 || currentLane >= numberOfLanes - 1)
            {
                direction *= -1;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
    private IEnumerator ReturnToCollectablesPoolAfterDelay(GameObject collectable, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (collectable != null)
        {
            collectable.SetActive(false);
            collectablePool.Enqueue(collectable);
        }
    }
    private GameObject GetCollectableFromPool()
    {
        if (collectablePool.Count == 0)
        {
            GameObject newCollectable = Instantiate(collectablePrefabs[Random.Range(0, collectablePrefabs.Length)]);
            newCollectable.SetActive(false);
            return newCollectable;
        }

        return collectablePool.Dequeue();
    }
}