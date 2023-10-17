using Extensions.Unity;
using UnityEngine;

namespace Components.Main
{
    public class Cell : MonoBehaviour
    {
        public int ID => _id;
        public Vector2Int Position => _position;
        public SpriteRenderer SpriteRenderer => _spriteRenderer;
        public GridItem GridItem => _gridItem;
        
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private GridItem _gridItem;
        
        private Vector2Int _position;
        private int _id;

        public void Construct(Vector2Int position)
        {
            _position = position;
        }
        
        public void AssignGridItem(GridItem gridItem)
        {
            if (gridItem == null)
            {
                if (_gridItem != null) 
                {
                    _gridItem.gameObject.DestroyNow();
                    SetGridItem(gridItem);
                }
                
                return;
            }
            if (_gridItem != null) _gridItem.gameObject.DestroyNow();

            SetGridItem(gridItem);
        }
        
        private void SetGridItem(GridItem gridItem)
        {
            _gridItem = gridItem;
            _id = gridItem ? _gridItem.ID : -1;
        }
    }
}