using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Enemy : MonoBehaviour
{
    int currentHealth;
    public int maxHealth;

    void Awake()
    {
        currentHealth = maxHealth;
    }
    void Update()
    {
        
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            killEnemy();
        }
    }

    void killEnemy()
    {
        Destroy(gameObject);
    }
}
