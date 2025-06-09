/*
using System;
using UnityEngine;

namespace DissolveEffect { 
public class EventSystem : MonoBehaviour
{
    private static EventSystem _instance;
    public static EventSystem Instance => _instance;
    public event EventHandler EventToDissolve;
    // Start is called before the first frame update

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
        }
        else
        {
            _instance = this;
        }
    }

    private void Update()
    {
        LunchDissolveEvent();
    }

    void LunchDissolveEvent()
    {
        //I'm using Space key for Testing
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (EventToDissolve != null)
            {
                Debug.Log("Lunch the event");
                EventToDissolve(this, EventArgs.Empty);
            }
        }
    }
}

};
*/