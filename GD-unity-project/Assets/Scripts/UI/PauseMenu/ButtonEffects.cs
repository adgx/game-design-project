using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField] private Sprite buttonNormalSprite;
	[SerializeField] private Sprite buttonHoverSprite;

	public void OnPointerEnter(PointerEventData eventData) {
		transform.GetComponent<Image>().sprite = buttonHoverSprite;
		transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.black;
	}

	public void OnPointerExit(PointerEventData eventData) {
		transform.GetComponent<Image>().sprite = buttonNormalSprite;
		transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
	}
}
