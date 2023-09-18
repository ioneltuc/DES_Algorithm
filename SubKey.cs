namespace DES_Algorithm
{
    public class SubKey
    {
        public int Index { get; set; }
        public int[] BitsBeforePermutation { get; set; }
        public int[] FinalBits { get; set; }

        public SubKey()
        {
            BitsBeforePermutation = new int[56];
            FinalBits = new int[48];
        }
    }
}