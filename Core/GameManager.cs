using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Infrastructure.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("State Data")]
        [SerializeField] private GameStateDataSO menuData;
        [SerializeField] private GameStateDataSO playingData;
        [SerializeField] private GameStateDataSO pausedData;
        [SerializeField] private GameStateDataSO winData;

        private GameStateDataSO currentState;

        private bool isTransitioning = false;

        private void Awake() => GoToMenu();

        private Dictionary<GameStateType, Dictionary<GameStateTypeTurnType, List<Action>>> _handlers = new();

        public void Subscribe(GameStateType stateType, GameStateTypeTurnType turnType, Action handler)
        {
            if (!_handlers.ContainsKey(stateType))
                _handlers[stateType] = new Dictionary<GameStateTypeTurnType, List<Action>>();

            if (!_handlers[stateType].ContainsKey(turnType))
                _handlers[stateType][turnType] = new List<Action>();

            _handlers[stateType][turnType].Add(handler);
        }

        public void Unsubscribe(GameStateType stateType, GameStateTypeTurnType turnType)
        {
            if (!_handlers.ContainsKey(stateType))
                return;

            if (!_handlers[stateType].ContainsKey(turnType))
                return;

            _handlers[stateType][turnType].Clear();
            _handlers[stateType].Remove(turnType);

            if (_handlers[stateType].Count == 0)
            {
                _handlers.Remove(stateType);
            }
        }

        private void Trigger(GameStateType stateType, GameStateTypeTurnType turnType)
        {
            if (_handlers.TryGetValue(stateType, out var turnHandlers))
            {
                if (turnHandlers.TryGetValue(turnType, out var handlers))
                {
                    foreach (var handler in handlers)
                    {
                        handler?.Invoke();
                    }
                }
            }
        }

        public void SwitchToState(GameStateDataSO newState)
        {
            if (isTransitioning)
            {
                //LogReader.Read(""); сюда кастом ошибку
                Debug.Log($"Состояние пыталось переключиться до того как прошлое переключение было завершено");
                return;
            }
            if (newState == null)
            {
                Debug.LogError("Переданное состояние для переключения игры равно == null");// LogReader.Read("");  надо ошибку написать
                return;
            }

            isTransitioning = true;

            if (currentState != null)
            {
                Trigger(currentState.type, GameStateTypeTurnType.OnExit);
            }
            Debug.Log($"Переключение состояния: {currentState?.type} -> {newState.type}");

            Time.timeScale = newState.timeScale;
            Cursor.lockState = newState.cursorMode;
            Cursor.visible = newState.cursorVisible;

            currentState = newState;

            Trigger(currentState.type, GameStateTypeTurnType.OnEnter);

            isTransitioning = false;
        }

        public void GoToMenu()
        {
            if (menuData != null) SwitchToState(menuData);
            else Debug.LogError("MenuData не назначено!"); //LogReader.Read(""); сюда кастом ошибку
        }

        public void StartGame()
        {
            if (playingData != null) SwitchToState(playingData);
            else Debug.LogError("GamePlayData не назначено!"); //LogReader.Read(""); сюда кастом ошибку
        }

        public void PausedGame()
        {
            if (pausedData != null) SwitchToState(pausedData);
            else Debug.LogError("GameOverData не назначено!"); //LogReader.Read(""); сюда кастом ошибку
        }

        public void WinGame()
        {
            if (winData != null) SwitchToState(winData);
            else Debug.LogError("WinData не назначено!");  //LogReader.Read(""); сюда кастом ошибку
        }

        public GameStateDataSO GetCurrentState() => currentState;
    }
}