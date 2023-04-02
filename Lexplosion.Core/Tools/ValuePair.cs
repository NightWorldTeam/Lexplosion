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

    public class ReferenceTuple<T, U, G>
    {
        public T Value1;
        public U Value2;
        public G Value3;
    }

    public class ReferenceTuple<T, U, G, B>
    {
        public T Value1;
        public U Value2;
        public G Value3;
        public B Value4;
    }
}
