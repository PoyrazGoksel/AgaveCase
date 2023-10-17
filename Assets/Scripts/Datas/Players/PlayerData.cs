using System;
using System.IO;
using Events.External;
using Extensions.Unity;
using JetBrains.Annotations;
using UnityEngine;
using Utils;
using Zenject;

namespace Datas.Players
{
    [Serializable]
    [UsedImplicitly]
    public class PlayerData : JsonVersionWrapper, IInitializable, IJsonCallBackReceiver, IPlayerData
    {
        [Inject] private GameStateEvents GameStateEvents { get; set; }
        [Inject] private PlayerEvents PlayerEvents { get; set; }
        [SerializeField] private int _currentLevel;
        int IPlayerData.CurrentLevel => _currentLevel;
        [SerializeField] private int _currency;
        int IPlayerData.Currency => _currency;
        private string _playerSavePath;

        public void OnBeforeSerialize() {}

        public void OnAfterDeserialize() {}

        public void Initialize()
        {
            _playerSavePath = $"{Application.persistentDataPath}/{nameof(PlayerData)}{EnvironmentVariables.SaveFileExt}";

            if (File.Exists(_playerSavePath) == false)
            {
                CreateDefaultPlayer();
                Save();
            }
            else
            {
                Load(_playerSavePath);
            }

            OnRegisterEvents();
        }

        private void Save()
        {
            string playerRawData = JsonUtilityWithCall.ToJson(this, true);

            JsonUtilityWithCall.WriteToEnd(playerRawData, _playerSavePath);
        }

        private void Load(string playerSavePath)
        {
            string playerRawData = JsonUtilityWithCall.ReadToEnd(playerSavePath);

            PlayerData loadedPlayer = JsonUtilityWithCall.FromJson<PlayerData>(playerRawData);

            _currentLevel = loadedPlayer._currentLevel;
            SetCurrency(loadedPlayer._currency);

            PlayerEvents.PlayerLoaded?.Invoke();
        }

        private void CreateDefaultPlayer()
        {
            SetDefaultPlayerData();

            void SetDefaultPlayerData()
            {
                _currentLevel = 1;
                SetCurrency(0);
            }

            PlayerEvents.NewPlayerCreated?.Invoke();
        }

        private void SetCurrency(int curr)
        {
            _currency = curr;
            PlayerEvents.PlayerCurrencyChanged?.Invoke(_currency);
        }

        private void LevelUp()
        {
            _currentLevel ++;

            PlayerEvents.PlayerLevelChanged?.Invoke(_currentLevel);
        }

        private void OnRegisterEvents()
        {
            GameStateEvents.LevelSuccess += OnLevelSuccess;
        }

        private void OnLevelSuccess()
        {
            LevelUp();
        }
    }

    public interface IPlayerData
    {
        int CurrentLevel { get; }
        int Currency { get; }
    }
}