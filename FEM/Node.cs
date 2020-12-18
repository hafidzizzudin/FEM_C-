namespace FEM
{
  class Node
  {
    public static int Count = 0;

    public int ID { set; get; }

    public Node() { Count++; }

    //Coordinate
    public double X { set; get; }
    public double Y { set; get; }
    public double Z { set; get; }

    //Displacement
    public double UX { set; get; }
    public double UY { set; get; }
    public double UZ { set; get; }

    //Force
    public double FX { set; get; }
    public double FY { set; get; }
    public double FZ { set; get; }
  }
}
