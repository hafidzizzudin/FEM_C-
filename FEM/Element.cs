using System;
using System.Collections.Generic;
using System.Text;

namespace FEM
{
  class Element
  {
    public static int Count = 0;

    public Node First { set; get; }
    public Node Second { set; get; }
    public List<List<double>> ElmStifMatrix { set; get; } = new List<List<double>>();
    public int ID { set; get; }
    public double Length { set; get; }
    public double Area { set; get; }
    public double E { set; get; }
    public double StrainEng { set; get; }
    public double StrainTrue { set; get; }
    public double StressEng { set; get; }
    public double StressTrue { set; get; }

    public Element( Node first, Node second, double r, double e )
    {
      First = first;
      Second = second;
      Area = 22 * r * r / 7;
      E = e;
      ID = ++Count;

      double dx = First.X - Second.X;
      double dy = First.Y - Second.Y;
      double dz = First.Z - Second.Z;
      Length = Math.Sqrt( Math.Pow( dx, 2 ) + Math.Pow( dy, 2 ) + Math.Pow( dz, 2 ) );

      //BAR ELEMENT 3D CREATOR
      double Cx = dx / Length, Cy = dy / Length, Cz = dz / Length;

      for( int i = 0; i < 6; i++ )
      {
        List<double> temprow = new List<double>( 6 ) { 0, 0, 0, 0, 0, 0 };
        ElmStifMatrix.Add( temprow );

        for( int j = 0; j < 6; j++ )
        {
          int[] index = new int[ 2 ];
          double[] C = new double[ 2 ];

          index[ 0 ] = i % 3 + 1;
          index[ 1 ] = j % 3 + 1;
          for( int k = 0; k < C.Length; k++ )
          {
            if( index[ k ] == 1 )
            {
              C[ k ] = Cx;
            }
            else if( index[ k ] == 2 )
            {
              C[ k ] = Cy;
            }
            else if( index[ k ] == 3 )
            {
              C[ k ] = Cz;
            }
          }

          ElmStifMatrix[ i ][ j ] = Area * E * C[ 0 ] * C[ 1 ] / Length;

          if( Math.Abs( ElmStifMatrix[ i ][ j ] ) < 1e-100 )
          {
            ElmStifMatrix[ i ][ j ] = 0;
            continue;
          }

          if( ( i < 3 && j < 3 ) || ( i >= 3 && j >= 3 ) )
          {
            continue;
          }
          else
          {
            ElmStifMatrix[ i ][ j ] = -ElmStifMatrix[ i ][ j ];
          }
        }
      }
    }
    public void ViewStiffness()
    {
      for( int i = 0; i < 6; i++ )
      {
        for( int j = 0; j < 6; j++ )
        {
          Console.Write( $"{ElmStifMatrix[ i ][ j ]} " );
        }
        Console.Write( "\n" );
      }
      Console.WriteLine();
    }
  }


}
