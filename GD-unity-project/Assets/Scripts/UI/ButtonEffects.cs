using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonEffects : MonoBehaviour
{
	[SerializeField] private Sprite buttonNormalSprite;
	[SerializeField] private Sprite buttonHoverSprite;

	public void OnMouseEnter(GameObject button) {
		button.GetComponent<Image>().sprite = buttonHoverSprite;
		button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.black;
		if(!EventSystem.current.alreadySelecting)
			EventSystem.current.SetSelectedGameObject(button);
	}

	public void OnMouseExit(GameObject button) {
		button.GetComponent<Image>().sprite = buttonNormalSprite;
		button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
	}
}
