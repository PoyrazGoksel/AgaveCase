using Extensions.Unity;
using UnityEditor;
using UnityEngine;

namespace Components.Main
{
    public class Cell : MonoBehaviour, ICellGridAccess
    {
        Cell ICellGridAccess.Cell => this;
        Transform ICellGridAccess.Transform => _transform;
        public int ID => _id;
        public Vector2Int Coord => _coord;
        public SpriteRenderer SpriteRenderer => _spriteRenderer;
        public GridItem GridItem => _gridItem;
        public Transform Transform => _transform;
        
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private GridItem _gridItem;
        [SerializeField] private Vector2Int _coord;
        [SerializeField] private int _id;
        [SerializeField] private Transform _transform;
        
        void ICellGridAccess.Construct(Vector2Int position)
        {
            _coord = position;
        }

        void ICellGridAccess.AssignGridItem(GridItem gridItem)
        {
            _gridItem = gridItem;
            _id = gridItem ? _gridItem.ID : -1;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            GUISkin skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
            Vector3 transformPosition = transform.position;
            GizmosUtils.DrawText(skin ,_coord.ToString(), transformPosition + 0.7f * Vector3.up, Color.red);
            GizmosUtils.DrawText(skin ,"ID: " + _id, transformPosition + 0.7f * Vector3.down, Color.magenta);
        }
#endif
    }

    public interface ICellGridAccess
    {
        void Construct(Vector2Int position);
        void AssignGridItem(GridItem gridItem);
        Vector2Int Coord { get; }
        Transform Transform { get; }
        Cell Cell { get; }
    }
}