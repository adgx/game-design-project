using System;
using System.Collections;
using UnityEngine;
using ORF;

namespace Helper {
	public class FadeManager : MonoBehaviour {
		public static FadeManager Instance;

		[SerializeField] private CanvasGroup fadeCanvasGroup;
		[SerializeField] private float fadeDuration = 0.5f;

		private Player player;

		private void Awake() {
			if(Instance == null)
				Instance = this;
			else
				Destroy(gameObject);

			transform.SetParent(null); // Detach from "Managers" to avoid warnings
			DontDestroyOnLoad(gameObject);

			player = GameObject.FindWithTag("Player").GetComponent<Player>();
		}

		public void FadeOutIn(Action onFadeMidpoint) {
			StartCoroutine(FadeOutInRoutine(onFadeMidpoint));
		}

		private IEnumerator FadeOutInRoutine(Action onFadeMidpoint) {
			player.FreezeMovement(true);

			yield return StartCoroutine(FadeTo(1));

			onFadeMidpoint?.Invoke();

			yield return new WaitForEndOfFrame();
			yield return new WaitForSeconds(0.5f);

			yield return StartCoroutine(FadeTo(0));
		}

		private IEnumerator FadeTo(float targetAlpha) {
			var startAlpha = fadeCanvasGroup.alpha;
			var time = 0f;

			while(time < fadeDuration) {
				fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
				time += Time.deltaTime;
				yield return null;
			}

			fadeCanvasGroup.alpha = targetAlpha;
		}
	}
}