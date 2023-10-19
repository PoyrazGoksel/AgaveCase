using System;
using Components.Main;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace Events.External
{
    [UsedImplicitly]
    public class GridEvents
    {
        public UnityAction<Cell, Cell> SwapGridItems;
        public UnityAction<GridItem, GridItem> NoMatches;
        public UnityAction<GridItem, GridItem> RevertSwipe;
        public UnityAction Matched;
        public UnityAction<Bounds> GridStart;
        public Func<Vector2Int, GridItem> GetGridItem;
    }
}