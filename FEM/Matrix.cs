using System;

namespace FEM
{
  class Matrix
  {
    public static double[] GaussElim( double[,] K, double[] u )
    {
      int size = K.GetLength( 0 );

      // LU decomposition without pivoting
      for( int k = 0; k < size - 1; k++ )
      {
        if( K[ k, k ] == 0 )
        { Console.WriteLine( "pivot is zero in Mtx::GaussElim()" ); }
        for( int i = k + 1; i < size; i++ )
        {
          if( K[ i, k ] != 0 )
          {   // tmpx[i][k] can be complex
            double mult = K[ i, k ] / K[ k, k ];
            K[ i, k ] = mult;
            for( int j = k + 1; j < size; j++ )
              K[ i, j ] -= mult * K[ k, j ];
          }
        }
      }

      // forwad substitution for L y = b. y still stored in bb
      for( int i = 1; i < size; i++ )
        for( int j = 0; j < i; j++ )
          u[ i ] -= K[ i, j ] * u[ j ];

      // back substitution for U x = y. x still stored in bb
      for( int i = size - 1; i >= 0; i-- )
      {
        for( int j = i + 1; j < size; j++ )
          u[ i ] -= K[ i, j ] * u[ j ];
        u[ i ] /= K[ i, i ];
      }
      return u;
    }
  }
}
