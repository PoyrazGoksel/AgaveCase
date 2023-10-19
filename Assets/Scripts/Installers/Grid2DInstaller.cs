using System;
using System.Collections.Generic;
using System.Linq;
using Components.Main;
using Datas;
using DG.Tweening;
using Events.External;
using Extensions.DoTween;
using Extensions.System;
using Extensions.Unity;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Utils;
using Zenject;

namespace Installers
{
    public class Grid2DInstaller : MonoInstaller<Grid2DInstaller>, IGridEditorAccess,
    ITweenContainerBind
    {
        [SerializeField] private Grid2D _grid2D;
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private GameObject _gridItemPrefab;
        [SerializeField] private ItemSpawner[] _itemSpawners;
        [SerializeField] private Transform _transform;
        private ITweenContainer TweenContainer { get; set; }
        [Inject] private GridEvents GridEvents { get; set; }
        [Inject] private GridInstallerSettings GridInstallerSettings { get; set; }
        [Inject] private GameStateEvents GameStateEvents { get; set; }
        private ZenjectPool _gridItemPool;
        private Vector3 _gridPadding;
        private Settings _mySettings;

        ITweenContainer ITweenContainerBind.TweenContainer
        {
            get => TweenContainer;
            set => TweenContainer = value;
        }

        private void Awake()
        {
            _gridItemPool = new ZenjectPool(new ZenjectPoolData(Container, _gridItemPrefab, 64));

            _gridPadding = _cellPrefab.GetComponent<Cell>()
            .SpriteRenderer.bounds.size;

            TweenContainer = TweenContain.Install(this);

            _itemSpawners = new ItemSpawner[_grid2D.Size.x];

            for (int x = 0; x < _grid2D.Size.x; x ++)
            {
                ItemSpawner itemSpawner = new
                (new Vector3(x * _gridPadding.x, GetGridTopCellPosY(), 0), _grid2D.Bounds);

                _itemSpawners[x] = itemSpawner;
                itemSpawner.SetActive();
            }
        }

        public override void Start()
        {
            _mySettings = GridInstallerSettings.Settings;
            _grid2D.DoToAll(e => e.Value.GridItem.gameObject.DestroyNow());
            GridEvents.GridStart?.Invoke(_grid2D.Bounds);
            RandomizeGrid();
        }

        private void OnEnable()
        {
            RegisterEvents();
        }

        private void OnDisable()
        {
            UnRegisterEvents();
            TweenContainer.Clear();
        }

        public override void InstallBindings() {}

        private static void Swap(Cell firstCell, Cell secondCell)
        {
            IGridItemGridAccess firstItem = firstCell.GridItem;
            IGridItemGridAccess secondItem = secondCell.GridItem;
            AssignGridItemToCell(firstCell, secondItem);
            AssignGridItemToCell(secondCell, firstItem);
        }

        private static void AssignGridItemToCell(ICellGridAccess cell, IGridItemGridAccess gridItem)
        {
            if (gridItem != null)
            {
                cell.AssignGridItem(gridItem.GridItem);
                gridItem.ChangeCell(cell.Cell);
            }
            else
            {
                cell.AssignGridItem(null);
            }
        }

        private void RandomizeGrid()
        {
            List<GridItemData> gridItemDatas = new
            (_mySettings.GridItemData.Where(e => e.ID != -1 && e.ID != 0));

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

                InstantiateGridItem(cell, randomGridItemData);
            }
        }

        private void InstantiateGridItem(Cell cellToAssign, GridItemData gridItemData)
        {
            GridItem instantiateGridItem = _gridItemPool.Request<GridItem>(cellToAssign.Transform);

            ((IGridItemGridAccess)instantiateGridItem).Construct
            (gridItemData.Clone(), cellToAssign, _grid2D.Bounds);

            ((ICellGridAccess)cellToAssign).AssignGridItem(instantiateGridItem);
            instantiateGridItem.Transform.SetParent(cellToAssign.Transform);
            instantiateGridItem.Transform.localPosition = Vector3.zero;
        }

        private void TryDestroyMatches(Cell mouseDownCell, Cell mouseUpCell)
        {
            List<GridItem> matchesMouseDown = GridF.TryGetMatches(mouseDownCell, _grid2D, 3);
            List<GridItem> matchesMouseUp = GridF.TryGetMatches(mouseUpCell, _grid2D, 3);

            List<GridItem> allMatches = matchesMouseDown.Union(matchesMouseUp)
            .ToList();

            if (allMatches.Count > 0)
            {
                allMatches.DoToAll(DestroyGridItem);

                SpawnNewGridItems();
            }
            else
            {
                GridEvents.NoMatches?.Invoke(mouseDownCell.GridItem, mouseUpCell.GridItem);
            }
        }

