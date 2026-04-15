using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine;
using Unity.VisualScripting;


/*Manages inventory, keeps several component references, and any other future control of the game itself you may need*/

public class GameManager : MonoBehaviour
{
    public static GameManager Instance
    {
        get
        {
            if (instance == null) instance = GameObject.FindObjectOfType<GameManager>();
            return instance;
        }
    }
    private static GameManager instance;
    public int enemiesKilled = 0; //We will use this to determine when to spawn different monsters
    public int enemiesToKillBeforeNextWaveStarts = 50;
    public int killsFromThisWave = 0;
    public int enemyCount;
    public float itemSpawnRadius = 10f; // Adjust as needed

    [SerializeField] private List<Wave> waves = new List<Wave>(); // List of waves
    [SerializeField] private int currentWaveIndex = 0; // Index of the current sp
    public GameObject[] spawnedEnemies;
    public GameObject[] itemsToSpawn;
    // private bool hasWon = false;
    //public GameObject activateWhenWonObject;
    //[SerializeField] private GameObject[] enemyPrefabsToSpawn;
    // public static bool waveIsCleared;
    // private float timeUntilSpawn;
    // public float timeBetweenSpawns = 40;
    //public AudioSource audioSource; //A primary audioSource a large portion of game sounds are passed through
    // Singleton instantiation

    // Use this for initialization
    public void RemoveEnemy(GameObject enemy)
    {
        //enemyCount--;
        List<GameObject> tempEnemies = new List<GameObject>(spawnedEnemies);
        tempEnemies.Remove(enemy);
        spawnedEnemies = tempEnemies.ToArray();
    }

    public void Victory()
    {
        //activateWhenWonObject.SetActive(true);
        // HUD.Instance.anim.SetTrigger("victory");
    }

    public void LoadSceneCutscene(string sceneName)
    {
        //EditorSceneManager.OpenScene(sceneName);
    }

    // Method to load a scene by name in playmode
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        HUD.Instance.PlayCoverScreenAnimation();
    }

    public void Retry()
    {
        //Player.Instance.ResetStats();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void KillAllEnemies()
    {
        foreach (var enemy in spawnedEnemies)
        {
            Destroy(enemy);
        }
        spawnedEnemies = new GameObject[0];
    }

    public void SpawnEnemy()
    {
        //Debug.Log(waves.Count);
        //Debug.Log(enemiesKilled + 50);

        if (killsFromThisWave > enemiesToKillBeforeNextWaveStarts && currentWaveIndex < waves.Count)
        {
            enemiesToKillBeforeNextWaveStarts -= 10;
            StartNextWave();
        }

        if (waves.Count == 0)
        {
            //Debug.LogWarning("No waves defined!");
            return;
        }

        if (currentWaveIndex < waves.Count)
        {
            // Select a random enemy prefab from the first wave's list of enemies
            int enemyIndex = Random.Range(0, waves[currentWaveIndex].enemiesToSpawn.Count);
            GameObject enemyPrefab = waves[currentWaveIndex].enemiesToSpawn[enemyIndex];

            if (enemyPrefab == null)
            {
                //Debug.LogError("Enemy prefab is null.");
                return;
            }

            // Calculate a random position around the player within the spawn radius
            Vector2 spawnPosition = RandomPointOnCircleEdge(Player.Instance.enemySpawnRadius);
            // Instantiate the enemy at the calculated position
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
        Debug.Log("Spawned item");
        // Decide randomly whether to spawn a chest or a health potion
        GameObject itemPrefab = Random.value > 0.5 ? itemsToSpawn[Random.Range(0, itemsToSpawn.Length - 1)] : null;
        if (itemPrefab != null)
        {
            Vector2 spawnPosition = RandomPointOnCircleEdge(itemSpawnRadius);
            Instantiate(itemPrefab, spawnPosition, Quaternion.identity);
        }
    }

    Vector2 RandomPointOnCircleEdge(float radius)
    {
        // Generate a random angle between 0 to 360 degrees
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

        // Calculate the x and y coordinates using the angle and the radius
        float x = Mathf.Cos(angle) * radius;
        float y = Mathf.Sin(angle) * radius;

        // Return the position as a Vector2, offset by the player's position
        return new Vector2(Player.Instance.transform.position.x + x, Player.Instance.transform.position.y + y);
    }

    public void StartNextWave()
    {
        currentWaveIndex++;
        //Debug.Log("Wave: " + currentWaveIndex + " has begun.");
        killsFromThisWave = 0;
    }
}
