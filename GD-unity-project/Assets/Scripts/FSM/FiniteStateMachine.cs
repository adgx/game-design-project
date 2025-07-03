using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//Define the FSM's behavior 
public class FiniteStateMachine<T>
{
    private T _owner;
    private State _currentState;
    //we can improve it with the hash
    //collect the transition for each state
    private Dictionary<string, List<Transition>> _transitions = new Dictionary<string, List<Transition>>();
    //for the current state 
    private List<Transition> _currentTransitions = new List<Transition>();
    public FiniteStateMachine(T owner)
    {
        _owner = owner;
    }

    public void Tik()
    {
        //start the transiton forward the next state
        State nextState = GetNextState();
        if (nextState != null)
            SetState(nextState);
        
        //Do the current tasks for the currentState
        if (_currentState != null)
            _currentState.Tik();
    }

    public void SetState(State state)
    {
        if (state == _currentState)
            return;
        //lunch the behavior defined when you leave the state
        _currentState?.Exit();
        //set the new state
        _currentState = state;
        //get the new transition set
        _transitions.TryGetValue(_currentState.Name, out _currentTransitions);
        //lunch the behavior defined when you enter into the new state
        _currentState.Enter();
    }

    //the owner can define the transition from a state to another
    public void AddTransition(State fromState, State toState, Func<bool> transitionCondition)
    {
        if (_transitions.TryGetValue(fromState.Name, out var stateTransitions) == false)
        {
            stateTransitions = new List<Transition>();
            _transitions[fromState.Name] = stateTransitions;
        }

        stateTransitions.Add(new Transition(toState, transitionCondition));

    }

    private State GetNextState()
    {
        if (_currentState.Name != "Death")
        {
            if (_currentTransitions == null)
                Debug.LogError($"Current State {_currentState.Name} has NO transitions");
            else
            {
                //Verify if there is a soisfied condition so change the current state
                foreach (Transition transition in _currentTransitions)
                {
                    if (transition.Condition())
                        return transition.NextState;
                }
            }
        }

        return null;
    }
}