using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Player Stats", menuName = "ScriptableObjects/PlayerStats", order = 2)]
public class PlayerStats : ScriptableObject
{
    public float dashSpeed = 1f;
    public float dashCooldown = 2f; // Seconds
    public float moveSpeed = 1.3f;
    public int currentHealth = 100;
    public int maxHealth = 100;
    public int damage = 1;
    public int defense = 1;
    public int level = 1;
    public int currentExp = 0;
    public int expNeededToLevelUp = 220;
    public int gold = 0;


}

