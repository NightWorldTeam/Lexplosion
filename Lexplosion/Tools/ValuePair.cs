namespace Lexplosion.Tools
{
    public struct ValuePair<T, U>
    {
        public T Value1;
        public U Value2;
    }

    public struct ValuePair<T, U, G>
    {
        public T Value1;
        public U Value2;
        public G Value3;
    }
}
