using DG.Tweening;
using Events.External;
using Extensions.DoTween;
using Extensions.Unity;
using Extensions.Unity.MonoHelper;
using UnityEngine;
using Zenject;

namespace Components.Main
{
    public class SwipeInput : EventListenerMono, ITweenContainerBind
    {
        [Inject] private GridEvents GridEvents { get; set; }
        [Inject] private CameraEvents CameraEvents { get; set; }
        private Camera _mainCam;
        private GridItem _mouseDownItem;
        private GridItem _mouseUpItem;
        private float _swapAnimDur = 0.3f;
        private RoutineHelper _swipeRoutine;
        public ITweenContainer TweenContainer { get; set; }

        private void Awake()
        {
            TweenContainer = TweenContain.Install(this);

            _swipeRoutine = new RoutineHelper
            (
                this,
                null,
                UpdateSwipe,
                () => true
            );
        }

        private void UpdateSwipe()
        {
            Vector3 mousePosition = Input.mousePosition;

            if (Input.GetMouseButtonDown(0))
            {
                _mouseDownItem = GetGridItemAtPoint(mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _mouseUpItem = GetGridItemAtPoint(mousePosition);

                if (_mouseUpItem != null && _mouseDownItem != null)
                {

                    if ((_mouseDownItem.Coord - _mouseUpItem.Coord).Mag() == 1)
                    {
                        TrySwap(_mouseDownItem, _mouseUpItem);
                    }
                }
                else
                {
                    _mouseDownItem = null;
                }
            }
        }

        private void TrySwap(GridItem mouseDownItem, GridItem mouseUpItem)
        {
            if (AreNeighbors(mouseDownItem, mouseUpItem))
            {
                _swipeRoutine.SetPaused();
                
                DoSwapAnim(mouseDownItem, mouseUpItem)
                .onComplete += delegate
                {
                    GridEvents.SwapGridItems?.Invoke(mouseDownItem.Cell, mouseUpItem.Cell);
                };
            }
        }

        public void RevertSwipe(GridItem mouseDown, GridItem mouseUp)
        {
            DoSwapAnim(mouseDown, mouseUp).onComplete += delegate
            { 
                GridEvents.RevertSwipe?.Invoke(mouseDown, mouseUp);
                _swipeRoutine.SetPaused(false);
            };
        }

        private Sequence DoSwapAnim(GridItem mouseDownItem, GridItem mouseUpItem)
        {
            Transform fromItemTransform = mouseDownItem.transform;
            Transform toItemTransform = mouseUpItem.transform;

            TweenContainer.AddSequence = DOTween.Sequence();

            Tween fromItemTween = fromItemTransform.DOMove(mouseUpItem.Cell.transform.position, _swapAnimDur);
            Tween toItemTween = toItemTransform.DOMove(mouseDownItem.Cell.transform.position, _swapAnimDur);

            TweenContainer.AddedSeq.Append(fromItemTween);
            TweenContainer.AddedSeq.Join(toItemTween);

            return TweenContainer.AddedSeq;
        }

        private bool AreNeighbors(GridItem item1, GridItem item2)
        {
            int distance = CalculateDistanceBetweenCells
            (
                item1.Cell,
                item2.Cell
            );

            return distance == 1;
        }

        private int CalculateDistanceBetweenCells(Cell cell1, Cell cell2)
        {
            Vector2Int dist = cell1.Coord - cell2.Coord;

            return dist.Mag();
        }

        private GridItem GetGridItemAtPoint(Vector2 point)
        {
            Vector3 point3 = point;

            point3.z = 10f;

            Ray ray = _mainCam.ScreenPointToRay(point3);

            RaycastHit2D hit = Physics2D.Raycast
            (
                ray.origin,
                ray.direction,
                Mathf.Infinity
            );

            Collider2D hitCollider = hit.collider;

            if (hitCollider != null)
            {
                if (hitCollider.TryGetComponent(out GridItem gridItem))
                {
                    return gridItem;
                }
            }

            return null;
        }

        protected override void RegisterEvents()
        {
            GridEvents.NoMatches += OnNoMatches;
            GridEvents.Matched += OnMatched;
            CameraEvents.MainCamStart += OnMainCamStart;
        }

        private void OnNoMatches(GridItem mouseDown, GridItem mouseUp)
        {
            RevertSwipe(mouseDown, mouseUp);
        }

        private void OnMatched()
        {
            _swipeRoutine.SetPaused(false);
        }

        private void OnMainCamStart(Camera arg0)
        {
            _mainCam = arg0;
            _swipeRoutine.StartCoroutine();
        }

        protected override void UnRegisterEvents()
        {
            GridEvents.NoMatches -= OnNoMatches;
            GridEvents.Matched -= OnMatched;
            CameraEvents.MainCamStart -= OnMainCamStart;
            TweenContainer.Clear();
        }
    }
}