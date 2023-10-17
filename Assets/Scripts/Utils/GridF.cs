using System.Collections.Generic;
using System.Linq;
using Components.Main;
using Datas;
using Installers;
using UnityEngine;

namespace Utils
{
    public static class GridF
    {
        //TODO: HERE

        public static List<GridItem> TryGetMatchesU(            GridItemData gridItemData,
            Cell cell,
            Grid2DInstaller.Grid2D grid2D,
            int matchCount)
        {
            List<GridItem> matchesOnAxis0 = TryMatchesOnAxisU
            (
                gridItemData,
                cell,
                grid2D,
                0,
                matchCount
            );

            List<GridItem> matchesOnAxis1 = TryMatchesOnAxisU
            (
                gridItemData,
                cell,
                grid2D,
                1,
                matchCount
            );

            if (matchesOnAxis0 != null && matchesOnAxis1 != null)
            {
                return matchesOnAxis0.Union(matchesOnAxis1)
                .ToList();
            }
            
            if (matchesOnAxis0 != null) return matchesOnAxis0;
            
            if (matchesOnAxis1 != null) return matchesOnAxis1;
            
            return null;
        }
        
        private static List<GridItem> TryMatchesOnAxisU
        (
            GridItemData gridItem,
            Cell cell,
            Grid2DInstaller.Grid2D grid2D,
            int axisIndex,
            int matchCount
        )
        {
            Vector2Int cellPosition = cell.Position;
            int cellPositionAxis = cellPosition[axisIndex];

            List<GridItem> matchesNeg = new();
            for (int i = cellPositionAxis - 1; i >= 0; i--)
            {
                Vector2Int neighPos = cellPosition;
                neighPos[axisIndex] = i;
                GridItem neighCell = grid2D[neighPos].GridItem;

                if (neighCell.ID != gridItem.ID) break;

                matchesNeg.Add(grid2D[neighPos].GridItem);
            }

            List<GridItem> matchesPos = new();
            for (int i = cellPositionAxis + 1; i < grid2D.Size[axisIndex]; i++)
            {
                Vector2Int neighPos = cellPosition;
                neighPos[axisIndex] = i;
                GridItem neighCell = grid2D[neighPos].GridItem;

                if (neighCell.ID != gridItem.ID) break;

                matchesPos.Add(grid2D[neighPos].GridItem);
            }

            if (matchesNeg.Count + matchesPos.Count + 1 >= matchCount)
            {
                List<GridItem> allMatches = new();
                allMatches.AddRange(matchesNeg);
                allMatches.Add(cell.GridItem);  // Adding the current cell to the match
                allMatches.AddRange(matchesPos);
                return allMatches;
            }

            return null;
        }
        
        public static List<GridItemData> GetPlaceableGridItems
        (
            List<GridItemData> gridItemDatas,
            Cell cell,
            Grid2DInstaller.Grid2D grid2D,
            int matchCount
        )
        {
            return gridItemDatas.Where
            (
                e => HasMatch
                (
                    e,
                    cell,
                    grid2D,
                    matchCount
                ) ==
                false &&
                e.ID != -1 &&
                e.ID != 0
            )
            .ToList();
        }

        public static bool HasMatch
        (
            GridItemData gridItem,
            Cell cell,
            Grid2DInstaller.Grid2D grid2D,
            int matchCount
        )
        {
            bool hasMatchOnAxis0 = HasMatchOnAxis
            (
                gridItem,
                cell,
                grid2D,
                0,
                matchCount
            );

            bool hasMatchOnAxis1 = HasMatchOnAxis
            (
                gridItem,
                cell,
                grid2D,
                1,
                matchCount
            );

            return hasMatchOnAxis0 || hasMatchOnAxis1;
        }

