using System.Drawing;

namespace JEdit;

public class Extent {
    private static readonly Point Uninitalised = new(-1, -1);

    /// <summary>
    /// Add a new point to the extent, increasing the size of the
    /// extent if the point falls outside its existing range.
    /// </summary>
    public Extent Add(Point point) {
        if (Before(point, Start)) {
            Start = point;
        }
        if (After(point, End)) {
            End = point;
        }
        return this;
    }

    /// <summary>
    /// Reduce the extent to the specified points.
    /// </summary>
    public void Subtract(Point p1, Point p2) {
        if (After(p1, Start)) {
            Start = p1;
        }
        if (Before(p2, End)) {
            End = p2;
        }
    }

    /// <summary>
    /// Clear the extent
    /// </summary>
    public void Clear() {
        Start = Uninitalised;
        End = Uninitalised;
    }

    /// <summary>
    /// Return the start of the extent.
    /// </summary>
    public Point Start { get; private set; } = Uninitalised;

    /// <summary>
    /// Return the end of the extent.
    /// </summary>
    public Point End { get; private set; } = Uninitalised;

    /// <summary>
    /// Return whether point p1 is before point p2 in the extent.
    /// </summary>
    private bool Before(Point p1, Point p2) =>
        p1 == Uninitalised || p2 == Uninitalised ||
        p1.Y < p2.Y || p1.Y == p2.Y && p1.X < p2.X;

    /// <summary>
    /// Return whether point p1 is after point p2 in the extent.
    /// </summary>
    private bool After(Point p1, Point p2) =>
        p1 == Uninitalised || p2 == Uninitalised ||
        p1.Y > p2.Y || p1.Y == p2.Y && p1.X > p2.X;
}