using UnityEngine;

namespace Datas.Settings
{
    [CreateAssetMenu(fileName = nameof(LevelSettings), menuName = "Settings/" + nameof(LevelSettings))]
    public class LevelSettings : ScriptableObject
    {
        [SerializeField] private GameObject _levelPrefab;
        public GameObject LevelPrefab => _levelPrefab;
    }
}