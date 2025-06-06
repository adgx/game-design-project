using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class VolumeMenu : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject firstSelected;

    private PlayerInput playerInput;

    private void Start()
    {
        menu.gameObject.SetActive(false);
        playerInput = Player.Instance.GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (playerInput.PausePressed())
        {
            ToggleVolumeMenu();
        }
    }

    private void ToggleVolumeMenu()
    {
        if (menu.gameObject.activeInHierarchy)
        {
            menu.gameObject.SetActive(false);
            Player.Instance.FreezeMovement(false);
        }
        else
        {
            menu.gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(firstSelected);
            Player.Instance.FreezeMovement(true);
        }
    }
}