using System;

namespace Lexplosion.WPF.NewInterface.Extensions.System
{
    public static class ObjectExtensions
    {
        public static bool IsNumber(this object value)
        {
            if (value == null) return false;

            Type type = value.GetType();
            return type == typeof(sbyte) ||
                   type == typeof(byte) ||
                   type == typeof(short) ||
                   type == typeof(ushort) ||
                   type == typeof(int) ||
                   type == typeof(uint) ||
                   type == typeof(long) ||
                   type == typeof(ulong) ||
                   type == typeof(float) ||
                   type == typeof(double) ||
                   type == typeof(decimal);
        }
    }
}
