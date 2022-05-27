using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo
{
    string name;
    int money;

    public string Name => name;
    public int Money => money;

    public PlayerInfo(string name)
    {
        this.name = name;
        money = 0;
    }
}
