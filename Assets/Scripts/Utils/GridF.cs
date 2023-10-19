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
        public static List<GridItem> TryGetMatches(
            Cell cell,
            Grid2DInstaller.Grid2D grid2D,
            int matchCount)
        => TryGetMatches(cell.GridItem.ID, cell, grid2D, matchCount);
        
        public static List<GridItem> TryGetMatches(int gridItemID,
            Cell cell,
            Grid2DInstaller.Grid2D grid2D,
            int matchCount)
        {
            List<GridItem> matchesOnAxis0 = TryMatchesOnAxis
            (
                gridItemID,
                cell,
                grid2D,
                0,
                matchCount
            );

            List<GridItem> matchesOnAxis1 = TryMatchesOnAxis
            (
                gridItemID,
                cell,
                grid2D,
                1,
                matchCount
            );

            List<GridItem> allMatches = matchesOnAxis0.Union(matchesOnAxis1)
            .ToList();
            
            return allMatches;
        }
        
        private static List<GridItem> TryMatchesOnAxis
        (
            int gridItemID,
            Cell cell,
            Grid2DInstaller.Grid2D grid2D,
            int axisIndex,
            int matchCount
        )
        {
            List<GridItem> allMatches = new();
            List<GridItem> matchesNeg = new();
            List<GridItem> matchesPos = new();

            Vector2Int cellPosition = cell.Coord;
            int cellPositionAxis = cellPosition[axisIndex];

            for (int i = cellPositionAxis - 1; i >= 0; i--)
            {
                Vector2Int neighPos = cellPosition;
                neighPos[axisIndex] = i;
                int neighCellID = grid2D[neighPos].ID;

                if (neighCellID != gridItemID) break;

                matchesNeg.Add(grid2D[neighPos].GridItem);
            }
            
            for (int i = cellPositionAxis + 1; i < grid2D.Size[axisIndex]; i++)
            {
                Vector2Int neighPos = cellPosition;
                neighPos[axisIndex] = i;
                int neighCellID = grid2D[neighPos].ID;

                if (neighCellID != gridItemID) break;

                matchesPos.Add(grid2D[neighPos].GridItem);
            }

            if (matchesNeg.Count + matchesPos.Count + 1 >= matchCount)
            {
                allMatches.AddRange(matchesNeg);
                allMatches.Add(cell.GridItem);  
                allMatches.AddRange(matchesPos);
                return allMatches;
            }

            return allMatches;
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
                    e.ID,
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

        private static bool HasMatch
        (
            int gridItemID,
            Cell cell,
            Grid2DInstaller.Grid2D grid2D,
            int matchCount
        )
        {
            bool hasMatchOnAxis0 = HasMatchOnAxis
            (
                gridItemID,
                cell,
                grid2D,
                0,
                matchCount
            );

            bool hasMatchOnAxis1 = HasMatchOnAxis
            (
                gridItemID,
                cell,
                grid2D,
                1,
                matchCount
            );

            return hasMatchOnAxis0 || hasMatchOnAxis1;
        }

        public static List<Cell> GetNeighs(Cell cell, Grid2DInstaller.Grid2D grid2D)
        {
            List<Cell> neighs = new();

            Vector2Int cellPosition = cell.Coord;
            
            for (int x = cellPosition.x - 1; x <= cellPosition.x + 1; x++)
            {
                if (x == cellPosition.x) continue;

                Vector2Int neighPos = new(x, cellPosition.y);

                if (grid2D.IsInBounds(neighPos) == false) continue;

                neighs.Add(grid2D[neighPos]);
            }
            
            for (int y = cellPosition.y - 1; y <= cellPosition.y + 1; y++)
            {
                if (y == cellPosition.y) continue;

                Vector2Int neighPos = new(cellPosition.x, y);

                if (grid2D.IsInBounds(neighPos) == false) continue;

                neighs.Add(grid2D[neighPos]);
            }

            return neighs;
        }
        
        private static bool HasMatchOnAxis
        (
            int gridItemID,
            Cell cell,
            Grid2DInstaller.Grid2D grid2D,
            int axisIndex,
            int matchCount
        )
        {
            int matchingNeighCount = matchCount - 1;
            
            Vector2Int cellPosition = cell.Coord;

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

                if (neighCell.ID != gridItemID) break;

                totalMatch ++;

                if (totalMatch == matchingNeighCount) return true;
            }

            totalMatch = 0;
            
            for (int i = cellPositionAxis + 1; i <= lastPosAxisPos; i ++)
            {
                Vector2Int neighPos = cellPosition;
                neighPos[axisIndex] = i;
                Cell neighCell = grid2D[neighPos];

                if (neighCell.ID != gridItemID) break;

                totalMatch ++;

                if (totalMatch == matchingNeighCount) return true;
            }

            return false;
        }

        private static int Get2DPerpendicularAxis(int axisIndex)
        {
            return axisIndex == 0 ? 1 : 0;
        }
    }
}