/* 
 * Original Implementation:
 * ------------------------
 * Smallest enclosing circle - Library (C#)
 * 
 * Copyright (c) 2020 Project Nayuki
 * https://www.nayuki.io/page/smallest-enclosing-circle
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program (see COPYING.txt and COPYING.LESSER.txt).
 * If not, see <http://www.gnu.org/licenses/>.
 * 
 * VL-Adoption by schnellebuntebilder/sebl.
 */


namespace VL.Lib.Mathematics;

public sealed class SmallestEnclosingCircle
{
    /// <summary>
    /// Returns the smallest circle that encloses all the given points. Runs in expected O(n) time, randomized.
    /// Note: If 0 points are given, a circle of radius -1 is returned. If 1 point is given, a circle of radius 0 is returned.
    /// </summary>
    /// <param name="points"> Spread of Vector2</param>
    /// <returns></returns>
    public static Circle SmallestCircle(Spread<Vector2> points)
    {
        // Clone list to preserve the caller's data, do Durstenfeld shuffle
        List<Vector2> shuffled = points.ToList<Vector2>();
        Random rand = new Random();
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = rand.Next(i + 1);
            Vector2 temp = shuffled[i];
            shuffled[i] = shuffled[j];
            shuffled[j] = temp;
        }

        // Progressively add points to circle or recompute circle
        Circle c = CircleExtensions.INVALID;
        for (int i = 0; i < shuffled.Count; i++)
        {
            Vector2 p = shuffled[i];
            if (c.Radius < 0 || !c.Contains(p))
                c = MakeCircleOnePoint(shuffled.GetRange(0, i + 1), p);
        }
        return c;
    }

    // One boundary point known
    private static Circle MakeCircleOnePoint(List<Vector2> points, Vector2 p)
    {
        Circle circ = new Circle(p, 0);
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 q = points[i];
            if (!circ.Contains(q))
            {
                if (circ.Radius == 0)
                    circ = MakeDiameter(p, q);
                else
                    circ = MakeCircleTwoPoints(points.GetRange(0, i + 1), p, q);
            }
        }
        return circ;
    }

    // Two boundary points known
    private static Circle MakeCircleTwoPoints(List<Vector2> points, Vector2 p, Vector2 q)
    {
        Circle circ = MakeDiameter(p, q);
        Circle left = CircleExtensions.INVALID;
        Circle right = CircleExtensions.INVALID;

        // For each point not in the two-point circle
        Vector2 pq = Vector2.Subtract(q, p);
        foreach (Vector2 r in points)
        {
            if (circ.Contains(r))
                continue;

            // Form a circumcircle and classify it on left or right side
            float cross = pq.Cross(Vector2.Subtract(r, p));
            Circle cir = MakeCircumcircle(p, q, r);
            if (cir.Radius < 0)
                continue;
            else if (cross > 0 && (left.Radius < 0 || pq.Cross(Vector2.Subtract(cir.Center, p)) > pq.Cross(Vector2.Subtract(left.Center, p))))
                left = cir;
            else if (cross < 0 && (right.Radius < 0 || pq.Cross(Vector2.Subtract(cir.Center, p)) < pq.Cross(Vector2.Subtract(right.Center, p))))
                right = cir;
        }

        // Select which circle to return
        if (left.Radius < 0 && right.Radius < 0)
            return circ;
        else if (left.Radius < 0)
            return right;
        else if (right.Radius < 0)
            return left;
        else
            return left.Radius <= right.Radius ? left : right;
    }

    private static Circle MakeDiameter(Vector2 a, Vector2 b)
    {
        Vector2 c = new Vector2((a.X + b.X) / 2, (a.Y + b.Y) / 2);
        return new Circle(c, Math.Max(Vector2.Distance(a, c), Vector2.Distance(b, c)));
    }

    private static Circle MakeCircumcircle(Vector2 a, Vector2 b, Vector2 c)
    {
        // Mathematical algorithm from Wikipedia: Circumscribed circle
        float ox = (Math.Min(Math.Min(a.X, b.X), c.X) + Math.Max(Math.Max(a.X, b.X), c.X)) / 2;
        float oy = (Math.Min(Math.Min(a.Y, b.Y), c.Y) + Math.Max(Math.Max(a.Y, b.Y), c.Y)) / 2;
        float ax = a.X - ox, ay = a.Y - oy;
        float bx = b.X - ox, by = b.Y - oy;
        float cx = c.X - ox, cy = c.Y - oy;
        float d = (ax * (by - cy) + bx * (cy - ay) + cx * (ay - by)) * 2;
        if (d == 0)
            return CircleExtensions.INVALID;
        float x = ((ax * ax + ay * ay) * (by - cy) + (bx * bx + by * by) * (cy - ay) + (cx * cx + cy * cy) * (ay - by)) / d;
        float y = ((ax * ax + ay * ay) * (cx - bx) + (bx * bx + by * by) * (ax - cx) + (cx * cx + cy * cy) * (bx - ax)) / d;
        Vector2 p = new Vector2(ox + x, oy + y);
        float r = Math.Max(Math.Max(Vector2.Distance(p, a), Vector2.Distance(p, b)), Vector2.Distance(p, c));
        return new Circle(p, r);
    }

}


// extensions
internal static class CircleExtensions
{
    private const double MULTIPLICATIVE_EPSILON = 1 + 1e-14;

    /// <summary>
    /// circleContainsVector2
    /// </summary>
    /// <param name="circle"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    internal static bool Contains(this Circle circle, Vector2 point)
    {
        return Vector2.Distance(point, circle.Center) <= circle.Radius * MULTIPLICATIVE_EPSILON;
    }

    /// <summary>
    /// util to make a new invalid circle
    /// </summary>
    internal static readonly Circle INVALID = new Circle(new Vector2(0, 0), -1);
}

internal static class Vector2Extensions
{
    internal static float Cross(this Vector2 t, Vector2 p)
    {
        return t.X * p.Y - t.Y * p.X;
    }
}