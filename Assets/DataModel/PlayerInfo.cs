using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo
{
    string name;
    int money;

    bool canAnswer = false;

    public string Name => name;
    public int Money => money;

    public bool CanAnswer => canAnswer;

    public PlayerInfo(string name)
    {
        this.name = name;
        money = 0;
    }

    public void AddMoney(int value)
    {
        money += value;
    }

    public void SetCanAnswer(bool can)
    {
        canAnswer = can;
    }
}
