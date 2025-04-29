using System;
using System.Collections.Generic;
using System.Linq;

/* Copyright (c) 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 * 
 * The MIT License (MIT)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 * ============= Description =============
 * Implements the A* pathfinding algorithm for 2D grid-based navigation.
 * The algorithm finds the shortest path between two points on a grid, avoiding unwalkable cells.
 * Supports customizable grid types and walkability rules via a generic parameter and delegate.
 * Includes path simplification to reduce unnecessary waypoints in the resulting path.
 * Contains a simple priority queue implementation optimized for use in Unity.
 *
 * ============= Usage =============
 * var grid = new int[width, height]; // Your grid representation
 * var path = AStarPathfinder.FindPath(grid, start, goal, isWalkable);
 * var simplified = AStarPathfinder.SimplifyPath(path);
 */
namespace PiDev.Utilities
{
    public static class AStarPathfinder
    {
        public static List<(int x, int y)> FindPath<T>(
            T[,] grid,
            (int x, int y) start,
            (int x, int y) goal,
            Func<T, bool> isWalkable)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            var openSet = new PriorityQueue<(int, int)>();
            var cameFrom = new Dictionary<(int, int), (int, int)>();
            var gScore = new Dictionary<(int, int), int>();
            var fScore = new Dictionary<(int, int), int>();

            gScore[start] = 0;
            fScore[start] = Heuristic(start, goal);
            openSet.Enqueue(start, fScore[start]);

            while (openSet.Count > 0)
            {
                (int x, int y) = openSet.Dequeue();

                if ((x, y) == goal)
                    return ReconstructPath(cameFrom, goal);

                foreach (var (nx, ny) in GetNeighbors(x, y, width, height))
                {
                    if (!isWalkable(grid[nx, ny])) continue;

                    int tentativeGScore = gScore.GetValueOrDefault((x, y), int.MaxValue) + 1;
                    if (tentativeGScore < gScore.GetValueOrDefault((nx, ny), int.MaxValue))
                    {
                        cameFrom[(nx, ny)] = (x, y);
                        gScore[(nx, ny)] = tentativeGScore;
                        fScore[(nx, ny)] = tentativeGScore + Heuristic((nx, ny), goal);

                        if (!openSet.Contains((nx, ny)))
                            openSet.Enqueue((nx, ny), fScore[(nx, ny)]);
                    }
                }
            }

            return new List<(int, int)>(); // No path found
        }

        private static int Heuristic((int, int) a, (int, int) b)
        {
            return Math.Abs(a.Item1 - b.Item1) + Math.Abs(a.Item2 - b.Item2);
        }

        private static List<(int, int)> GetNeighbors(int x, int y, int width, int height)
        {
            var neighbors = new List<(int, int)>
        {
            (x - 1, y), (x + 1, y),
            (x, y - 1), (x, y + 1)
        };

            neighbors.RemoveAll(pos => pos.Item1 < 0 || pos.Item1 >= width || pos.Item2 < 0 || pos.Item2 >= height);
            return neighbors;
        }

        private static List<(int, int)> ReconstructPath(Dictionary<(int, int), (int, int)> cameFrom, (int, int) current)
        {
            var path = new List<(int, int)> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }
            path.Reverse();
            return path;
        }

        public static List<(int, int)> SimplifyPath(List<(int, int)> path)
        {
            if (path == null || path.Count < 3)
                return path;

            var simplifiedPath = new List<(int, int)> { path[0] };
            (int, int) direction = (path[1].Item1 - path[0].Item1, path[1].Item2 - path[0].Item2);

            for (int i = 1; i < path.Count - 1; i++)
            {
                var newDirection = (path[i + 1].Item1 - path[i].Item1, path[i + 1].Item2 - path[i].Item2);
                if (newDirection != direction)
                {
                    simplifiedPath.Add(path[i]);
                    direction = newDirection;
                }
            }

            simplifiedPath.Add(path[path.Count - 1]);
            return simplifiedPath;
        }
    }

    // Simple Priority Queue for Unity (Min-Heap)
    public class PriorityQueue<T>
    {
        private readonly SortedDictionary<int, Queue<T>> _elements = new SortedDictionary<int, Queue<T>>();
        public int Count { get; private set; } = 0;

        public void Enqueue(T item, int priority)
        {
            if (!_elements.ContainsKey(priority))
                _elements[priority] = new Queue<T>();

            _elements[priority].Enqueue(item);
            Count++;
        }

        public T Dequeue()
        {
            if (Count == 0) throw new InvalidOperationException("Queue is empty");

            var firstKey = _elements.Keys.First();
            var item = _elements[firstKey].Dequeue();
            if (_elements[firstKey].Count == 0)
                _elements.Remove(firstKey);

            Count--;
            return item;
        }

        public bool Contains(T item)
        {
            return _elements.Values.Any(q => q.Contains(item));
        }
    }
}