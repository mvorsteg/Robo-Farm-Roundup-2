using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WaveManager : MonoBehaviour
{

    /*  data structure used to hold an enemy's health, material, and behavior */
    private class Tuple
    {
        public int hp;
        public int type;
        public EnemyTest.Behavior behavior;
        public Material material;
        //  constructor
        public Tuple(EnemyTest.Behavior behavior, int hp, Material material, int type)
        {
            this.hp = hp;
            this.behavior = behavior;
            this.material = material;
            this.type = type;
        }

    }
    //singleton
    public static WaveManager WM;

    public GameObject[] enemyPrefabs;
    public GameObject player;
    public GameObject waveText;
    public GameObject gameOver;
    public GameObject anyKey;
    public GameObject bossBar;
    public Audio aud;
    public Material white;
    public Material yellow;
    public Material red;
    public Material blue;
    public Material boss2;
    public Material boss3;

    public Image bossBarHp;

    private Queue<Tuple> queue;

    private int numEnemy = 0;
    private int wave = 0;
    private int bossWaveCounter;
    private PlayerControls controls;
    
    private readonly int limit = 10;

    private void Awake() 
    {
        controls = new PlayerControls();
        controls.GameOverScreen.AnyKey.performed += ctx => EndGame(wave-1);
    }

    void OnDisable()
    {
        controls.GameOverScreen.Disable();
    }


    // Start is called before the first frame update
    void Start()
    {
        bossBar.SetActive(false);
        bossWaveCounter = 1;//Random.Range(3, 6);
        WM = this;
        queue = new Queue<Tuple>();
        StartCoroutine("NextWaveCoroutine");
    }

    /*  called to start the next wave by initializing the enemies that the player will fight
        and queueing them up to spawn when others die */
    void NextWave()
    {
        aud.Levelup();
        player.GetComponent<Player>().RestoreHealth();
        List<Tuple> waveList = new List<Tuple>();
        wave++;
        // logarithmic method : easier difficulty scale
        int waveCredits = 50 * (int)Mathf.Log((float)wave, 5.0f) + 50;
        StartCoroutine("WaveTitleCoroutine");
        // check for boss wave
        bossWaveCounter--;
        if (bossWaveCounter == 0)
        {
            // boss wave
            Debug.Log("Enqueueing boss");
            bossWaveCounter = Random.Range(3,6);
            int bossNum = Random.Range(1,4);
            waveList.Add(new Tuple(EnemyTest.Behavior.Passive, 10, bossNum == 3 ? boss3 : (bossNum == 2 ? boss2 : blue), bossNum));
            bossBar.SetActive(true);
            Text bossText = bossBar.GetComponentInChildren<Text>();
            bossBarHp.fillAmount = 1;
            switch(bossNum)
            {
                case 1 :
                    bossText.text = "Goliath";
                    break;
                case 2 :
                    bossText.text = "Black Decker";
                    break;
                case 3 :
                    bossText.text = "Sherman";
                    break;
            }
        }
        else
        {
            bossBar.SetActive(false);
            // regular wave
            while (waveCredits > 0)
            {
                // set up behavior
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
                waveList.Add(new Tuple(behavior, hp, mat, 0));
            }
        }
        ShuffleQueue<Tuple>(waveList, queue);
        StartCoroutine("WaveStartCoroutine");        
    }

    /*  called whenever an enemy dies to spawn a new one */
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

    /*  instantiate the next enemy in the queue in a random location 
        that is not too close to the player */
    private void SpawnEnemy(Tuple Tuple)
    {
        int hp = Tuple.hp;
        int type = Tuple.type;
        EnemyTest.Behavior behavior = Tuple.behavior;
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
        GameObject enemy = Instantiate(enemyPrefabs[type], pos, Quaternion.Euler(0, Random.Range(0,360), 0));
        enemy.GetComponent<Enemy>().Initialize(behavior, hp);
        enemy.GetComponentsInChildren<Renderer>()[0].material = Tuple.material;
        numEnemy++;
    }

    /*  randomizes the order that the enemies will spawn */
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

    /*  spawns the enemies at the beginning of a wave with a delay between them */
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

    /*  pulls the title across the screen at the beginning of a wave */
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

    /*  gives a delay after the previous wave ends until the next one starts */
    IEnumerator NextWaveCoroutine()
    {
        yield return new WaitForSeconds(3f);
        NextWave();
    }

    /*  ends the game and shows the ending text */
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
        controls.GameOverScreen.Enable();
    }

    /*  switches to the leaderboard scene */
    public void EndGame(int score)
    {
        UnlockHats();
        MainMenu.newScore = score;
        MainMenu.leaderboardMode = true;
        SceneManager.LoadScene(0);
    }

    /*  unlocks certain cosmetic options for the player */
    public void UnlockHats()
    {
        int unlockedHats = PlayerPrefs.GetInt("unlockedHats");
        // cowboy hat
        if (wave == 3)
            unlockedHats = unlockedHats | 1 << 0;
        // sleepy hat
        else if (wave == 5)
            unlockedHats = unlockedHats | 1 << 1;
        // top hat
        else if (wave == 10)
            unlockedHats = unlockedHats | 1 << 2;
        // crown
        else if (wave == 20)
            unlockedHats = unlockedHats | 1 << 3;
        // sombrero
        if (PlayerPrefs.GetInt("numBoss1Defeated") > 0)
            unlockedHats = unlockedHats | 1 << 4;
        // mining helmet
        if (PlayerPrefs.GetInt("numBoss2Defeated") > 0)
            unlockedHats = unlockedHats | 1 << 5;
        // police hat
        if (PlayerPrefs.GetInt("numBoss3Defeated") > 0)
            unlockedHats = unlockedHats | 1 << 6;
        // shower cap
        if (PlayerPrefs.GetInt("numWhiteEnemiesDefeated") > 1000)
            unlockedHats = unlockedHats | 1 << 7;
        // circle hat
        if (PlayerPrefs.GetInt("numYellowEnemiesDefeated") > 500)
            unlockedHats = unlockedHats | 1 << 8;
        // viking hat
        if (PlayerPrefs.GetInt("numRedEnemiesDefeated") > 250)
            unlockedHats = unlockedHats | 1 << 9;
        PlayerPrefs.SetInt("unlockedHats", unlockedHats);
    }
}
