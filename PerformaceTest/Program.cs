using System;

namespace PerformaceTest
{
    internal class Program
    {
        static void Main( string[] args )
        {
            CompareSpeed.Run( 1 );
            CompareSpeed.Run( 100 );
            CompareSpeed.Run( 10_000 );
            Console.WriteLine( "Well done!" );
        }
    }
}