        private static bool HasMatchOnAxis
        (
            GridItemData gridItem,
            Cell cell,
            Grid2DInstaller.Grid2D grid2D,
            int axisIndex,
            int matchCount
        )
        {
            int matchingNeighCount = matchCount - 1;
            
            Vector2Int cellPosition = cell.Position;

            int cellPositionAxis = cellPosition[axisIndex];

            int lastPosAxisNeg = cellPositionAxis - matchingNeighCount;
            if (lastPosAxisNeg < 0) lastPosAxisNeg = 0;
            
            int lastPosAxisPos = cellPositionAxis + matchingNeighCount;
            if (lastPosAxisPos >= grid2D.Size[axisIndex]) lastPosAxisPos = grid2D.Size[axisIndex] - 1;

            int totalMatch = 0;
            
            for (int i = cellPositionAxis - 1; i >= lastPosAxisNeg; i --)
            {
                Vector2Int neighPos = cellPosition;
                neighPos[axisIndex] = i;
                Cell neighCell = grid2D[neighPos];

                if (neighCell.ID != gridItem.ID) break;

                totalMatch ++;

                if (totalMatch == matchingNeighCount) return true;
            }

            totalMatch = 0;
            
            for (int i = cellPositionAxis + 1; i <= lastPosAxisPos; i ++)
            {
                Vector2Int neighPos = cellPosition;
                neighPos[axisIndex] = i;
                Cell neighCell = grid2D[neighPos];

                if (neighCell.ID != gridItem.ID) break;

                totalMatch ++;

                if (totalMatch == matchingNeighCount) return true;
            }

            return false;
        }

        // public static List<GridItem> TryGetMatches
        // (GridItemData gridItem, Cell cell, Grid2DInstaller.Grid2D grid2D)
        // {
        //     List<GridItem> horizontalMatches = TryGetMatches
        //     (
        //         gridItem,
        //         cell,
        //         grid2D,
        //         0,
        //         3
        //     );
        //
        //     List<GridItem> verticalMatches = TryGetMatches
        //     (
        //         gridItem,
        //         cell,
        //         grid2D,
        //         1,
        //         3
        //     );
        //
        //     if (horizontalMatches.Count > 0 && verticalMatches.Count > 0)
        //     {
        //         return horizontalMatches.Concat(verticalMatches)
        //         .ToList();
        //     }
        //
        //     if (horizontalMatches.Count > 0) return horizontalMatches;
        //
        //     if (verticalMatches.Count > 0) return verticalMatches;
        //
        //     return null;
        // }
        //
        // private static List<GridItem> TryGetMatches
        // (
        //     GridItemData gridItem,
        //     Cell cell,
        //     Grid2DInstaller.Grid2D grid2D,
        //     int axisIndex,
        //     int matchCount
        // )
        // {
        //     int matchingNeighCount = matchCount - 1;
        //     
        //     Vector2Int cellPosition = cell.Position;
        //
        //     int cellPositionAxis = cellPosition[axisIndex];
        //
        //     int lastPosAxisNeg = cellPositionAxis - matchingNeighCount;
        //     if (lastPosAxisNeg < 0) lastPosAxisNeg = 0;
        //     
        //     int lastPosAxisPos = cellPositionAxis + matchingNeighCount;
        //     if (lastPosAxisPos >= grid2D.Size[axisIndex]) lastPosAxisPos = grid2D.Size[axisIndex] - 1;
        //
        //     List<GridItem> matchesNeg = new();
        //
        //     for (int i = cellPositionAxis - 1; i >= lastPosAxisNeg; i --)
        //     {
        //         Vector2Int neighPos = cellPosition;
        //         neighPos[axisIndex] = i;
        //         GridItem neighCell = grid2D[neighPos].GridItem;
        //
        //         if (neighCell.ID != gridItem.ID) break;
        //
        //         matchesNeg.Add(grid2D[neighPos].GridItem);
        //     }
        //
        //     if (matchesNeg.Count < matchingNeighCount) 
        //     {
        //         matchesNeg.Clear();
        //     }
        //     
        //     List<GridItem> matchesPos = new();
        //
        //     for (int i = cellPositionAxis + 1; i <= lastPosAxisPos; i ++)
        //     {
        //         Vector2Int neighPos = cellPosition;
        //         neighPos[axisIndex] = i;
        //         GridItem neighCell = grid2D[neighPos].GridItem;
        //
        //         if (neighCell.ID != gridItem.ID) break;
        //
        //         matchesPos.Add(grid2D[neighPos].GridItem);
        //     }
        //
        //     if (matchesPos.Count < matchingNeighCount) 
        //     {
        //         matchesPos.Clear();
        //     }
        //     
        //     return matchesNeg.Concat(matchesPos).ToList();
        // }

        private static int Get2DPerpendicularAxis(int axisIndex)
        {
            return axisIndex == 0 ? 1 : 0;
        }
    }
}