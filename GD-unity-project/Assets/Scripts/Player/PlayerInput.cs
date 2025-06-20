using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    // Movement
    [SerializeField] private KeyCode forwardInput, backInput, leftInput, rightInput;
    // Shooting
    [SerializeField] private KeyCode leftMouseInput;

	// Pause
	[SerializeField] private KeyCode pauseInput, pauseInputController;
    // Inventory
	[SerializeField] private KeyCode inventoryInput, inventoryInputController;
    // Back
    [SerializeField] private KeyCode backKeyController;


	private float verticalInput, horizontalInput;

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

	// Audio management
	public bool PausePressed() {
		return Input.GetKeyDown(pauseInput) || Input.GetKeyDown(pauseInputController);
	}

    public bool InventoryPressed() {
        return Input.GetKeyDown(inventoryInput) || Input.GetKeyDown(inventoryInputController);
    }

    public bool backKeyPressed() {
        return Input.GetKeyDown(backKeyController);
    }

	private void GetInput()
    {
        //Get vertical input
		verticalInput = Input.GetAxis("Vertical");
        
        //Get horizontal input
		horizontalInput = Input.GetAxis("Horizontal");
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
    }
}
