using System.Drawing;
using Heuristic.Linq;
using Sensum.Framework.Entities;
using Sensum.Framework.Growtopia.Entities.Enums;
using Sensum.Framework.Growtopia.Network;
using Sensum.Framework.Utils.Extensions;

namespace Sensum.Framework.Growtopia.Features;

public class PathFinder(ENetClient client)
{
    private bool isRunning;
    private bool isCancellationRequested;

    /// <summary>
    /// Just to save the world name used to verify if the world is still the same.
    /// </summary>
    private string worldName = null!;

    /// <summary>
    /// How many blocks to skip when moving.
    /// </summary>
    private const byte skip = 4;

    /// <summary>
    /// Finds a path to the specified goal. Blocks the current thread until the task is done.
    /// </summary>
    /// <param name="goalVec">goal pos</param>
    /// <returns>true if finished without problems</returns>
    public bool FindPath(Vector2Int goalVec)
    {
        if (isRunning) return false;
        isRunning = true;
        isCancellationRequested = false;

        if (worldLoaded() == false) return false;

        worldName = client.World.Name ?? "";

        Point start = client.NetAvatar.TilePos.Point;
        Point goal = goalVec.Point;

        if (start == goal)
        {
            isRunning = false;
            return true;
        }

        if (IsHigherByOne(start, goal))
        {
            if (cancelled()) return true;
            sendMoveState(start, goal, goalVec);
            return true;
        }

        var boundary = new Rectangle(0, 0, client.World.Width, client.World.Height);
        var queryable = HeuristicSearch.AStar(start, goal, (step, _) => step.GetFourDirections(1));
        var solutions = (from step in queryable.Except(getWorldObstacles())
            where boundary.Contains(step)
            orderby step.GetManhattanDistance(goal)
            select step);

        solutions.Count();

        int solutionCount = solutions.Count();

        if (solutionCount == 0)
        {
            isRunning = false;
            return false;
        }

        List<Vector2Int> points = [];

        for (int i = 0; i < solutionCount; i++)
        {
            int skipAmount = Math.Min(i + skip, solutionCount - 1);
            var path = solutions.ElementAt(skipAmount);
            points.Add(new Vector2Int(path.X, path.Y));
        }

        foreach (var point in points)
        {
            if (cancelled()) return true;
            if (isInSameWorld() == false) return false;
            if (SafeChecks.PositionCheck(client, point) == false)
            {
                isRunning = false;
                return false;
            }
            sendMoveState(client.NetAvatar.TilePos.Point, point.Point, point);
            Thread.Sleep(getDelay(points.Count, points.IndexOf(point)));
        }

        // Not sure if this really helps to verify as the pos is updated directly locally but it's there just in case.
        while (client.NetAvatar.TilePos != goalVec)
        {
            Thread.Sleep(10);
        }

        isRunning = false;
        return true;
    }

    public bool CanFindPath(Vector2Int goalVec)
    {
        Point start = client.NetAvatar.TilePos.Point;
        Point goal = goalVec.Point;

        if (start == goal)
        {
            return true;
        }

        var boundary = new Rectangle(0, 0, client.World.Width, client.World.Height);
        var queryable = HeuristicSearch.AStar(start, goal, (step, _) => step.GetFourDirections(1));
        var solutions = (from step in queryable.Except(getWorldObstacles())
            where boundary.Contains(step)
            orderby step.GetManhattanDistance(goal)
            select step).ToList();

        int solutionCount = solutions.Count;

        return solutionCount != 0;
    }

    private int getDelay(int pointCount, int currentPoint)
    {
        return (int) Math.Round((pointCount + currentPoint) * 1.5f); // Higher -> delay is longer
    }

    private void sendMoveState(in Point start, in Point goal, in Vector2Int goalVec)
    {
        var visualState = goal.X == start.X ? client.NetAvatar.VisualState : goal.X > start.X ? VisualState.StandingRight : VisualState.StandingLeft;
        client.SendPlayerState(visualState, goalVec.ToWorldPosition(), Vector2Int.NEGATIVE);
        isRunning = false;
    }

    private bool cancelled()
    {
        if (isCancellationRequested)
        {
            isCancellationRequested = false;
            isRunning = false;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Cancels the current pathfinding operation.
    /// </summary>
    public void Cancel()
    {
        isCancellationRequested = true;
    }

    private bool isInSameWorld()
    {
        return client.World.Name == worldName;
    }

    private bool worldLoaded()
    {
        if (client.World.Loaded) return true;
        isRunning = false;
        return false;
    }

    private List<Point> getWorldObstacles()
    {
        List<Point> obstacles = [];
        if (client.World.WorldTileMap.Tiles == null)
        {
            return obstacles;
        }
        var tiles = client.World.WorldTileMap.Tiles.ToArray();
        obstacles.AddRange(from tile in tiles where tile.IsCollideable() select tile.Pos.Point);
        return obstacles;
    }

    private static bool IsHigherByOne(Point pos1, Point pos2)
    {
        return (pos2.X - pos1.X == 1 && pos2.Y - pos1.Y == 0) || (pos2.X - pos1.X == 0 && pos2.Y - pos1.Y == 1);
    }
}