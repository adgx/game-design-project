using System;
using System.Collections;
using UnityEngine;
using ORF;

namespace Helper {
	public class FadeManager : MonoBehaviour {
		public static FadeManager Instance;

		[SerializeField] private CanvasGroup fadeCanvasGroup;
		[SerializeField] private float fadeDuration = 1f;

		private void Awake() {
			if(Instance == null)
				Instance = this;
			else
				Destroy(gameObject);

			transform.SetParent(null); // Detach from "Managers" to avoid warnings
			DontDestroyOnLoad(gameObject);
		}

		public void FadeOutIn(Action onFadeMidpoint) {
			StartCoroutine(FadeOutInRoutine(onFadeMidpoint));
		}

		private IEnumerator FadeOutInRoutine(Action onFadeMidpoint) {
			yield return StartCoroutine(FadeTo(1));

			onFadeMidpoint?.Invoke();

			yield return new WaitForEndOfFrame();
			yield return new WaitForSeconds(fadeDuration);

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