using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    [SerializeField] Transform respawnPoint;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] float respawnTime;

    float respawnTimeStart;
    bool respawn;
    CinemachineVirtualCamera cinemachineVirtualCamera;

    private void Start()
    {
        cinemachineVirtualCamera = GameObject.Find("Player Camera").GetComponent<CinemachineVirtualCamera>();
    }

    private void Update()
    {
        CheckRespawn();
    }

    public void Respawn()
    {
        respawnTimeStart = Time.time;
        respawn = true;
    }

    private void CheckRespawn()
    {
        if (respawn && Time.time >= respawnTimeStart + respawnTime)
        {
            var playerTemp = Instantiate(playerPrefab, respawnPoint.position, Quaternion.Euler(0.0f, 0.0f, 0.0f));
            cinemachineVirtualCamera.m_Follow = playerTemp.transform;
            respawn = false;
        }
    }
}
