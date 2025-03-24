using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private KeyCode forwardInput, backInput, leftInput, rightInput;

    private float horizontalInput, rotationInput;

    public float Horizontal
    {
        get
        {
            return horizontalInput;
        }
    }

    public float Rotation {
        get {
            return rotationInput;
        }
    }

    private void GetInput()
    {
        //Get horizontal input
        if (UnityEngine.Input.GetKey(forwardInput))
        {
            horizontalInput = 1.0f;
        }
        else if (UnityEngine.Input.GetKey(backInput))
        {
            horizontalInput = -1.0f;
        }
        else                                                                //Non sto premendo nè destra nè sinistra
        {
            horizontalInput = 0.0f;
        }
        
        //Get rotation input
        if (UnityEngine.Input.GetKey(rightInput))
        {
            rotationInput = 1.0f;
        }
        else if (UnityEngine.Input.GetKey(leftInput))
        {
            rotationInput = -1.0f;
        }
        else                                                                //Non sto premendo nè destra nè sinistra
        {
            rotationInput = 0.0f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
    }
}
