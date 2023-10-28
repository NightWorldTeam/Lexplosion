namespace Lexplosion.WPF.NewInterface.Core.Tools
{
    internal static class HashCodeHelper
    {
        public static int CombineHashCodes(int hash1, int hash2)
        {
            return ((hash1 << 5) + hash1) ^ hash2;
        }
    }
}
