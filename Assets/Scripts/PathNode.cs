using Unity.Mathematics;

public struct PathNode
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    public int x;
    public int y;

    public int index;
    public int previousIndex;

    public int gCost;
    public int hCost;
    public int fCost;

    public bool Solid { get; set; }


    public void CalcH(int x, int y, int2 bPos)
    {
        int dx = math.abs(x - bPos.x);
        int dy = math.abs(y - bPos.y);

        int remaining = math.abs(dx - dy);

        hCost = MOVE_DIAGONAL_COST * math.min(dx, dy) + MOVE_STRAIGHT_COST * remaining;
    }


    public void CalcF()
    {
        fCost = gCost + hCost;
    }
}
