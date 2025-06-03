using Helper;
using UnityEngine;
using TMPro;

namespace RoomManager
{
    public class GameTimer : MonoBehaviour
    {
        private const float TimeLimit = 30f;
        private float currentTime;

        public TMP_Text timerText;

        private bool isRunning;

        public RoomManager roomManager;

        private void OnDestroy()
        {
            if (roomManager)
                roomManager.OnRunReady -= HandleRunReady;
        }

        private void Start()
        {
            if (roomManager)
            {
                roomManager.OnRunReady += HandleRunReady;
            }
        }

        private void Update()
        {
            if (!isRunning)
                return;

            currentTime -= Time.deltaTime;

            if (currentTime <= 0f)
            {
                currentTime = 0f;
                isRunning = false;

                ResetRun();
            }

            UpdateTimerUI();
        }

        private void HandleRunReady()
        {
            currentTime = TimeLimit;
            isRunning = true;
        }

        private void UpdateTimerUI()
        {
            var minutes = Mathf.FloorToInt(currentTime / 60f);
            var seconds = Mathf.FloorToInt(currentTime % 60f);

            timerText.text = $"{minutes:00}:{seconds:00}";
        }

        private void ResetRun()
        {
            if (!roomManager)
                return;

            FadeManager.Instance.FadeOutIn(() =>
            {
                roomManager.RegenerateRooms();
                currentTime = TimeLimit;
                isRunning = true;
            });
        }
    }
}