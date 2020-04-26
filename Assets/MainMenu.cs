using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using TMPro;

public class MainMenu : MonoBehaviour
{

    public class Score
    {
        int points;
        string name;

        public Score(int pts, string nm)
        {
            points = pts;
            name = nm;
        }

        public int getPoints()
        {
            return points;
        }

        public string getName()
        {
            return name;
        }
    }

    public GameObject menu;
    public GameObject leaderboard;
    public static int newScore;
    public static bool leaderboardMode;
    public Score[] highScores;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI scoreText2;
    public GameObject input;
    public Text button;
    public TextMeshProUGUI tips;
    public TextMeshProUGUI tips2;
    public AudioMixer mix;
    public AudioSource click;
    public GameObject dancer;
    public GameObject dancer2;

    //34 tips total
    private string[] tipArray = {"A real cowboy eats oats on Sunday", "Rolling into an enemy deals damage", "Yellow enemies have double health, red ones have triple", 
    "Don't fall off the edge of the world!", "Enemies track you around obstacles", "Sometimes the chicken follows you! It doesn't do anything useful",
    "If you click the mouse twice, you perform a double punch", "You regenerate your health when the wave ends", "Each wave can be harder than the last",
    "Don't dodge too often or you might deplete your fuel bar", "Jumping backwards costs half as much fuel as rolling forwards",
    "A bird in the hand is worth 2 in the bush", "Traffic is lighter on the weekends", "Don't let yourself get surrounded!", "Also try Tic Tac Toe Ultimate",
    "For some reason, it is fastest to walk diagonally", "There will be more tough enemies on later rounds", "You are invincible while rolling or jumping",
    "Moss points to town", "It's legal to hunt coyotes in most states", "I do not know why they call it a 10 gallon hat", "Everything is bigger in Texas",
    "If you get close to the edge, roll back to safety!", "Real farmers are hard working people", "It takes 2 to tango but only 1 to cha cha slide",
    "There's no such thing as a jackalope", "You only damage an enemy if they touch your fist", "White enemies will die in 1 hit", "You can only take 4 hits befor dying",
    "Never let your guard down", "Be sure to check behind you occasionally", "Never give up", "It's 5:00 somewhere", "Be sure to eat your fruit and veggies"};

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (leaderboardMode)
        {
            menu.SetActive(false);
            leaderboard.SetActive(true);
            Leaderboard();
        }
        else
        {
            StartCoroutine("EasterEggCoroutine");
            menu.SetActive(true);
            leaderboard.SetActive(false);
        }
    }

    public void ClickSound()
    {
        click.Play();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SetMusic(float volume)
    {
        mix.SetFloat("musicVol", volume);
    }
    public void SetVolume(float volume)
    {
        mix.SetFloat("soundVol", volume);
    }

    public void LeaderboardPreview()
    {
        tips2.text = tipArray[Random.Range(0,34)];
        highScores = new Score[10];
        Load();
        scoreText2.text = DisplayScores();
    }

    public void Leaderboard()
    {
        tips.text = tipArray[Random.Range(0,34)];
        highScores = new Score[10];
        Load();
        if (highScores[9].getName() != "" && highScores[9].getPoints() >= newScore)
        {
            input.SetActive(false);
            button.text = "Menu";
        }
        else
        {
            //sadMusic.GetComponent<Music2>().StopMusic();
            //happyMusic.GetComponent<Music>().PlayMusic();
        }
        //wrangler.text = "You Wrangled\n" + newScore + " bots!";
        scoreText.text = DisplayScores();

    }

    void Load()
    {
        for (int i=0; i<10; i++)
        {
            highScores[i] = new Score(PlayerPrefs.GetInt("HSPoints" + i),PlayerPrefs.GetString("HSName" + i));
        }
    }

    void Save()
    {
        for(int i=0; i<10; i++)
        {
            PlayerPrefs.SetInt("HSPoints" + i, highScores[i].getPoints());
            PlayerPrefs.SetString("HSName" + i, highScores[i].getName());
        }
    }

    public void AddScore()
    {
        int score = newScore;
        string name = input.GetComponent<Transform>().GetChild(1).GetComponent<Text>().text;
        if (name.Length < 1)
            name = "anonymous";
        else if (name.Length > 16)
            name = name.Substring(0, 15);
        bool search = true;
        int i = 0;
        Score[] newScores = new Score[10];
        
        while (i < 10 && search)
        {
            if (highScores[i].getName() == "") 
            {
                newScores[i] = new Score(score, name);
                search = false;
                i++;
                while (i < 10)
                {
                    newScores[i] = new Score(0,"");
                    i++;
                }

            }
            else if (score > highScores[i].getPoints())
            {
                search = false;
                newScores[i] = new Score(score, name);
                i++;
                while (i < 10)
                {
                    newScores[i] = highScores[i - 1];
                    i++;
                }
            }
            else
                newScores[i] = highScores[i];
            i++;
        }
        highScores = newScores;
        Save();
        menu.SetActive(true);
        leaderboard.SetActive(false);
        StartCoroutine("EasterEggCoroutine");
    }

    public string DisplayScores()
    {
        string highScoreString = "";
        for(int i=0; i<10; i++)
        {
            highScoreString += i+1;
            if (i < 9)
                highScoreString += " ";
            highScoreString += ": ";
            if (highScores[i].getName() != "")
            {
                highScoreString += highScores[i].getName() + " " + highScores[i].getPoints();
            }
            highScoreString += "\n";
        }
        return highScoreString;
    }

    public void ResetScores()
    {
        for (int i = 0; i < 10; i++)
        {
            PlayerPrefs.SetInt("HSPoints" + i, 0);
            PlayerPrefs.SetString("HSName" + i, "");
        }
    }

    IEnumerator EasterEggCoroutine()
    {
        yield return new WaitForSeconds(45f);
        Transform t = dancer.GetComponent<Transform>();
        t.position = new Vector3(960, -50, -100);
        Vector3 offset = new Vector3(3, 0, 0);
        for(int i = 0; i < 136; i++)
        {
            t.position -= offset;
            yield return new WaitForSeconds(0.001f);
        }
        yield return new WaitForSeconds(10f);
        t = dancer2.GetComponent<Transform>();
        t.position = new Vector3(-960, -50, -100);
        for(int i = 0; i < 136; i++)
        {
            t.position += offset;
            yield return new WaitForSeconds(0.001f);
        }
    }

}
