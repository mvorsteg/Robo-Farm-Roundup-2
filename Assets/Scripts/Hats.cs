using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hats : MonoBehaviour
{

    public GameObject[] hats;
    public int currentHat;

    // Start is called before the first frame update
    void Start()
    {
        currentHat = PlayerPrefs.GetInt("hat");
        if (currentHat > 0)
            hats[currentHat-1].SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NextHat()
    {
        if (currentHat > 0)
            hats[currentHat-1].SetActive(false);
        currentHat++;
        if (currentHat > hats.Length)
        {
            currentHat = 0;
        }
        if (currentHat > 0)
            hats[currentHat-1].SetActive(true);
        PlayerPrefs.SetInt("hat", currentHat);
    }
}
