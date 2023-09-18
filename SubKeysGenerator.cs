namespace DES_Algorithm
{
    public class SubKeysGenerator
    {
        private string _hexadecimalKey;
        private int[] _binary64bitKey;
        private List<SplittedSubKey> _splittedSubKeys;
        private List<SubKey> _subKeys;

        public SubKeysGenerator(string hexadecimalKey)
        {
            _hexadecimalKey = hexadecimalKey;
            _binary64bitKey = new int[64];
            _splittedSubKeys = new List<SplittedSubKey>();
            _subKeys = new List<SubKey>();
        }

        private int[] CreateAndReturn64bitKey()
        {
            string binaryKey = "";

            foreach (var c in _hexadecimalKey)
            {
                int value = Convert.ToInt32(c.ToString(), 16);
                binaryKey += Convert.ToString(value, 2).PadLeft(4, '0');
            }

            for (int i = 0; i < _binary64bitKey.Length; i++)
            {
                _binary64bitKey[i] = int.Parse(binaryKey[i].ToString());
            }

            return _binary64bitKey;
        }

        private int[] CreateAndReturn56bitKey()
        {
            int[] binary64bitKey = CreateAndReturn64bitKey();
            int[] binary56bitKey = new int[56];
            int keyIndex = 0;

            for (int i = 0; i < DefaultTables.PC1.GetLength(0); i++)
            {
                for (int j = 0; j < DefaultTables.PC1.GetLength(1); j++)
                {
                    binary56bitKey[keyIndex] = binary64bitKey[DefaultTables.PC1[i, j] - 1];
                    keyIndex++;
                }
            }

            return binary56bitKey;
        }

        private int[] LeftCircularRotation(int[] bits, int numberOfShifts)
        {
            int first = bits[0];
            int second = bits[1];
            int[] shiftedBits = new int[bits.Length];

            for (int i = numberOfShifts; i < bits.Length; i++)
            {
                shiftedBits[i - numberOfShifts] = bits[i];
            }

            if (numberOfShifts == 1)
            {
                shiftedBits[^1] = first;
            }

            if (numberOfShifts == 2)
            {
                shiftedBits[^1] = second;
                shiftedBits[^2] = first;
            }

            return shiftedBits;
        }

        private void CreateAllSplittedSubKeys()
        {
            int[] binary56bitKey = CreateAndReturn56bitKey();
            int[] leftHalf = new int[28];
            int[] rightHalf = new int[28];

            Array.Copy(binary56bitKey, leftHalf, 28);
            Array.Copy(binary56bitKey, 28, rightHalf, 0, 28);

            for (int i = 0; i < DefaultTables.LeftShifts.Length; i++)
            {
                var splittedSubKey = new SplittedSubKey();
                splittedSubKey.Index = i + 1;
                splittedSubKey.LeftHalf = LeftCircularRotation(leftHalf, DefaultTables.LeftShifts[i]);
                splittedSubKey.RightHalf = LeftCircularRotation(rightHalf, DefaultTables.LeftShifts[i]);

                _splittedSubKeys.Add(splittedSubKey);

                Array.Copy(splittedSubKey.LeftHalf, leftHalf, 28);
                Array.Copy(splittedSubKey.RightHalf, rightHalf, 28);
            }
        }

        private void CreateFinalSubKeys()
        {
            CreateAllSplittedSubKeys();

            for (int i = 0; i < 16; i++)
            {
                var subKey = new SubKey();
                subKey.Index = i + 1;
                subKey.BitsBeforePermutation = _splittedSubKeys[i].LeftHalf.Concat(_splittedSubKeys[i].RightHalf).ToArray();
                _subKeys.Add(subKey);
            }

            foreach (var subKey in _subKeys)
            {
                int subKeyIndex = 0;
                for (int i = 0; i < DefaultTables.PC2.GetLength(0); i++)
                {
                    for (int j = 0; j < DefaultTables.PC2.GetLength(1); j++)
                    {
                        subKey.FinalBits[subKeyIndex] = subKey.BitsBeforePermutation[DefaultTables.PC2[i, j] - 1];
                        subKeyIndex++;
                    }
                }
            }
        }

        private void PrintSubKeys()
        {
            foreach (var subKey in _subKeys)
            {
                Console.Write($"K{subKey.Index} = ");

                for (int i = 0; i < 48; i++)
                {
                    if (i % 6 == 0)
                        Console.Write(" ");

                    Console.Write(subKey.FinalBits[i]);
                }

                Console.WriteLine();
            }
        }

        public List<SubKey> EncryptionPhaseI()
        {
            CreateFinalSubKeys();

            Console.WriteLine($"Key in hexadecimal: {_hexadecimalKey}");

            Console.Write("Key in binary:");
            for (int i = 0; i < _binary64bitKey.Length; i++)
            {
                if (i % 8 == 0)
                    Console.Write(" ");

                Console.Write(_binary64bitKey[i]);
            }

            Console.WriteLine("\n\nSub keys:");
            PrintSubKeys();

            return _subKeys;
        }
    }
}