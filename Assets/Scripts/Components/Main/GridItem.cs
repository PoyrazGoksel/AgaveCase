using System.Collections.Generic;
using Datas;
using UnityEngine;
using Zenject;

namespace Components.Main
{
    public class GridItem : MonoBehaviour, IGridItem
    {
        public int ID => _id;
        public Vector2Int Position => _position;
        public SpriteRenderer SpriteRenderer => _spriteRenderer;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private GridItemData _gridItemData;
        
        private Vector2Int _position;
        private int _id;
        public GridItemData GridItemData => _gridItemData;

        public void Construct(GridItemData gridItemData, Vector2Int position)
        {
            _gridItemData = gridItemData;
            _id = _gridItemData.ID;
            _spriteRenderer.sprite = _gridItemData.Sprite;
            _position = position;
        }
    }

    public interface IGridItem
    {
        int ID { get; }
        Vector2Int Position { get; }
    }
}
