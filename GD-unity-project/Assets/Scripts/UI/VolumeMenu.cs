using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class VolumeMenu : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject volumeMenu;
    [SerializeField] private GameObject firstSelected;

    public void OpenVolumeMenu()
    {
		volumeMenu.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(firstSelected);
    }

    public void CloseVolumeMenu() {
		volumeMenu.gameObject.SetActive(false);
    }
}