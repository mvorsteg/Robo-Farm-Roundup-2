using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WaveManager : MonoBehaviour
{

    private class Pair
    {
        public int hp;
        public EnemyTest.Behavior behavior;
        public Material material;

        public Pair(EnemyTest.Behavior behavior, int hp, Material material)
        {
            this.hp = hp;
            this.behavior = behavior;
            this.material = material;
        }

    }
    //singleton
    public static WaveManager WM;
    public GameObject enemyPrefab;
    public GameObject player;
    private Queue<Pair> queue;
    private int numEnemy;
    private int wave;
    public GameObject waveText;
    public Material white;
    public Material yellow;
    public Material red;
    public GameObject gameOver;
    public GameObject anyKey;
    public Audio aud;

    private readonly int limit = 10;

    // Start is called before the first frame update
    void Start()
    {
        WM = this;
        queue = new Queue<Pair>();
        numEnemy = 0;
        wave = 0;
        StartCoroutine("NextWaveCoroutine");
    }

    void NextWave()
    {
        aud.Levelup();
        player.GetComponent<Player>().RestoreHealth();
        List<Pair> waveList = new List<Pair>();
        wave++;
        // linear method : difficulty scales too fast
        // int waveCredits = 25 * (wave - 1) + 50;
        // logarithmic method : easier difficulty scale
        int waveCredits = 1;//50 * (int)Mathf.Log((float)wave, 5.0f) + 50;
        StartCoroutine("WaveTitleCoroutine");
        while (waveCredits > 0)
        {
            //set up behavior
            int roll = Random.Range(1,100);
            EnemyTest.Behavior behavior;
            if (roll > 85 && waveCredits > 4)
            {
                behavior = EnemyTest.Behavior.Charge;
                waveCredits -= 5;
            }
            else if (roll > 70 && waveCredits > 3)
            {
                behavior = EnemyTest.Behavior.Smart;
                waveCredits -= 4;
            }
            else if (roll > 55 && waveCredits > 3)
            {
                behavior = EnemyTest.Behavior.Orbit;
                waveCredits -= 4;
            }
            else if (roll > 30 && waveCredits > 2)
            {
                behavior = EnemyTest.Behavior.Fast;
                waveCredits -= 3;
            }
            else
            {
                behavior = EnemyTest.Behavior.Slow;
                waveCredits -= 1;
            }
            //set bonus hp
            int hp = 1;
            roll = Random.Range(1,100);
            Material mat = white;
            if (roll > 90 && waveCredits > 1)
            {
                hp = 3;
                waveCredits -= 2;
                mat = red;
            }
            else if (roll > 75 && waveCredits > 0)
            {
                hp = 2;
                waveCredits -= 1;
                mat = yellow;
            }
            waveList.Add(new Pair(behavior, hp, mat));
        }
        ShuffleQueue<Pair>(waveList, queue);
        StartCoroutine("WaveStartCoroutine");
    }

    public void InformDeath()
    {
        numEnemy--;
        if (queue.Count > 0)
        {
            SpawnEnemy(queue.Dequeue());
        }
        else if (numEnemy <= 0)
        {
            StartCoroutine("NextWaveCoroutine");
        }
    }

    private void SpawnEnemy(Pair pair)
    {
        int hp = pair.hp;
        EnemyTest.Behavior behavior = pair.behavior;
        Vector3 playerPos = player.GetComponent<Transform>().position;
        Vector3 pos = new Vector3(0,2,0);
        bool foundPos = false;
        UnityEngine.AI.NavMeshHit hit;
        while (!foundPos)
        {
            Vector3 randomPos = new Vector3(Random.Range(-45, 45), 2, Random.Range(-45, 45));
            foundPos = UnityEngine.AI.NavMesh.SamplePosition(randomPos, out hit, 2f, UnityEngine.AI.NavMesh.AllAreas);
            pos = hit.position;
            if (Vector3.Distance(playerPos, pos) < 5f)
            {
                foundPos = false;
            }
        }
        //Debug.Log(pos);
        GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.Euler(0, Random.Range(0,360), 0));
        enemy.GetComponent<EnemyTest>().Initialize(behavior, hp);
        enemy.GetComponentsInChildren<Renderer>()[0].material = pair.material;
        numEnemy++;
    }

    private void ShuffleQueue<T>(List<T> lst, Queue<T> queue)
    {
        
        for (int i = lst.Count-1; i >= 0; i--)
        {
            int r = Random.Range(0,i);
            T temp = lst[r];
            lst.RemoveAt(r);
            queue.Enqueue(temp);
        }
    }

    IEnumerator WaveStartCoroutine()
    {
        yield return new WaitForSeconds(1);
        for (int i = 0; i < limit; i++)
        {
            if (queue.Count > 0)
            {
                SpawnEnemy(queue.Dequeue());
                yield return new WaitForSeconds(2F); 
            }
        }
    }

    IEnumerator WaveTitleCoroutine()
    {
        waveText.SetActive(true);
        waveText.GetComponent<Text>().text = "Wave " + wave;
        RectTransform t = waveText.GetComponent<RectTransform>();
        t.localPosition = new Vector3(-560, 0, 0);
        Vector3 offset = new Vector3(10, 0, 0);
        for(int i = 0; i < 56; i++)
        {
            t.localPosition += offset;
            yield return new WaitForSeconds(0.001f);
        }
        yield return new WaitForSeconds(1.5f);
        for(int i = 0; i < 56; i++)
        {
            t.localPosition += offset;
            yield return new WaitForSeconds(0.001f);
        }
        waveText.SetActive(false);
    }

    IEnumerator NextWaveCoroutine()
    {
        yield return new WaitForSeconds(3f);
        NextWave();
    }

    IEnumerator GameOverCoroutine()
    {
        gameOver.SetActive(true);
        if (wave == 2)
        {
            gameOver.transform.GetChild(1).GetComponent<Text>().text = "You Survived 1 Wave";
        }
        else
        {
            gameOver.transform.GetChild(1).GetComponent<Text>().text = "You Survived " + (wave-1) + " Waves";
        }
        RectTransform t = gameOver.GetComponent<RectTransform>();
        t.localPosition = new Vector3(-600, 0, 0);
        Vector3 offset = new Vector3(10, 0, 0);

        for(int i = 0; i < 60; i++)
        {
            t.localPosition += offset;
            yield return new WaitForSeconds(0.001f);
        }
        yield return new WaitForSeconds(2.0f);
        anyKey.SetActive(true);
        while(!Input.anyKey)
        {
            yield return null;
        }
        EndGame(wave-1);
    }

    public void EndGame(int score)
    {
        MainMenu.newScore = score;
        MainMenu.leaderboardMode = true;
        SceneManager.LoadScene(0);
    }
}
