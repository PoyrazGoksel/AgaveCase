using System;
using UnityEngine;

namespace Datas
{
    [Serializable]
    public class GridItemData
    {
        public Sprite Sprite => _sprite;
        public int ID => _id;
            
        [SerializeField] private Sprite _sprite;
        [SerializeField] private int _id;

        public GridItemData Clone()
        {
            return new GridItemData
            {
                _sprite = _sprite,
                _id = _id
            };
        }
    }
}