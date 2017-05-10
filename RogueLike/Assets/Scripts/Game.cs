using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField]
    private LevelDebugView levelGenerator;

    [SerializeField]
    private Camera gameCamera;

    void Start()
    {
        levelGenerator.OnComplete += SpawnPlayer;
    }

    void SpawnPlayer()
    {

        Vector3 spawnPoint = levelGenerator.GetRandomWorldPointInRoom(levelGenerator.GetRandomRoom());
        print(spawnPoint);
        GameObject playerGameOject = Instantiate(playerPrefab, spawnPoint, Quaternion.identity);
        gameCamera.GetComponent<TopDownCamera>().SetTarget(playerGameOject.transform);
    }
}