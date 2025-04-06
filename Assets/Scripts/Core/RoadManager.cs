using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class RoadManager : MonoBehaviour
{
    #region Fields

    [Header("Road Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameObject roadPrefab;
    [SerializeField] private float roadLength = 30f;
    [SerializeField] private int initialRoadCount = 5;
    [SerializeField] private int activeRoadCount = 7; // Maximum active road segments at once
    
    [Header("Environment Settings")]
    [SerializeField] private GameObject[] environmentPrefabs;
    [SerializeField] private float environmentSpawnChance = 0.5f;
    [SerializeField] private float environmentPropsSpacing = 15f;
    [SerializeField] private float environmentPropsOffsetRange = 20f;

    // Object pooling
    private Queue<GameObject> roadPool = new Queue<GameObject>();
    private Queue<GameObject> environmentPool = new Queue<GameObject>();
    private int environmentPoolSize = 20;
    
    // Road tracking
    private List<GameObject> activeRoads = new List<GameObject>();
    private float spawnPosition = 0f;
    private float despawnThresholdRange = -30f;
    
    private float laneWidth = 3f;

    #endregion

    private void Start()
    {
        InitializeEnvironmentPools();
        SpawnInitialRoadsSegment();
    }
    private void InitializeEnvironmentPools()
    {
        // Initialize road segments pool
        for (int i = 0; i < activeRoadCount + 4; i++)
        {
            GameObject road = Instantiate(roadPrefab);
            road.SetActive(false);
            roadPool.Enqueue(road);
        }

        // Initialize environment objects pool
        for (int i = 0; i < environmentPoolSize; i++)
        {
            int prefabIndex = Random.Range(0, environmentPrefabs.Length);
            GameObject environment = Instantiate(environmentPrefabs[prefabIndex]);
            environment.SetActive(false);
            environmentPool.Enqueue(environment);
        }
    }
    private void SpawnInitialRoadsSegment()
    {
        for (int i = 0; i < initialRoadCount; i++)
        {
            SpawnNewRoadSegment();
        }
    }
    private void Update()
    {
        
        if (playerTransform.position.z + roadLength * 12 > spawnPosition)
        {
            SpawnNewRoadSegment();
        }

        // Check if we need to despawn old road segments
        CheckForRoadSegmentDespawn();
    }
    private void SpawnNewRoadSegment()
    {
        GameObject road = GetRoadFromPool();
        //First spawnPosition is 0, Then it updates
        road.transform.position = new Vector3(0, 0, spawnPosition);
        road.SetActive(true);
        activeRoads.Add(road);
        //Updating RoadPositions
        spawnPosition += roadLength;
        SpawnEnvironmentPropsAroundRoad(spawnPosition - roadLength);
    }
    private void SpawnEnvironmentPropsAroundRoad(float startRoadPosZ)
    {
        // Calculate the number of environment objects to spawn
        int environmentCount = Mathf.FloorToInt(roadLength / environmentPropsSpacing);
        
        for (int i = 0; i < environmentCount; i++)
        {
            // Randomly decide whether to spawn an environment object
            if (Random.value > environmentSpawnChance) continue;
            
            float roadSpawnZPos = startRoadPosZ + i * environmentPropsSpacing + Random.Range(0f, environmentPropsSpacing * 0.5f);
            
            // Randomly choose left or right side of the road and Avoid the road segment
            float roadDirection = (Random.value > 0.5f) ? 1f : -1f;
            float roadSideXPos = roadDirection * (laneWidth + Random.Range(2f, environmentPropsOffsetRange));
            
            GameObject environmentPropsObj = GetEnvironmentFromPool();
            environmentPropsObj.transform.position = new Vector3(roadSideXPos, 0, roadSpawnZPos);
            environmentPropsObj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            environmentPropsObj.SetActive(true);
            
            // Return to pool after the player passes by with some buffer
            StartCoroutine(ReturnEnvironmentPropsToPoolAfterDelay(environmentPropsObj, 
                (roadSpawnZPos - playerTransform.position.z) / 5f + 5f));
        }
    }
    private GameObject GetRoadFromPool()
    {
        // If pool is empty create a new road segment
        if (roadPool.Count == 0)
        {
            GameObject newRoad = Instantiate(roadPrefab);
            newRoad.SetActive(false);
            return newRoad;
        }
        // Get an existing road segment from the pool
        return roadPool.Dequeue(); 
    }
    private GameObject GetEnvironmentFromPool()
    {
        // If pool is empty, create a new environment object
        if (environmentPool.Count == 0)
        {
            int prefabIndex = Random.Range(0, environmentPrefabs.Length);
            GameObject newEnvironment = Instantiate(environmentPrefabs[prefabIndex]);
            newEnvironment.SetActive(false);
            return newEnvironment;
        }
        // Get an existing environment object from the pool
        return environmentPool.Dequeue();
    }
    private IEnumerator ReturnEnvironmentPropsToPoolAfterDelay(GameObject environmentObj, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (environmentObj != null)
        {
            environmentObj.SetActive(false);
            environmentPool.Enqueue(environmentObj);
        }
    }
    private void CheckForRoadSegmentDespawn()
    {
        // Check if any road segments are behind the player by enough distance to despawn
        //Leave last one road segment active
        for (int i = activeRoads.Count - 1; i >= 0; i--) 
        {
            if (activeRoads[i].transform.position.z < playerTransform.position.z + despawnThresholdRange)
            {
                GameObject road = activeRoads[i];
                activeRoads.RemoveAt(i);
                
                road.SetActive(false);
                roadPool.Enqueue(road);
            }
        }
    }
}