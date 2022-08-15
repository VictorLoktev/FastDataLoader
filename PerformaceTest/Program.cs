using System;

namespace PerformaceTest
{
    internal class Program
    {
        static void Main()
        {
            Console.WriteLine( "Worm up!" );
            CompareSpeed.Run( 1, 1 );
            Console.WriteLine( "\r\nRun!" );

            for( int i = 0; i < 20; i++ )
            {
                CompareSpeed.Run( 10, 50_000 );
            }

            for( int i = 0; i < 50; i++ )
            {
                CompareSpeed.Run( 1, 1_000_000 );
                CompareSpeed.Run( 100, 1_000_000 );
                CompareSpeed.Run( 10_000, 100_000 );
            }
            Console.WriteLine( "Well done!" );
        }
    }
}
