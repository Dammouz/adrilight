using System;

namespace adrilight.Extensions
{
    internal static class ArrayExtensions
    {
        public static void Swap<T>(this T[] array, int index1, int index2)
        {
            var temp = array[index2];
            array[index2] = array[index1];
            array[index1] = temp;
        }
    }
}