        private void DestroyGridItem(GridItem gridItem)
        {
            ICellGridAccess cellGridAccess = gridItem.Cell;
            _gridItemPool.DeSpawnAfterTween(gridItem);

            cellGridAccess.AssignGridItem(null);
        }

        private void DestroyCellItem(Cell cell)
        {
            if (cell.GridItem == null) return;

            DestroyImmediate(cell.GridItem.gameObject);
            ((ICellGridAccess)cell).AssignGridItem(null);
        }

        private void SpawnNewGridItems()
        {
            List<GridItemData> gridItemDatas = new(_mySettings.GridItemData);

            gridItemDatas = gridItemDatas.Where(e => e.ID != -1 && e.ID != 0)
            .ToList();

            Dictionary<Vector2Int, Cell> droppedCells = new();

            for (int x = 0; x < _grid2D.Size.x; x ++)
            {
                for (int y = 0; y < _grid2D.Size.y; y ++)
                {
                    Cell thisCell = GetCell(x, y);

                    if (thisCell.GridItem == null) continue;

                    Cell cellToAssign = null;

                    for (int y1 = thisCell.Coord.y - 1; y1 >= 0; y1 --)
                    {
                        Cell cell = GetCell(x, y1);

                        if (cell.GridItem != null) break;

                        cellToAssign = cell;
                    }

                    if (cellToAssign == null) continue;

                    Swap(thisCell, cellToAssign);

                    droppedCells.Add(cellToAssign.Coord, cellToAssign);
                }
            }

            for (int x = 0; x < _grid2D.Size.x; x ++)
            {
                int posYCounter = 0;

                for (int y = 0; y < _grid2D.Size.y; y ++)
                {
                    Cell cell = GetCell(x, y);

                    if (cell.GridItem == null)
                    {
                        posYCounter ++;
                        Cell cellToAssign = null;

                        for (int y1 = cell.Coord.y; y1 >= 0; y1 --)
                        {
                            Cell cell1 = GetCell(cell.Coord.x, y1);

                            if (cell1.GridItem != null) break;

                            cellToAssign = cell1;
                        }

                        if (cellToAssign == null) continue;

                        ItemSpawner columnSpawner = _itemSpawners[cellToAssign.Coord.x];

                        if (columnSpawner.CanSpawn == false)
                        {
                            continue;
                        }

                        GridItemData randomGridItemData = gridItemDatas.Random();

                        GridItem instantiateGridItem = columnSpawner.InstantiateGridItem
                        (_gridItemPool, cellToAssign, randomGridItemData);

                        instantiateGridItem.Transform.position = new Vector3
                        (
                            cellToAssign.Transform.position.x,
                            GetGridTopCellPosY() + posYCounter * _gridPadding.y,
                            0
                        );

                        instantiateGridItem.UpdateRenderer();

                        droppedCells.Add
                        (new Vector2Int(cellToAssign.Coord.x, cellToAssign.Coord.y), cellToAssign);
                    }
                }
            }

            TweenContainer.AddSequence = DOTween.Sequence();

            for (int x = 0; x < _grid2D.Size.x; x ++)
            {
                int delayCounter = 0;

                for (int y = 0; y < _grid2D.Size.y; y ++)
                {
                    if (droppedCells.ContainsKey(new Vector2Int(x, y)))
                    {
                        delayCounter ++;

                        GridItem gridItem = droppedCells[new Vector2Int(x, y)].GridItem;

                        Tween doLocalMove = gridItem.Transform.DOLocalMove
                        (Vector3.zero, _mySettings.DropAnimDur);

                        TweenContainer.AddedSeq.Insert
                        (delayCounter * _mySettings.DropAnimDelay, doLocalMove);

                        doLocalMove.onUpdate += delegate
                        {
                            if (gridItem.IsVisible) return;

                            gridItem.UpdateRenderer();
                        };
                    }
                }
            }

            TweenContainer.AddedSeq.onComplete += delegate
            {
                List<GridItem> allMatches = new();

                foreach (KeyValuePair<Vector2Int, Cell> pair in _grid2D)
                {
                    if (pair.Value.GridItem == null)
                    {
                        continue;
                    }

                    List<GridItem> matches = GridF.TryGetMatches(pair.Value, _grid2D, 3);

                    if (matches.Count == 0) continue;

                    allMatches = allMatches.Union(matches)
                    .ToList();
                }

                if (allMatches.Count > 0)
                {
                    allMatches.DoToAll(DestroyGridItem);
                    SpawnNewGridItems();
                }
                else
                {
                    bool isGameOver = false;

                    /*
                    foreach (KeyValuePair<Vector2Int,Cell> pair in _grid2D)
                    {
                        Cell cell = pair.Value;

                        if (cell.GridItem != null)
                        {
                            List<Cell> neighs = GridF.GetNeighs(cell, _grid2D);

                            if (neighs.Count == 0) continue;

                            foreach (Cell neigh in neighs)
                            {
                                List<GridItem> matches = GridF.TryGetMatches
                                (
                                    cell.GridItem.ID,
                                    neigh,
                                    _grid2D,
                                    3
                                );

                                if (matches.Count == 0) continue;

                                _matchHintCells.Add(new Hint(matches));
                                isGameOver = false;
                            }
                        }
                    }*/

                    if (isGameOver)
                    {
                        GameStateEvents.LevelFail?.Invoke();
                    }
                    else
                    {
                        GridEvents.Matched?.Invoke();
                    }
                }
            };
        }

