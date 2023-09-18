namespace DES_Algorithm
{
    public class SplittedSubKey
    {
        public int Index { get; set; }
        public int[] LeftHalf { get; set; }
        public int[] RightHalf { get; set; }

        public SplittedSubKey()
        {
            LeftHalf = new int[28];
            RightHalf = new int[28];
        }
    }
}