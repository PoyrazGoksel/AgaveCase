using System;
using Datas;
using DG.Tweening;
using Extensions.DoTween;
using Extensions.Unity;
using UnityEngine;

namespace Components.Main
{
    public class GridItem : MonoBehaviour, IGridItemGridAccess, IZenjPoolObj, ITweenContainerBind, ISelectable
    {
        ITweenContainer ITweenContainerBind.TweenContainer
        {
            get => TweenContainer;
            set => TweenContainer = value;
        }
        
        private ITweenContainer TweenContainer { get; set; }
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
        private Tween _selectedTween;

        void IGridItemGridAccess.Construct(GridItemData gridItemData, Cell cell, Bounds gridBounds)
        {
            _gridBounds = gridBounds;
            _gridItemData = gridItemData;
            _spriteRenderer.sprite = _gridItemData.Sprite;
            _cell = cell;
        }

        private void Awake()
        {
            TweenContainer = TweenContain.Install(this);
        }

        private void OnDisable()
        {
            TweenContainer.Clear();
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

        void IZenjPoolObj.AfterCreate() {}

        void IZenjPoolObj.BeforeDeSpawn() {}

        void IZenjPoolObj.TweenDelayedDeSpawn(Func<bool> onComplete)
        {
            TweenContainer.AddTween = _transform.DOScale(Vector3.zero, 0.3f);
            TweenContainer.AddedTween.onComplete += delegate
            {
                onComplete?.Invoke();
            };
        }

        void IZenjPoolObj.AfterSpawn()
        {
            _transform.localScale = Vector3.one;
        }

        void ISelectable.OnSelect()
        {
            _selectedTween = _transform.DoYoYo(1.2f, 0.3f);
            TweenContainer.AddTween = _selectedTween;
        }

        void ISelectable.OnDeselect()
        {
            if (_selectedTween.IsActive())
            {
                _selectedTween.Kill();
                _transform.localScale = Vector3.one;
            }
        }
    }

    public interface ISelectable
    {
        void OnSelect();
        void OnDeselect();
    }

    public interface IGridItemGridAccess
    {
        void Construct(GridItemData gridItemData, Cell cell, Bounds gridBounds);
        void ChangeCell(Cell newCell);
        GridItem GridItem { get; }
    }
}
