using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace FEM
{
  class System
  {
    public string FileInput { set; get; }

    private Dictionary<int, Node> Nodes { set; get; } = new Dictionary<int, Node>();

    private Dictionary<int, Element> Elements { set; get; } = new Dictionary<int, Element>();

    private List<double> DisVector { set; get; } = new List<double>();

    private List<int> DisVectorId { set; get; } = new List<int>();

    private List<double> ForceVector { set; get; } = new List<double>();

    private List<int> ForceVectorId { set; get; } = new List<int>();

    private double[,] MatrixStiffnessSystem { set; get; }

    public void SetFileInput( string file ) => FileInput = file;

    public async void BuildSystem()
    {
      string[] inputLines = await File.ReadAllLinesAsync( FileInput );

      ExtractFromFile( inputLines );

      CreateMatrixStiffness();

      //ViewSystem();

      SolveSystem();
    }

    private void SolveSystem()
    {
      int oldMatSize = MatrixStiffnessSystem.GetLength( 0 );
      int newMatSize = oldMatSize - DisVectorId.Count;

      double[,] modStifMatSys = new double[ newMatSize, newMatSize ];
      double[] modForce = new double[ newMatSize ];

      //copy matStifSys
      int rnew = 0;
      for( int r = 0; r < MatrixStiffnessSystem.GetLength( 0 ); r++ )
      {
        if( DisVectorId.Contains( r ) )
          continue;

        modForce[ rnew ] = ForceVector[ r ];

        int cnew = 0;
        for( int c = 0; c < MatrixStiffnessSystem.GetLength( 1 ); c++ )
        {
          if( DisVectorId.Contains( c ) )
          {
            modForce[ rnew ] -= MatrixStiffnessSystem[ r, c ] * DisVector[ c ];
            continue;
          }

          modStifMatSys[ rnew, cnew ] = MatrixStiffnessSystem[ r, c ];

          cnew++;
        }

        rnew++;
      }

      //CheckSymmetry( modStifMatSys );
      //ViewMatrixSystem( modStifMatSys );
      int ctr = 0;
      foreach( var force in modForce )
      {
        Console.WriteLine( $"{++ctr} {force}" );
      }

      modForce = Matrix.GaussElim( modStifMatSys, modForce );

      ctr = 0;
      foreach( var force in modForce )
      {
        Console.WriteLine( $"{++ctr} {force}" );
      }

    }

    private void ViewMatrixSystem( double[,] mat )
    {
      Console.WriteLine( "STIFNESS MATRIX SYSTEM" );

      for( int i = 0; i < mat.GetLength( 0 ); i++ )
      {
        for( int j = 0; j < mat.GetLength( 1 ); j++ )
        {
          Console.Write( $"{mat[ i, j ],6:0.#}" );
        }
        Console.Write( "\n" );
      }
    }

    private bool CheckSymmetry( double[,] mat )
    {
      bool check = true;

      for( int i = 1; i < mat.GetLength( 0 ); i++ )
      {
        for( int j = 0; j < i; j++ )
        {
          if( mat[ i, j ] != mat[ j, i ] )
          {
            check = !check;
            goto lableGoto;
          }
        }
      }

      lableGoto:

      if( check )
      {
        Console.WriteLine( "The stiffness matrix is symmetry" );
      }
      else
      {
        Console.WriteLine( "The stiffness matrix is not symmetry, please check again" );
      }

      return check;
    }

    private void CreateMatrixStiffness()
    {
      foreach( var elm in Elements.Values )
      {
        int idnode1 = elm.First.ID, idnode2 = elm.Second.ID;
        for( int r = 0; r < 6; r++ )
        {
          int R;
          if( r < 3 )
          {
            R = ( idnode1 - 1 ) * 3 + r;
          }
          else
          {
            R = ( idnode2 - 1 ) * 3 + r % 3;
          }

          for( int c = 0; c < 6; c++ )
          {
            int C;
            if( c < 3 )
            {
              C = ( idnode1 - 1 ) * 3 + c;
            }
            else
            {
              C = ( idnode2 - 1 ) * 3 + c % 3;
            }
            MatrixStiffnessSystem[ R, C ] = MatrixStiffnessSystem[ R, C ] + elm.ElmStifMatrix[ r ][ c ];
          }
        }
      }
    }

    public void ViewSystem()
    {
      Console.WriteLine( $"NODES {Node.Count}" );
      foreach( var data in Nodes.Values )
      {
        Console.WriteLine( $"{data.ID} {data.X} {data.Y} {data.Z}" );
      }

      Console.WriteLine( $"ELEMENTS {Element.Count}" );
      foreach( var data in Elements.Values )
      {
        Console.WriteLine( $"{data.ID} {data.First.ID} {data.Second.ID} {data.Area} {data.E}" );
      }

      int ctr = 0;
      Console.WriteLine( $"DISPLACEMENT {DisVector.Count}" );
      foreach( var dis in DisVector )
      {
        Console.WriteLine( $"{ctr / 3 + 1}{( ctr % 3 == 0 ? "UX" : ctr % 3 == 1 ? "UY" : "UZ" )} {DisVector[ ctr++ ]}" );
      }

      ctr = 0;
      Console.WriteLine( $"FORCE {ForceVector.Count}" );
      foreach( var force in ForceVector )
      {
        Console.WriteLine( $"{ctr / 3 + 1}{( ctr % 3 == 0 ? "FX" : ctr % 3 == 1 ? "FY" : "FZ" )} {ForceVector[ ctr++ ]}" );
      }

      if( CheckSymmetry( MatrixStiffnessSystem ) )
        ViewMatrixSystem( MatrixStiffnessSystem );
    }

    private void ExtractFromFile( string[] lines )
    {
      int lineNum = 0;
      while( lineNum < lines.Length )
      {
        var words = lines[ lineNum ].Split( " " );
        if( words.Length == 0 || words[ 0 ][ 0 ] == '$' )
        {
          lineNum++;
          continue;
        }

        switch( words[ 0 ].Trim() )
        {
          case "NODE":
            int n_Node = Convert.ToInt32( lines[ ++lineNum ] );

            //Setup dis, force, matrix stiffness size
            double[] tempDisVec = new double[ 3 * n_Node ];
            DisVector = new List<double>( tempDisVec );
            ForceVector = new List<double>( tempDisVec );
            ForceVector = new List<double>( tempDisVec );
            MatrixStiffnessSystem = new double[ n_Node * 3, n_Node * 3 ];

            for( int ii = 0; ii < n_Node; ii++ )
            {
              var values = lines[ ++lineNum ].Split();
              if( values.Length == 0 || values[ 0 ] == "" || values[ 0 ][ 0 ] == '$' )
              {
                ii--;
                continue;
              }

              //for( int jj = 0; jj < values.Length; jj++ )
              //{
              //  Console.Write( $"[{Convert.ToInt32( values[ jj ] )}] " );
              //  if( jj == values.Length - 1 )
              //    Console.Write( "\n" );
              //}
              var newNode = new Node { ID = Convert.ToInt32( values[ 0 ] ), X = Convert.ToInt32( values[ 1 ] ), Y = Convert.ToInt32( values[ 2 ] ), Z = Convert.ToInt32( values[ 3 ] ) };
              Nodes.Add( newNode.ID, newNode );
            }

            break;
          case "ELEMENT":
            int n_Elm = Convert.ToInt32( lines[ ++lineNum ] );

            for( int ii = 0; ii < n_Elm; ii++ )
            {
              var values = lines[ ++lineNum ].Split();
              if( values.Length == 0 || values[ 0 ] == "" || values[ 0 ][ 0 ] == '$' )
              {
                ii--;
                continue;
              }

              var newElem = new Element( Nodes[ Convert.ToInt32( values[ 1 ] ) ], Nodes[ Convert.ToInt32( values[ 2 ] ) ], double.Parse( values[ 3 ].Replace( ',', '.' ), CultureInfo.InvariantCulture ), double.Parse( values[ 4 ].Replace( ',', '.' ), CultureInfo.InvariantCulture ) );
              Elements.Add( newElem.ID, newElem );
              //newElem.ViewStiffness();
            }

            break;
          case "DISPLACEMENT":
            int n_Dis = Convert.ToInt32( lines[ ++lineNum ] );

            for( int ii = 0; ii < n_Dis; ii++ )
            {
              var values = lines[ ++lineNum ].Split();
              if( values.Length == 0 || values[ 0 ] == "" || values[ 0 ][ 0 ] == '$' )
              {
                ii--;
                continue;
              }

              int cord = -1;
              double val = double.Parse( values[ 3 ].Replace( ',', '.' ), CultureInfo.InvariantCulture );

              if( values[ 2 ] == "ux" )
              {
                Nodes[ Convert.ToInt32( values[ 1 ] ) ].X = val;
                cord = ( Convert.ToInt32( values[ 1 ] ) - 1 ) * 3;
              }
              else if( values[ 2 ] == "uy" )
              {
                Nodes[ Convert.ToInt32( values[ 1 ] ) ].Y = val;
                cord = ( Convert.ToInt32( values[ 1 ] ) - 1 ) * 3 + 1;
              }
              else if( values[ 2 ] == "uz" )
              {
                Nodes[ Convert.ToInt32( values[ 1 ] ) ].Z = val;
                cord = ( Convert.ToInt32( values[ 1 ] ) - 1 ) * 3 + 2;
              }

              if( cord != -1 )
              {
                DisVector[ cord ] = val;
                DisVectorId.Add( cord );
              }
            }
            break;
          case "FORCE":
            int n_Force = Convert.ToInt32( lines[ ++lineNum ] );
            for( int ii = 0; ii < n_Force; ii++ )
            {
              var values = lines[ ++lineNum ].Split();
              if( values.Length == 0 || values[ 0 ] == "" || values[ 0 ][ 0 ] == '$' )
              {
                ii--;
                continue;
              }

              int cord = -1;
              double val = double.Parse( values[ 3 ].Replace( ',', '.' ), CultureInfo.InvariantCulture );

              if( values[ 2 ] == "fx" )
              {
                Nodes[ Convert.ToInt32( values[ 1 ] ) ].FX = val;
                cord = ( Convert.ToInt32( values[ 1 ] ) - 1 ) * 3;
              }
              else if( values[ 2 ] == "fy" )
              {
                Nodes[ Convert.ToInt32( values[ 1 ] ) ].FY = val;
                cord = ( Convert.ToInt32( values[ 1 ] ) - 1 ) * 3 + 1;
              }
              else if( values[ 2 ] == "fz" )
              {
                Nodes[ Convert.ToInt32( values[ 1 ] ) ].FZ = val;
                cord = ( Convert.ToInt32( values[ 1 ] ) - 1 ) * 3 + 2;
              }

              if( cord != -1 )
              {
                ForceVector[ cord ] = val;
                ForceVectorId.Add( cord );
              }
            }
            break;
          default:
            lineNum++;
            continue;
        }

        lineNum++;
      }
    }
  }
}
