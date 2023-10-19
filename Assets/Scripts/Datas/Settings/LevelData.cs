using UnityEngine;

namespace Datas.Settings
{
    [CreateAssetMenu(fileName = nameof(LevelData), menuName = "Settings/" + nameof(LevelData))]
    public class LevelData : ScriptableObject
    {
        [SerializeField] private GameObject _levelPrefab;
        public GameObject LevelPrefab => _levelPrefab;
    }
}