        private Cell GetCell(int x, int y)
        {
            if (_grid2D.ContainsKey(new Vector2Int(x, y)) == false)
            {
                return null;
            }

            return _grid2D[new Vector2Int(x, y)];
        }

        private float GetGridTopCellPosY()
        {
            return _transform.position.y + (_grid2D.Size.y - 1) * _gridPadding.y;
        }

        private void RegisterEvents()
        {
            GridEvents.SwapGridItems += OnSwapGridItems;
            GridEvents.RevertSwipe += OnRevertSwipe;
            GridEvents.GetGridItem = OnGetGridItem;
        }

        private void OnSwapGridItems(Cell mouseDownCell, Cell mouseUpCell)
        {
            Swap(mouseDownCell, mouseUpCell);

            TryDestroyMatches(mouseDownCell, mouseUpCell);
        }

        private void OnRevertSwipe(GridItem arg0, GridItem arg1)
        {
            Swap(arg0.Cell, arg1.Cell);
        }

        private GridItem OnGetGridItem(Vector2Int arg)
        {
            Cell cell = GetCell(arg.x, arg.y);

            if (cell == null) return null;

            return cell.GridItem;
        }

        private void UnRegisterEvents()
        {
            GridEvents.SwapGridItems -= OnSwapGridItems;
            GridEvents.RevertSwipe -= OnRevertSwipe;
            GridEvents.GetGridItem = null;
        }

        [Serializable]
        public class Grid2D : UnityDictionary<Vector2Int, Cell>
        {
            [SerializeField] private Vector2Int _size = Vector2Int.zero;
            [SerializeField] private Bounds _bounds = new();
            public Vector2Int Size => _size;
            public Bounds Bounds => _bounds;

            public new void Add(Vector2Int key, Cell value)
            {
                base.Add(key, value);

                _size = new Vector2Int
                (Mathf.Max(_size.x, key.x + 1), Mathf.Max(_size.y, key.y + 1));

                _bounds.Encapsulate(value.SpriteRenderer.bounds);
            }

            public bool IsInBounds(Vector3 itemPos)
            {
                return _bounds.Contains(itemPos);
            }

            public bool IsInBounds(Vector2Int neighPos)
            {
                return neighPos.x >= 0 &&
                neighPos.x < _size.x &&
                neighPos.y >= 0 &&
                neighPos.y < _size.y;
            }
        }

        [Serializable]
        public class Settings
        {
            [SerializeField] private List<GridItemData> _gridItemSettingsDatas;
            [SerializeField] private float _dropAnimDelay = 0.1f;
            [SerializeField] private float _dropAnimDur = 0.5f;
            public List<GridItemData> GridItemData => _gridItemSettingsDatas;
            public float DropAnimDelay => _dropAnimDelay;
            public float DropAnimDur => _dropAnimDur;
        }
#if UNITY_EDITOR

        [Button]
        void IGridEditorAccess.CreateGrid(Vector2Int size)
        {
            if (size == Vector2Int.zero) size = new Vector2Int(8, 8);

            if (_grid2D is {Count: > 0}) ((IGridEditorAccess)this).DiscardGrid();

            Vector3 boundsSize = ((IGridEditorAccess)this).GetGridPadding();

            _grid2D = new Grid2D();

            for (int x = 0; x < size.x; x ++)
            {
                for (int y = 0; y < size.y; y ++)
                {
                    Cell instantiateCell = ((IGridEditorAccess)this).InstantiateCell
                    (x, y, boundsSize);

                    _grid2D.Add(new Vector2Int(x, y), instantiateCell);
                }
            }
        }

