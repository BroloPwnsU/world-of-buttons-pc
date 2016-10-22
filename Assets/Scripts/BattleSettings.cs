using UnityEngine;
using System;
using System.Collections;

public class BattleSettings
{
    public int VictoriesNeededToWin = 1;
    public int Party1CurrentVictories = 0;
    public int Party2CurrentVictories = 0;

    public bool BossFight;
    public float PVEStartHealth;
    public float PVPStartHealth;
    public float BossStartHealth;

    public float InitialActiveScreenSeconds;
    public float MinimumActiveScreenSeconds;
    public float ActiveScreenScalingFactor;
    public float BuffIncreaseActiveTimeMultiplier;
    public float BossMinimumDamagePerAttack;
    public float BossMaximumDamagePerAttack;
    public float PlayerMinimumDamagePerAttack;
    public float PlayerMaximumDamagePerAttack;
    public float BuffParty1CritChance;
    public float BuffParty2CritChance;

    public Action<RoundResult> EndRoundNotification;
}

public class RoundResult
{
    public BattleVictor Victor;
}
