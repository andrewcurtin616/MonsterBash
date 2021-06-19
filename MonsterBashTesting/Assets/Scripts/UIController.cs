using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    GameObject healthObject;
    Image[] hearts;

    GameObject livesObject;
    Text livesText;

    GameObject scoreObject;
    Text scoreText;
    int scoreCounter;
    bool countingScore;

    Text messageTopText;
    Text messagebottomText;

    GameObject treasureObject;
    Text treasureText;

    Image fadePanel;

    public bool showHUD;

    GameManagerController GameManager;

    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        healthObject = GameObject.Find("Health");
        hearts = GameObject.Find("Health").GetComponentsInChildren<Image>();
        livesObject = GameObject.Find("Lives");
        livesText = GameObject.Find("Lives").GetComponentInChildren<Text>();
        scoreObject = GameObject.Find("Score");
        scoreText = GameObject.Find("ScoreText2").GetComponent<Text>();
        messageTopText = GameObject.Find("MessageTop").GetComponent<Text>();
        messagebottomText = GameObject.Find("MessageBottom").GetComponent<Text>();
        messagebottomText.gameObject.SetActive(false); //For now
        treasureObject = GameObject.Find("Treasure");
        treasureText = GameObject.Find("Treasure").GetComponentInChildren<Text>();
        fadePanel = GameObject.Find("FadePanel").GetComponent<Image>();

        GameManager = GameManagerController.getInstance();
        GameManager.SetUserInterface(this);
        StartCoroutine("FadeTitle");
    }

    // Update is called once per frame
    void Update()
    {

        
    }

    public void AddScore(int value)
    {
        scoreCounter += value;
        if (countingScore)
            return;
        ResetCoroutine("AddingScore");
        //ResetCoroutine("ShowScore");
        //StopCoroutine("AddingScore");
        //StopCoroutine("ShowScore");
        //StartCoroutine("ShowScore");
        //StartCoroutine("AddingScore");
    }

    void ResetCoroutine(string name)
    {
        StopCoroutine(name);
        StartCoroutine(name);
    }

    IEnumerator AddingScore()
    {
        countingScore = true;
        int temp = System.Int32.Parse(scoreText.text);
        yield return new WaitForSeconds(0.5f);
        while (scoreCounter >0)
        {
            /*** Maybe if over a certain limit, add more at once ***/
            /*** so we aren't counting up forever ***/
            temp++;
            scoreText.text = "" + temp;
            //play sound
            scoreCounter--;
             
            yield return null;
        }
        countingScore = false;
    }

    public void UpdateHearts(int health)
    {
        if (health <= 0)
            for (int i = 0; i < 5; i++)
                hearts[hearts.Length - 1 - i].fillAmount = 0;
        else
        {
            health = 10 - health;
            for (int i = 0; i < 5; i++)
                hearts[hearts.Length - 1 - i].fillAmount = 1;
            for (int i = 0; i < health/2; i++)
                hearts[hearts.Length - 1 - i].fillAmount = 0;
            if (health % 2 != 0)
                hearts[hearts.Length - 1 - health / 2].fillAmount = 0.55f;
        }       
    }

    public void UpdateLives(int lives)
    {
        int temp = System.Int32.Parse(livesText.text[2].ToString());
        Debug.Log(temp);
        if (lives > temp)
        {
            //lives increased
        }
        else if (lives < temp)
        {
            //lives decreased
            //StartCoroutine("FadeTitle");
        }

        livesText.text = "x " + lives;
    }

    public void UpdateTreasure()
    {
        int temp = System.Int32.Parse(treasureText.text[0].ToString());
        treasureText.text = (temp + 1) + " / 3";
    }

    public void PlayerDeath()
    {
        StartCoroutine("Death");
    }

    IEnumerator Death()
    {
        //wait a second for player to reflect on their poor life choices
        yield return new WaitForSeconds(4);

        //fade out
        for (float i = 0; i < 1; i += 0.05f)
        {
            Color c = fadePanel.color;
            c.a = i;
            fadePanel.color = c;
            yield return new WaitForSeconds(0.05f);
        }
        GameManager.WarpPlayer(GameManager.spawnPos);
        yield return new WaitForSeconds(1.5f);

        //fade back in
        for (float i = 1; i > 0; i -= 0.05f)
        {
            Color c = fadePanel.color;
            c.a = i;
            fadePanel.color = c;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(0.5f);
        GameManager.PlayerWakeUp();
        
        //move HUD in
        yield return new WaitForSeconds(1.5f);
        //move hud away
    }

    IEnumerator FadeTitle()
    {
        yield return new WaitForSeconds(0.75f);

        for (float i = 0; i < 1; i += 0.05f)
        {
            Color c = messageTopText.color;
            c.a = i;
            messageTopText.color = c;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(3f);

        for (float i = 1; i > 0; i -= 0.05f)
        {
            Color c = messageTopText.color;
            c.a = i;
            messageTopText.color = c;
            yield return new WaitForSeconds(0.05f);
        }
    }

    IEnumerator ShowHealth()
    {
        yield return new WaitForSeconds(0.75f);

        //loop for bring down

        yield return new WaitForSeconds(3f);

        //loop for bring up
    }

    IEnumerator ShowScore()
    {
        yield return new WaitForSeconds(0.75f);

        //loop for bring down

        while (countingScore)
            yield return null;

        yield return new WaitForSeconds(1.5f);

        //loop for bring up
    }

    IEnumerator ShowLives()
    {
        yield return new WaitForSeconds(0.75f);

        //loop for bring right

        yield return new WaitForSeconds(3f);

        //loop for bring left
    }

    IEnumerator ShowTreasure()
    {
        yield return new WaitForSeconds(0.75f);

        //loop for bring left

        yield return new WaitForSeconds(3f);

        //loop for bring right
    }

    IEnumerator ShowHUD()
    {
        showHUD = true;
        yield return new WaitForSeconds(0.5f);

        //bring HUD in

        while (showHUD)
            yield return null;

        //take out HUD
    }
}
