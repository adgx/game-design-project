using UnityEngine;

public class FadeManagerLoadingScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup LoadingScreenCanvas;
    [SerializeField] private GameObject LoadingScreen;
    
    public bool fadeIn = false, fadeOut = false;

    public void Show()
    {
        fadeIn = true;
        fadeOut = false;
    }

    public void Hide()
    {
        fadeIn = false;
        fadeOut = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (fadeIn)
        {
            if (LoadingScreenCanvas.alpha < 1)
            {
                LoadingScreenCanvas.alpha += Time.deltaTime;
                if (LoadingScreenCanvas.alpha >= 1)
                {
                    LoadingScreenCanvas.alpha = 1;
                    fadeIn = false;
                }
            }
        }

        if (fadeOut)
        {
            if (LoadingScreenCanvas.alpha > 0)
            {
                LoadingScreenCanvas.alpha -= Time.deltaTime;
                if (LoadingScreenCanvas.alpha <= 0)
                {
                    LoadingScreenCanvas.alpha = 0;
                    fadeOut = false;
                }
            }
        }
    }
}
