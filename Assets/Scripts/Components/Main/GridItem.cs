using System;
using Datas;
using Extensions.Unity;
using UnityEngine;

namespace Components.Main
{
    public class GridItem : MonoBehaviour, IGridItemGridAccess, IZenjPoolObj
    {
        GridItem IGridItemGridAccess.GridItem => this;
        public int ID => _gridItemData.ID;
        public Vector2Int Coord => _cell.Coord;
        public Cell Cell => _cell;
        public Transform Transform => _transform;
        public bool IsVisible => _isVisible;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private GridItemData _gridItemData;
        [SerializeField] private Cell _cell;
        [SerializeField] private Transform _transform;
        [SerializeField] private Bounds _gridBounds;
        [SerializeField] private bool _isVisible = true;

        void IGridItemGridAccess.Construct(GridItemData gridItemData, Cell cell, Bounds gridBounds)
        {
            _gridBounds = gridBounds;
            _gridItemData = gridItemData;
            _spriteRenderer.sprite = _gridItemData.Sprite;
            _cell = cell;
        }

        void IGridItemGridAccess.ChangeCell(Cell newCell)
        {
            _cell = newCell;
            transform.SetParent(newCell.transform);
        }

        public void UpdateRenderer()
        {
            if (_gridBounds.Contains(_transform.position) == false)
            {
                _spriteRenderer.enabled = false;
                _isVisible = false;
            }
            else
            {
                _spriteRenderer.enabled = true;
                _isVisible = true;
            }
        }

        public ZenjectPool MyPool { get; set; }

        public void AfterCreate() {}

        public void BeforeDeSpawn() {}

        public void TweenDelayedDeSpawn(Func<bool> onComplete) {}

        public void AfterSpawn() {}
    }

    public interface IGridItemGridAccess
    {
        void Construct(GridItemData gridItemData, Cell cell, Bounds gridBounds);
        void ChangeCell(Cell newCell);
        GridItem GridItem { get; }
    }
}
