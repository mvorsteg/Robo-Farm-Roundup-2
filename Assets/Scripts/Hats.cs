using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hats : MonoBehaviour
{

    public GameObject[] hats;
    public Material lockedMat;

    private Material[] mats;

    private int currentHat;

    // Start is called before the first frame update
    void Start()
    {
        currentHat = PlayerPrefs.GetInt("hat");
        if (currentHat > 0)
            hats[currentHat-1].SetActive(true);
    }

    void OnDisable()
    {
        // if the last hat looked at was not unlocked
        if (hats != null && currentHat > 0 && (PlayerPrefs.GetInt("unlockedHats") & (1 << currentHat-1)) == 0)
        {   
            hats[currentHat-1].SetActive(false);
            currentHat = PlayerPrefs.GetInt("hat");
            if (currentHat != 0)
                hats[currentHat-1].SetActive(true);
        }
    }

    public void NextHat()
    {
        // check to see if previous hat had been locked
        if (currentHat != 0 && (PlayerPrefs.GetInt("unlockedHats") & (1 << currentHat-1)) == 0)
        {    
            RestoreHat();
        }
        if (currentHat > 0)
            hats[currentHat-1].SetActive(false);
        currentHat++;
        if (currentHat > hats.Length)
        {
            currentHat = 0;
            PlayerPrefs.SetInt("hat", currentHat);
        }
        if (currentHat > 0)
        {
            hats[currentHat-1].SetActive(true);
            // check if unlocked
            if ((PlayerPrefs.GetInt("unlockedHats") & (1 << currentHat-1)) != 0)
            {    
                PlayerPrefs.SetInt("hat", currentHat);
            }
            else
            {
                Debug.Log("LOCKED"+PlayerPrefs.GetInt("unlockedHats"));
                int len = hats[currentHat-1].GetComponent<Renderer>().materials.Length;
                Material[] lockedMats = new Material[len];
                for (int i=0; i<len; i++)
                {
                    lockedMats[i] = lockedMat;
                }
                mats = hats[currentHat-1].GetComponent<Renderer>().materials;
                hats[currentHat-1].GetComponent<Renderer>().materials = lockedMats;
            }
        }
    }

    /*  changes a hat's colors back to normal after being locked */ 
    private void RestoreHat()
    {
        hats[currentHat-1].GetComponent<Renderer>().materials =  mats;
    }
}
