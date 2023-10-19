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
        ITweenContainer ITweenContainerBind.TweenContainer
        {
            get => TweenContainer;
            set => TweenContainer = value;
        }

        private ITweenContainer TweenContainer { get; set; }
        [Inject] private GridEvents GridEvents { get; set; }
        [Inject] private CameraEvents CameraEvents { get; set; }
        private Camera _mainCam;
        private GridItem _mouseDownItem;
        private GridItem _mouseUpItem;
        private float _swapAnimDur = 0.3f;
        private RoutineHelper _swipeRoutine;
        private Vector3 _mouseDownPos;
        private float _swipeThreshold = 0.5f;

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
                _mouseDownPos = mousePosition;
                SelectItem(GetGridItemAtPoint(mousePosition));
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (_mouseDownItem == null) return;
                
                if (Vector2.Distance(_mouseDownPos, mousePosition) > _swipeThreshold)
                {
                    Vector3 swipeDir = mousePosition - _mouseDownPos;

                    int maxAxisIndex = swipeDir.GetMaxAxisIndex();

                    Vector2Int swipeDirCoord = Vector2Int.zero;

                    swipeDirCoord[maxAxisIndex] = (int)(1 *
                        (swipeDir[maxAxisIndex] / Mathf.Abs(swipeDir[maxAxisIndex])));

                    GridItem swipeDirNeigh = GridEvents.GetGridItem?.Invoke
                    (_mouseDownItem.Coord + swipeDirCoord);

                    if (swipeDirNeigh != null)
                    {
                        SwapItems(_mouseDownItem, swipeDirNeigh);
                    }
                }
                
                SelectItem(null);
            }
        }

        private void SelectItem(GridItem getGridItemAtPoint)
        {
            if (_mouseDownItem)
            {
                ((ISelectable)_mouseDownItem).OnDeselect();
            }
            
            if (getGridItemAtPoint)
            {
                ((ISelectable)getGridItemAtPoint).OnSelect();
            }
            
            _mouseDownItem = getGridItemAtPoint;
        }

        private void SwapItems(GridItem mouseDownItem, GridItem mouseUpItem)
        {
            _swipeRoutine.SetPaused();

            DoSwapAnim(mouseDownItem, mouseUpItem)
            .onComplete += delegate
            {
                GridEvents.SwapGridItems?.Invoke(mouseDownItem.Cell, mouseUpItem.Cell);
            };
        }

        public void RevertSwipe(GridItem mouseDown, GridItem mouseUp)
        {
            DoSwapAnim(mouseDown, mouseUp)
            .onComplete += delegate
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

            Tween fromItemTween = fromItemTransform.DOMove
            (mouseUpItem.Cell.transform.position, _swapAnimDur);

            Tween toItemTween = toItemTransform.DOMove
            (mouseDownItem.Cell.transform.position, _swapAnimDur);

            TweenContainer.AddedSeq.Append(fromItemTween);
            TweenContainer.AddedSeq.Join(toItemTween);

            return TweenContainer.AddedSeq;
        }
        
        private GridItem GetGridItemAtPoint(Vector2 point)
        {
            Vector3 point3 = point;

            point3.z = 10f;

            Ray ray = _mainCam.ScreenPointToRay(point3);

            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);

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