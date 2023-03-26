using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnEnemy : MonoBehaviour
{
    public GameObject enemyToRespawn;

    public void RespawnTheEnemy()
    {
        Enemy enemy = enemyToRespawn.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemyToRespawn.SetActive(true);
            enemy.currentHp = (int)enemy.maxHp;
        }
    }
}