        Vector3 IGridEditorAccess.GetGridPadding()
        {
            return _cellPrefab.GetComponent<Cell>()
            .SpriteRenderer.bounds.size;
        }

        [Button]
        void IGridEditorAccess.RandomizeGridEditor()
        {
            ((IGridEditorAccess)this).DiscardItems();

            _mySettings = Resources.Load<GridInstallerSettings>
            (EnvironmentVariables.GridInstallerSettingsPath)
            .Settings;

            List<GridItemData> gridItemDatas = new(_mySettings.GridItemData);

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

                ((IGridEditorAccess)this).InstantiateGridItemsEditor(cell, randomGridItemData);
            }
        }

        Cell IGridEditorAccess.InstantiateCell(int x, int y, Vector3 tileSpriteSize)
        {
            Cell instantiateCell = PrefabUtility.InstantiatePrefab(_cellPrefab, gameObject.scene)
            .GetComponent<Cell>();

            ((ICellGridAccess)instantiateCell).Construct(new Vector2Int(x, y));

            instantiateCell.Transform.position = new Vector3
            (x * tileSpriteSize.x, y * tileSpriteSize.y, 0);

            instantiateCell.name = $"Cell {x} {y}";
            instantiateCell.Transform.SetParent(_transform);

            return instantiateCell;
        }

        [Button]
        void IGridEditorAccess.DiscardGrid()
        {
            foreach (KeyValuePair<Vector2Int, Cell> keyValuePair in _grid2D)
            {
                DestroyImmediate(keyValuePair.Value.gameObject);
            }

            _grid2D = new Grid2D();
        }

        void IGridEditorAccess.InstantiateGridItemsEditor
        (Cell cellToAssign, GridItemData gridItemData)
        {
            GridItem instantiateGridItem = PrefabUtility.InstantiatePrefab
            (_gridItemPrefab, cellToAssign.Transform)
            .GetComponent<GridItem>();

            ((IGridItemGridAccess)instantiateGridItem).Construct
            (gridItemData.Clone(), cellToAssign, _grid2D.Bounds);

            ((ICellGridAccess)cellToAssign).AssignGridItem(instantiateGridItem);
        }

        [Button]
        void IGridEditorAccess.DiscardItems()
        {
            foreach (KeyValuePair<Vector2Int, Cell> keyValuePair in _grid2D)
            {
                DestroyCellItem(keyValuePair.Value);
            }
        }

#endif
    }

    [Serializable]
    public class Hint
    {
        public readonly List<GridItem> GridItems;

        public Hint(List<GridItem> gridItems)
        {
            GridItems = gridItems;
        }

        [Button]
        public void HintMatch()
        {
            GridItems.DoToAll
            (
                gridItem => gridItem.Transform.DOShakeScale
                (
                    0.5f,
                    0.5f,
                    10,
                    90
                )
            );
        }
    }

    [Serializable]
    public class ItemSpawner
    {
        [SerializeField] private Vector3 _spawnPos;
        [SerializeField] private bool _canSpawn;
        [SerializeField] private Bounds _gridBounds;
        public bool CanSpawn => _canSpawn;
        public Vector3 SpawnPos => _spawnPos;

        public ItemSpawner(Vector3 spawnPos, Bounds gridBounds)
        {
            _spawnPos = spawnPos;
            _gridBounds = gridBounds;
        }

        public void SetActive(bool isActive = true)
        {
            _canSpawn = isActive;
        }

        public GridItem InstantiateGridItem
        (ZenjectPool zenjectPool, Cell cellToAssign, GridItemData gridItemData)
        {
            if (_canSpawn == false) return null;

            GridItem instantiateGridItem = zenjectPool.Request<GridItem>(cellToAssign.Transform);

            ((IGridItemGridAccess)instantiateGridItem).Construct
            (gridItemData.Clone(), cellToAssign, _gridBounds);

            ((ICellGridAccess)cellToAssign).AssignGridItem(instantiateGridItem);
            instantiateGridItem.Transform.SetParent(cellToAssign.Transform);

            return instantiateGridItem;
        }
    }

    public interface IGridEditorAccess
    {
#if UNITY_EDITOR
        void CreateGrid(Vector2Int size);

        Vector3 GetGridPadding();

        void RandomizeGridEditor();

        Cell InstantiateCell(int x, int y, Vector3 tileSpriteSize);

        void DiscardGrid();

        void InstantiateGridItemsEditor(Cell cellToAssign, GridItemData gridItemData);

        void DiscardItems();
#endif
    }
}