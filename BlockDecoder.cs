namespace DES_Algorithm
{
    public class BlockDecoder
    {
        public string BinaryMessage { private get; set; }
        private int[] _binaryMessage;
        private int[] _leftHalf;
        private int[] _rightHalf;
        private List<SubKey> _subKeys;

        public BlockDecoder(List<SubKey> subKeys)
        {
            _binaryMessage = new int[64];
            _leftHalf = new int[32];
            _rightHalf = new int[32];
            _subKeys = subKeys.ToList();
        }

        private void CreateBinaryMessageIntArray()
        {
            _binaryMessage = BinaryMessage.Select(c => c - '0').ToArray();
        }

        private void IPTablePermutation()
        {
            CreateBinaryMessageIntArray();

            int[] cipherAfterPermutation = new int[64];
            int cipherIndex = 0;

            for (int i = 0; i < DefaultTables.IP.GetLength(0); i++)
            {
                for (int j = 0; j < DefaultTables.IP.GetLength(1); j++)
                {
                    cipherAfterPermutation[cipherIndex] = _binaryMessage[DefaultTables.IP[i, j] - 1];
                    cipherIndex++;
                }
            }

            DivideCipherIntoHalves(cipherAfterPermutation);
        }

        private void DivideCipherIntoHalves(int[] cipher)
        {
            Array.Copy(cipher, _leftHalf, 32);
            Array.Copy(cipher, 32, _rightHalf, 0, 32);
        }

        private void Perform16Rounds()
        {
            IPTablePermutation();

            int keyNumber = 15;
            for (int i = 0; i < 16; i++)
            {
                AlgorithmPerRound(_leftHalf, _rightHalf, keyNumber);
                keyNumber--;
            }
        }

        private void AlgorithmPerRound(int[] leftHalf, int[] rightHalf, int roundNumber)
        {
            _leftHalf = (int[])rightHalf.Clone();
            _rightHalf = XOR(leftHalf, SecretFunctionF(rightHalf, _subKeys[roundNumber].FinalBits));
        }

        private int[] SecretFunctionF(int[] rightHalf, int[] key)
        {
            int bitIndex = 0;
            int[] rightHalfAfterExpansion = new int[48];
            int[] keyXORrightHalf = new int[48];

            for (int i = 0; i < DefaultTables.EBitSelectionTable.GetLength(0); i++)
            {
                for (int j = 0; j < DefaultTables.EBitSelectionTable.GetLength(1); j++)
                {
                    rightHalfAfterExpansion[bitIndex] = rightHalf[DefaultTables.EBitSelectionTable[i, j] - 1];
                    bitIndex++;
                }
            }

            keyXORrightHalf = XOR(key, rightHalfAfterExpansion);

            int sTableIndex = 0;
            int[] s1b1_s8b8 = new int[32];
            int s1b1_s8b8Index = 0;

            for (int i = 0; i <= 42; i += 6)
            {
                int firstPosition = keyXORrightHalf[i] == 1 ? 2 : 0;
                int lastPosition = keyXORrightHalf[i + 5] == 1 ? 1 : 0;
                int rowNumber = firstPosition + lastPosition;

                string colNumberInBinary =
                    keyXORrightHalf[i + 1].ToString() +
                    keyXORrightHalf[i + 2].ToString() +
                    keyXORrightHalf[i + 3].ToString() +
                    keyXORrightHalf[i + 4].ToString();
                int colNumber = Convert.ToInt32(colNumberInBinary, 2);

                int numberFromSTable = DefaultTables.STables[sTableIndex][rowNumber, colNumber];
                string numberFromSTableInBinary = Convert.ToString(numberFromSTable, 2).PadLeft(4, '0');
                sTableIndex++;

                for (int j = 0; j < 4; j++)
                {
                    s1b1_s8b8[s1b1_s8b8Index] = int.Parse(numberFromSTableInBinary[j].ToString());
                    s1b1_s8b8Index++;
                }
            }

            int[] finalPermutation = new int[32];
            int finalPermutationIndex = 0;
            for (int i = 0; i < DefaultTables.P.GetLength(0); i++)
            {
                for (int j = 0; j < DefaultTables.P.GetLength(1); j++)
                {
                    finalPermutation[finalPermutationIndex] = s1b1_s8b8[DefaultTables.P[i, j] - 1];
                    finalPermutationIndex++;
                }
            }

            return finalPermutation;
        }

        private int[] XOR(int[] first, int[] second)
        {
            int[] result = new int[first.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Convert.ToInt32(first[i] != second[i]);
            }

            return result;
        }

        private int[] PerformFinalPermutation()
        {
            Perform16Rounds();

            int[] reversedCipher = new int[64];
            int[] finalCipher = new int[64];
            int finalCipherIndex = 0;

            reversedCipher = _rightHalf.Concat(_leftHalf).ToArray();

            for (int i = 0; i < DefaultTables.IPPowerNegativeOne.GetLength(0); i++)
            {
                for (int j = 0; j < DefaultTables.IPPowerNegativeOne.GetLength(1); j++)
                {
                    finalCipher[finalCipherIndex] = reversedCipher[DefaultTables.IPPowerNegativeOne[i, j] - 1];
                    finalCipherIndex++;
                }
            }

            return finalCipher;
        }

        public string DecryptBlock()
        {
            int[] decryptedBlock = PerformFinalPermutation();
            string stringDecryptedBlock = "";

            for (int i = 0; i < _binaryMessage.Length; i++)
            {
                stringDecryptedBlock += decryptedBlock[i].ToString();
            }

            return stringDecryptedBlock;
        }
    }
}