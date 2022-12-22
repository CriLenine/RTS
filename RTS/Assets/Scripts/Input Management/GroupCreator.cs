using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupCreator
{
    /// <summary>
    /// Clusterize a list of Character taking into account the obstacles
    /// </summary>
    /// <para name="peons">Characters to clusterize</para>
    /// <para name="maxSqrtMagnitude">The square of the maximum distance to the leader of a group</para>
    /// <returns>A list of Character list</returns>
    public static List<List<Character>> MakeGroups(int performer, Character[] peons, float maxSqrtMagnitude = 100f)
    {
        /// <summary>
        /// Check if all characters can be contained in a square without obstacles
        /// </summary>
        bool IsComplexGroupsNeeded(Character[] characters)
        {
            if (characters.Length < 2)
                return false;

            int minX = characters[0].Coords.x;
            int maxX = characters[0].Coords.x;
            int minY = characters[0].Coords.y;
            int maxY = characters[0].Coords.y;

            for (int i = 1; i < characters.Length; ++i)
            {
                Vector2Int coords = characters[i].Coords;

                if (coords.x < minX)
                    minX = coords.x;

                if (coords.x > maxX)
                    maxX = coords.x;

                if (coords.y < minY)
                    minY = coords.y;

                if (coords.y > maxY)
                    maxY = coords.y;
            }

            return TileMapManager.ObstacleDetection(performer, minX, maxX, minY, maxY);
        }

        List<List<Character>> groups = new List<List<Character>>();

        if (!IsComplexGroupsNeeded(peons))
        {
            groups.Add(new List<Character>(peons));

            return groups;
        }

        List<Character> openSet = new List<Character>(peons);

        /// <summary>
        /// Return the Character closest to the center of gravity of openSet
        /// </summary>
        int GetLeader()
        {
            Vector2 gravityCenter = Vector2.zero;

            for (int i = 0; i < peons.Length; ++i)
                gravityCenter += (Vector2)peons[i].transform.position;

            gravityCenter /= peons.Length;

            float bestSrtMagnitude = Vector2.SqrMagnitude((Vector2)openSet[0].transform.position - gravityCenter), sqrtMagnitude;
            int bestIndex = 0;

            for (int i = 1; i < openSet.Count; ++i)
            {
                sqrtMagnitude = Vector2.SqrMagnitude((Vector2)openSet[i].transform.position - gravityCenter);

                if (sqrtMagnitude < bestSrtMagnitude)
                {
                    bestIndex = i;

                    bestSrtMagnitude = sqrtMagnitude;
                }
            }

            return bestIndex;
        }

        List<Character> group;

        while (openSet.Count > 0)
        {
            int leaderIndex = GetLeader();

            Character leader = openSet[leaderIndex];

            openSet.RemoveAt(leaderIndex);

            group = new List<Character>() { leader };

            for (int i = 0; i < openSet.Count; ++i)
            {
                if (Vector2.SqrMagnitude(openSet[i].transform.position - leader.transform.position) <= maxSqrtMagnitude)
                {
                    if (TileMapManager.LineOfSight(performer, leader.Coords, openSet[i].Coords))
                    {
                        group.Add(openSet[i]);

                        openSet.RemoveAt(i);

                        --i;
                    }
                }
            }

            groups.Add(group);
        }

        return groups;
    }
}
