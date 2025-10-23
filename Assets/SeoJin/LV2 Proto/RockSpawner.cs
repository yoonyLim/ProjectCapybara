using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class RockSpawner : MonoBehaviour
{
    [SerializeField] private Transform leftSpawner;
    [SerializeField] private Transform middleSpawner;
    [SerializeField] private Transform rightSpawner;

    private Vector3 leftSpawnPoint;
    private Vector3 middleSpawnPoint;
    private Vector3 rightSpawnPoint;
    

    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private float spawnInterval = 1.5f;
    
    private enum RockSet {
        Left,                //OXX
        Middle,              //XOX
        Right,               //XXO
        
        LeftMiddle,          //OOX
        LeftRight,           //OXO
        MiddleRight,         //XOO
        
        LeftMiddleRight,     //OOO
    }

    private void Awake()
    {
        leftSpawnPoint = leftSpawner.position;
        middleSpawnPoint = middleSpawner.position;
        rightSpawnPoint = rightSpawner.position;
    }

    private void Start()
    {
        StartCoroutine(SpawnRocks());
    }

    private int GetRandomRockSet()
    {
        return Random.Range(1,8);
    }

    private IEnumerator SpawnRocks()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnRock(GetRandomRockSet());
        }
    }

    private void SpawnRock(int rockSet)
    {
        bool spawnLeft = false;
        bool spawnMiddle = false;
        bool spawnRight = false;
        
        switch (rockSet)
        {
            case 1:
                spawnLeft = true;
                break;
            case 2:
                spawnMiddle = true;
                break;
            case 3:
                spawnRight = true;
                break;
            case 4:
                spawnLeft = true;
                spawnMiddle = true;
                break;
            case 5:
                spawnLeft = true;
                spawnRight = true;
                break;
            case 6:
                spawnMiddle = true;
                spawnRight = true;
                break;
            case 7:
                spawnLeft = true;
                spawnMiddle = true;
                spawnRight = true;
                break;
        }
        
        
        if (spawnLeft) Instantiate(rockPrefab, leftSpawnPoint, GetRandomQuat());
        if (spawnMiddle) Instantiate(rockPrefab, middleSpawnPoint, GetRandomQuat());
        if (spawnRight) Instantiate(rockPrefab, rightSpawnPoint, GetRandomQuat());
    }


    private Quaternion GetRandomQuat()
    {
        Quaternion quat = Quaternion.identity;
        quat.eulerAngles = new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
        return quat;
    }
}
