using System;

namespace PerformaceTest
{
    internal class Program
    {
        static void Main()
        {
            Console.WriteLine( "Worm up!" );
            CompareSpeed.Run( 1, 1 );

            for( int i = 0; i < 10; i++ )
            {
                CompareSpeed.Run( 1, 10_000_000 );
                CompareSpeed.Run( 100, 1_000_000 );
                CompareSpeed.Run( 10_000, 100_000 );
            }
            Console.WriteLine( "Well done!" );
        }
    }
}
