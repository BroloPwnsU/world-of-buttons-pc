using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ResolutionPanel : GamePanel
{
    private Text _victorText;

    void Awake()
    {
        _victorText = gameObject.GetComponentInChildren<Text>(true);
    }

    public void SetVictor(RoundResult roundResult)
    {
        this.Show();

        string sText = "Who won? I don't know!";
        if (roundResult.Victor == BattleVictor.Boss)
        {
            float randy = Random.Range(0, 5);
            if (randy < 1)
            {
                sText = "You win!";
            }
            else if (randy < 2)
            {
                sText = "Death rains from a button! You win!";
            }
            else if (randy < 3)
            {
                sText = "You are the Button Master!";
            }
            else if (randy < 4)
            {
                sText = "You win! Now do it again!";
            }
            else
            {
                sText = "Let's turn up the difficulty. You are too good.";
            }
        }
        else if(roundResult.Victor == BattleVictor.Boss)
        {
            float randy = Random.Range(0, 5);
            if (randy < 1)
            {
                sText = "The BIG BAD BOSS defeated you. I am ashamed. Please try harder next time.";
            }
            else if(randy < 2)
            {
                sText = "The Boss is not impressed. Can you do better?";
            }
            else if (randy < 3)
            {
                sText = "You were not prepared.";
            }
            else if(randy < 4)
            {
                sText = "If I were the Boss, I would just send my minions to defeat you. You are not worthy.";
            }
            else
            {
                sText = "You Lose! Hahahahahahaha! Ok next round.";
            }
        }
        else if (roundResult.Victor == BattleVictor.Party1)
        {
            float randy = Random.Range(0, 5);
            if (randy < 1)
            {
                sText = "Blue Team told me a secret: they think Green Team sucks. And they are right.";
            }
            else if (randy < 2)
            {
                sText = "Green Team is handsome, pretty, and smart. But they suck at this game.";
            }
            else if (randy < 3)
            {
                sText = "Blue Team is the best! Let's have another round, shall we?";
            }
            else if (randy < 4)
            {
                sText = "Green Team is noobz. GG EZ.";
            }
            else
            {
                sText = "Blue Team wins. Cool beans.";
            }
        }
        else if(roundResult.Victor == BattleVictor.Party2)
        {
            float randy = Random.Range(0, 5);
            if (randy < 1)
            {
                sText = "Green Team is so cool. Gosh, I wish I was that cool.";
            }
            else if (randy < 2)
            {
                sText = "Blue Team is a bunch of failures. Get better, scrubs.";
            }
            else if (randy < 3)
            {
                sText = "Green Team whooped up on Blue Team.";
            }
            else if (randy < 4)
            {
                sText = "Green Team's power level is over 9000!!!!!!!";
            }
            else
            {
                sText = "Grats Green Team. Scintillating stuff. /slowclap";
            }
        }

        _victorText.text = sText;
    }
}
