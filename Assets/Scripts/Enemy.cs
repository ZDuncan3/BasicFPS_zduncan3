using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public int currentHp;
    public uint maxHp = 4;

    public Slider healthSlider;

    private void Start()
    {
        if (currentHp <= 0)
        {
            currentHp = (int)maxHp;
        }

        healthSlider.value = currentHp;
        healthSlider.maxValue = (int)maxHp;
    }

    private void Update()
    {
        if (currentHp <= 0)
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            healthSlider.value = currentHp;
        }
    }
}
