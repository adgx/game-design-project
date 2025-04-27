using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    // Movement
    [SerializeField] private KeyCode forwardInput, backInput, leftInput, rightInput;
    // Shooting
    [SerializeField] private KeyCode leftMouseInput;

    private float horizontalInput, rotationInput, shootInput;

    public float Horizontal
    {
        get
        {
            return horizontalInput;
        }
    }

    public float Rotation {
        get
        {
            return rotationInput;
        }
    }

    public float Shoot
    {
        get
        {
            return shootInput;
        }
    }

    private void GetInput()
    {
        //Get horizontal input
        if (Input.GetKey(forwardInput))
        {
            horizontalInput = 1.0f;
        }
        else if (Input.GetKey(backInput))
        {
            horizontalInput = -1.0f;
        }
        else    //Not pressing forward not back
        {
            horizontalInput = 0.0f;
        }
        
        //Get rotation input
        if (Input.GetKey(rightInput))
        {
            rotationInput = 1.0f;
        }
        else if (Input.GetKey(leftInput))
        {
            rotationInput = -1.0f;
        }
        else    //Not pressing right nor left
        {
            rotationInput = 0.0f;
        }
        
        // Get shoot input
        if (Input.GetKey(leftMouseInput))
        {
            shootInput = 1.0f;
        }
        else
        {
            shootInput = 0.0f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
    }
}
