using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerController
{
    public static GameManagerController instance = null;

    PlayerController player;
    UIController userInterface;

    public Vector3 spawnPos;//changes with checkpoints

    public static GameManagerController getInstance()
    {
        if (instance == null)
            instance = new GameManagerController();

        return instance;
    }

    public void SetPlayer(PlayerController player)
    {
        this.player = player;
        if (this.player != null)
            Debug.Log("player set");
        else
            Debug.Log("Error: player not set");
    }

    public void SetUserInterface(UIController userInterface)
    {
        this.userInterface = userInterface;
        if (this.userInterface != null)
            Debug.Log("userInterface set");
        else
            Debug.Log("Error: userInterface not set");
    }



    //////////////////////////***Start Methods***//////////////////////////

    public void UpdateHealth()
    {
        userInterface.UpdateHearts(player.health);
        if (player.health <= 0)
            PlayerDeath();
    }

    public void UpdateScore(int score)
    {
        userInterface.AddScore(score);
    }

    public void PlayerDeath()
    {
        if(player.lives < 0)
        {
            //game over
        }
        else
        {
            userInterface.PlayerDeath();
        }
    }

    public void WarpPlayer(Vector3 position)
    {
        if (player.health <= 0)
        {
            player.health = 10;
        }
        player.transform.position = position;
        userInterface.UpdateHearts(player.health);
        Camera.main.transform.position = Camera.main.transform.position - position;
    }

    public void PlayerWakeUp()
    {
        player.GetUp();
        userInterface.UpdateLives(player.lives);
    }

    public void PauseGame()
    {

    }

    public void UnpauseGame()
    {

    }

    public void UpdateTreasure()
    {
        userInterface.UpdateTreasure();
    }
}
