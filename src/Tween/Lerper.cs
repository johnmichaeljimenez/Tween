using System;
using System.Collections.Generic;

namespace TweenLib
{
    public static class Lerper
    {
        private static readonly Dictionary<Type, Delegate> lerpers = new Dictionary<Type, Delegate>
        {
            [typeof(float)] = (Func<float, float, float, float>)((from, to, t) => from + (to - from) * t),
            [typeof(double)] = (Func<double, double, float, double>)((from, to, t) => from + (to - from) * t),
            [typeof(decimal)] = (Func<decimal, decimal, float, decimal>)((from, to, t) => from + (to - from) * (decimal)t),
            [typeof(int)] = (Func<int, int, float, int>)((from, to, t) => (int)Math.Round(from + (to - from) * t)),
            [typeof(uint)] = (Func<uint, uint, float, uint>)((from, to, t) => (uint)Math.Round(from + (to - from) * t)),
            [typeof(long)] = (Func<long, long, float, long>)((from, to, t) => (long)Math.Round(from + (to - from) * (double)t)),
            [typeof(ulong)] = (Func<ulong, ulong, float, ulong>)((from, to, t) => (ulong)Math.Round(from + (to - from) * (double)t)),
            [typeof(short)] = (Func<short, short, float, short>)((from, to, t) => (short)Math.Round(from + (to - from) * t)),
            [typeof(ushort)] = (Func<ushort, ushort, float, ushort>)((from, to, t) => (ushort)Math.Round(from + (to - from) * t)),
            [typeof(byte)] = (Func<byte, byte, float, byte>)((from, to, t) => (byte)Math.Round(from + (to - from) * t)),
            [typeof(sbyte)] = (Func<sbyte, sbyte, float, sbyte>)((from, to, t) => (sbyte)Math.Round(from + (to - from) * t)),
        };

        public static void Register<T>(Func<T, T, float, T> lerp) where T : struct
        {
            if (lerp == null)
                throw new ArgumentNullException(nameof(lerp));
            lerpers[typeof(T)] = lerp;
        }

        public static bool IsRegistered<T>() where T : struct => lerpers.ContainsKey(typeof(T));

        public static Func<T, T, float, T> Get<T>() where T : struct
        {
            if (lerpers.TryGetValue(typeof(T), out var lerp))
                return (Func<T, T, float, T>)lerp;

            throw new ArgumentException($"No lerper registered for {typeof(T).Name}. Register one with Lerper.Register<T>.");
        }
    }
}