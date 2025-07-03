using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Used to define the FSM's state
public abstract class State
{
    private string _name;

    public string Name => _name;

    protected State(string name)
    {
        _name = name;
    }
    //Do something when you enter in the state
    public abstract void Enter();
    //Update
    public abstract void Tik();
    //Do something when you leave the state
    public abstract void Exit();
}