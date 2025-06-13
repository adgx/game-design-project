using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    // Movement
    [SerializeField] private KeyCode forwardInput, backInput, leftInput, rightInput;
    // Shooting
    [SerializeField] private KeyCode leftMouseInput;

	// Audio management: Pause
	[SerializeField] private KeyCode pauseInput;


	private float verticalInput, horizontalInput, shootInput;

    public float Vertical
    {
        get
        {
            return verticalInput;
        }
    }

    public float Horizontal {
        get
        {
            return horizontalInput;
        }
    }

    public float Shoot
    {
        get
        {
            return shootInput;
        }
    }

	// Audio management
	public bool PausePressed() {
		return Input.GetKeyDown(pauseInput);
	}

	private void GetInput()
    {
        //Get horizontal input
        if (Input.GetKey(forwardInput))
        {
			verticalInput = 1.0f;
        }
        else if (Input.GetKey(backInput))
        {
			verticalInput = -1.0f;
        }
        else    //Not pressing forward not back
        {
			verticalInput = 0.0f;
        }
        
        //Get rotation input
        if (Input.GetKey(rightInput))
        {
			horizontalInput = 1.0f;
        }
        else if (Input.GetKey(leftInput))
        {
			horizontalInput = -1.0f;
        }
        else    //Not pressing right nor left
        {
            horizontalInput = 0.0f;
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
