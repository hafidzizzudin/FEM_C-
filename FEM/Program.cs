using System;

namespace FEM
{
  class Program
  {
    static void Main( string[] args )
    {
      try
      {
        System system = new System();

        system.SetFileInput( "D:\\C#\\FEM\\FEM\\Input\\Input.txt" );

        system.BuildSystem();
      }
      catch( Exception e )
      {
        Console.WriteLine( $"Error: {e.Message}" );
      }

    }
  }
}
