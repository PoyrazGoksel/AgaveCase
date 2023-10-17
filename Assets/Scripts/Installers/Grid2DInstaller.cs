using System;
using System.Collections.Generic;
using System.Linq;
using Components.Main;
using Datas;
using Extensions.System;
using Extensions.Unity;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Utils;
using Zenject;

namespace Installers
{
    public class Grid2DInstaller : MonoInstaller<Grid2DInstaller>
    {
        [SerializeField] private Grid2D _grid2D;
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private GameObject _gridItemPrefab;
        [Inject] private Settings MySettings { get; set; }

        //[Button]
        private void RandomizeGrid()
        {
            /*foreach (KeyValuePair<Vector2Int, Cell> keyValuePair in _grid2D)
            {
                Cell cell = keyValuePair.Value;

                List<GridItemData> placeableGridItem = GridCalculations.GetPlaceableGridItems
                (
                    MySettings,
                    cell,
                    _grid2D,
                    3
                );

                GridItemData randomGridItemData = placeableGridItem.Random();

                if (randomGridItemData == null)
                {
                    Debug.LogError("No placeable grid item found");
                    RandomizeGrid();
                }

                InstantiateGridItems(cell, randomGridItemData);
            }*/
        }

        private void InstantiateGridItems(Cell cellToAssign, GridItemData gridItemData)
        {
            GridItem instantiateGridItem = Container.InstantiatePrefab
            (_gridItemPrefab, cellToAssign.transform)
            .GetComponent<GridItem>();

            instantiateGridItem.Construct(gridItemData, cellToAssign.Position);
            cellToAssign.AssignGridItem(instantiateGridItem);
        }

#if UNITY_EDITOR
        [Button]
        private void CreateGrid(Vector2Int size)
        {
            if (size == Vector2Int.zero) size = new Vector2Int(8, 8);

            if (_grid2D is {Count: > 0}) DiscardGrid();

            Vector3 boundsSize = _cellPrefab.GetComponent<Cell>()
            .SpriteRenderer.bounds.size;

            _grid2D = new Grid2D();

            for (int x = 0; x < size.x; x ++)
            {
                for (int y = 0; y < size.y; y ++)
                {
                    Cell instantiateCell = InstantiateCell(x, y, boundsSize);
                    _grid2D.Add(new Vector2Int(x, y), instantiateCell);
                }
            }
        }

        private Cell InstantiateCell(int x, int y, Vector3 tileSpriteSize)
        {
            Cell instantiateCell = PrefabUtility.InstantiatePrefab(_cellPrefab, gameObject.scene)
            .GetComponent<Cell>();

            instantiateCell.Construct(new Vector2Int(x, y));

            instantiateCell.transform.position = new Vector3
            (x * tileSpriteSize.x, y * tileSpriteSize.y, 0);

            instantiateCell.name = $"Cell {x} {y}";
            instantiateCell.transform.SetParent(transform);

            return instantiateCell;
        }

        [Button]
        private void RandomizeGridEditor()
        {
            DiscardItems();

            MySettings = Resources.Load<GridInstallerSettings>
            (EnvironmentVariables.SettingsPath + nameof(GridInstallerSettings))
            .Settings;

            List<GridItemData> gridItemDatas = new(MySettings.GridItemData);

            foreach (KeyValuePair<Vector2Int, Cell> keyValuePair in _grid2D)
            {
                Cell cell = keyValuePair.Value;

                List<GridItemData> placeableGridItems = GridF.GetPlaceableGridItems
                (
                    gridItemDatas,
                    cell,
                    _grid2D,
                    3
                );

                if (placeableGridItems.Count == 0)
                {
                    Debug.LogError("No placeable grid item found");

                    return;
                }

                int randomIndex = UnityEngine.Random.Range(0, placeableGridItems.Count);

                GridItemData randomGridItemData = placeableGridItems[randomIndex];

                InstantiateGridItemsEditor(cell, randomGridItemData.Clone());
            }
        }

        [Button]
        private void DebugMatches()
        {
            _grid2D.DoToAll(
                delegate(KeyValuePair<Vector2Int, Cell> e)
                {
                    e.Value.transform.localScale = Vector3.one;
                    e.Value.GridItem.transform.localScale = Vector3.one;
                }
            );
            _grid2D.DoToAll(TryDebugMatches);
        }

        /*[Button]
        private void DebugMatchesU()
        {
            _grid2D.DoToAll(TryDebugMatchesU);
        }*/

        /*private void TryDebugMatchesU(KeyValuePair<Vector2Int, Cell> obj)
        {
            List<GridItem> tryGetMatches = GridF.TryGetMatchesU
            (
                obj.Value.GridItem.GridItemData,
                obj.Value,
                _grid2D,
                3
            );

            if (tryGetMatches != null)
            {
                obj.Value.transform.localScale = Vector3.one * 1.2f;
                tryGetMatches.DoToAll(x => x.transform.localScale = Vector3.one * 1.2f);
            }
        }*/

        private void TryDebugMatches(KeyValuePair<Vector2Int, Cell> e)
        {
            /*
            List<GridItem> tryGetMatches = GridF.TryGetMatches
            (e.Value.GridItem.GridItemData, e.Value, _grid2D);

            if (tryGetMatches != null)
            {
                e.Value.transform.localScale = Vector3.one * 1.2f;
                tryGetMatches.DoToAll(x => x.transform.localScale = Vector3.one * 1.2f);
            }
        */
            List<GridItem> tryGetMatches = GridF.TryGetMatchesU
            (
                e.Value.GridItem.GridItemData,
                e.Value,
                _grid2D,
                3
            );

            if (tryGetMatches != null)
            {
                e.Value.GridItem.transform.localScale = Vector3.one * 1.2f;
                tryGetMatches.DoToAll(x => x.transform.localScale = Vector3.one * 1.2f);
            }
        }

        [Button]
        private void DiscardGrid()
        {
            foreach (KeyValuePair<Vector2Int, Cell> keyValuePair in _grid2D)
            {
                DestroyImmediate(keyValuePair.Value.gameObject);
            }

            _grid2D = new Grid2D();
        }

        private void InstantiateGridItemsEditor(Cell cellToAssign, GridItemData gridItemData)
        {
            GridItem instantiateGridItem = Instantiate(_gridItemPrefab, cellToAssign.transform)
            .GetComponent<GridItem>();

            instantiateGridItem.Construct(gridItemData, cellToAssign.Position);
            cellToAssign.AssignGridItem(instantiateGridItem);
        }

        [Button]
        private void DiscardItems()
        {
            foreach (KeyValuePair<Vector2Int, Cell> keyValuePair in _grid2D)
            {
                keyValuePair.Value.AssignGridItem(null);
            }
        }

#endif
        [Serializable]
        public class Grid2D : UnityDictionary<Vector2Int, Cell>
        {
            public Vector2Int Size => _size;
            private Vector2Int _size;

            public new void Add(Vector2Int key, Cell value)
            {
                base.Add(key, value);

                _size = new Vector2Int
                (Mathf.Max(_size.x, key.x + 1), Mathf.Max(_size.y, key.y + 1));
            }
        }

        [Serializable]
        public class Settings
        {
            [SerializeField] private List<GridItemData> _gridItemSettingsDatas;
            public List<GridItemData> GridItemData => _gridItemSettingsDatas;
        }
    }
}