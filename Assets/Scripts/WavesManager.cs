using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WavesManager : MonoBehaviour
{
    public static WavesManager Instance;

    [Header("Waves")]
    public List<Wave> waves;

    [Header("Spawn Settings")]
    public Transform spawnCenter;
    public float spawnRadius = 5f;

    [Header("Events")]
    public UnityEvent<int> onWaveStarted;
    public UnityEvent<int> onWaveCompleted;

    [Header("Debug (Read Only)")]
    [SerializeField] private int aliveCount;
    [SerializeField] private int currentWaveIndex = -1;
    [SerializeField] private bool waveActive;

    void Awake()
    {
        Instance = this;
    }

    public void StartWave(int waveIndex)
    {
        if (waveIndex < 0 || waveIndex >= waves.Count)
        {
            Debug.LogWarning($"WavesManager: Wave index {waveIndex} is out of range.");
            return;
        }

        if (waveActive)
        {
            Debug.LogWarning("WavesManager: A wave is already active.");
            return;
        }

        currentWaveIndex = waveIndex;
        Wave wave = waves[waveIndex];
        aliveCount = 0;
        waveActive = true;

        for (int i = 0; i < wave.spawnAmount; i++)
        {
            if (wave.enemiesToSpawn == null || wave.enemiesToSpawn.Count == 0) break;

            GameObject prefab = wave.enemiesToSpawn[Random.Range(0, wave.enemiesToSpawn.Count)];
            if (prefab == null) continue;

            Vector2 spawnPos = (Vector2)(spawnCenter != null ? spawnCenter.position : Vector3.zero)
                               + Random.insideUnitCircle * spawnRadius;
            GameObject go = Instantiate(prefab, spawnPos, Quaternion.identity);
            var enemy = go.GetComponent<EnemyBase>() ?? go.GetComponentInChildren<EnemyBase>();
            enemy?.RegisterToWave(this);
            aliveCount++;
        }

        onWaveStarted?.Invoke(currentWaveIndex);
        Debug.Log($"Wave {waveIndex + 1} started with {aliveCount} enemies.");
    }

    public void OnEnemyDied()
    {
        if (!waveActive) return;

        aliveCount = Mathf.Max(0, aliveCount - 1);
        Debug.Log($"Enemy died. Alive: {aliveCount}");
        if (aliveCount <= 0)
        {
            waveActive = false;
            aliveCount = 0;
            Debug.Log($"Wave {currentWaveIndex + 1} completed!");
            onWaveCompleted?.Invoke(currentWaveIndex);
        }
    }
}