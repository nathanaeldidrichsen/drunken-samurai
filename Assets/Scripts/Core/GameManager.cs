using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<GameManager>();
            return instance;
        }
    }
    private static GameManager instance;

    [Header("Enemy Spawning")]
    public int enemiesKilled;
    public int enemiesToKillBeforeNextWaveStarts = 50;
    public int killsFromThisWave;
    public float itemSpawnRadius = 10f;

    [SerializeField] private List<Wave> waves = new List<Wave>();
    [SerializeField] private int currentWaveIndex;
    public GameObject[] spawnedEnemies;
    public GameObject[] itemsToSpawn;

    [Header("Well")]
    public int wellHealCost = 5;
    public SoundData wellHealSound;

    public void HealFromWell()
    {
        var player = Player.Instance;
        if (player == null) return;

        if (player.stats.currentHealth >= player.stats.maxHealth)
        {
            HUD.Instance?.ShowFeedback("Already at full health!");
            return;
        }

        if (player.stats.gold < wellHealCost)
        {
            HUD.Instance?.ShowFeedback("Not enough gold!");
            return;
        }

        player.SpendGold(wellHealCost);
        player.stats.currentHealth = player.stats.maxHealth;
        SoundManager.Instance?.PlaySFX(wellHealSound);
        HUD.Instance?.ShowFeedback("Healed to full!");
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        HUD.Instance.PlayCoverScreenAnimation();
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SpawnEnemy()
    {
        if (killsFromThisWave > enemiesToKillBeforeNextWaveStarts && currentWaveIndex < waves.Count)
        {
            enemiesToKillBeforeNextWaveStarts -= 10;
            StartNextWave();
        }

        if (waves.Count == 0) return;

        if (currentWaveIndex < waves.Count)
        {
            int enemyIndex = Random.Range(0, waves[currentWaveIndex].enemiesToSpawn.Count);
            GameObject enemyPrefab = waves[currentWaveIndex].enemiesToSpawn[enemyIndex];
            if (enemyPrefab == null) return;

            Vector2 spawnPosition = RandomPointOnCircleEdge(Player.Instance.enemySpawnRadius);
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            currentWaveIndex = 0;
            enemiesToKillBeforeNextWaveStarts = 50;
        }
    }

    public void SpawnItem()
    {
        GameObject itemPrefab = Random.value > 0.5f ? itemsToSpawn[Random.Range(0, itemsToSpawn.Length - 1)] : null;
        if (itemPrefab == null) return;

        Vector2 spawnPosition = RandomPointOnCircleEdge(itemSpawnRadius);
        Instantiate(itemPrefab, spawnPosition, Quaternion.identity);
    }

    private Vector2 RandomPointOnCircleEdge(float radius)
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        return new Vector2(
            Player.Instance.transform.position.x + Mathf.Cos(angle) * radius,
            Player.Instance.transform.position.y + Mathf.Sin(angle) * radius
        );
    }

    private void StartNextWave()
    {
        currentWaveIndex++;
        killsFromThisWave = 0;
    }
}
