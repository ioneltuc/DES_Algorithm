namespace DES_Algorithm
{
    public class BlockEncoder
    {
        public string HexadecimalMessage { private get; set; }
        private int[] _binaryMessage;
        private int[] _leftHalf;
        private int[] _rightHalf;
        private List<SubKey> _subKeys;

        public BlockEncoder(List<SubKey> subKeys)
        {
            _binaryMessage = new int[64];
            _leftHalf = new int[32];
            _rightHalf = new int[32];
            _subKeys = subKeys.ToList();
        }

        private int[] CreateAndReturn64bitMessage()
        {
            string binaryMessage = "";

            foreach (var c in HexadecimalMessage)
            {
                int value = Convert.ToInt32(c.ToString(), 16);
                binaryMessage += Convert.ToString(value, 2).PadLeft(4, '0');
            }

            for (int i = 0; i < _binaryMessage.Length; i++)
            {
                _binaryMessage[i] = int.Parse(binaryMessage[i].ToString());
            }

            return _binaryMessage;
        }

        private void ApplyInitialPermutation()
        {
            int[] binaryMessage = CreateAndReturn64bitMessage();
            int[] messageAfterPermutation = new int[binaryMessage.Length];
            int messageIndex = 0;

            for (int i = 0; i < DefaultTables.IP.GetLength(0); i++)
            {
                for (int j = 0; j < DefaultTables.IP.GetLength(1); j++)
                {
                    messageAfterPermutation[messageIndex] = binaryMessage[DefaultTables.IP[i, j] - 1];
                    messageIndex++;
                }
            }

            DividePermutedBlockIntoHalves(messageAfterPermutation);
        }

        private void DividePermutedBlockIntoHalves(int[] block)
        {
            Array.Copy(block, _leftHalf, 32);
            Array.Copy(block, 32, _rightHalf, 0, 32);
        }

        private void AlgorithmPerRound(int[] leftHalf, int[] rightHalf, int roundNumber)
        {
            _leftHalf = (int[])rightHalf.Clone();
            _rightHalf = XOR(leftHalf, SecretFunctionF(rightHalf, _subKeys[roundNumber].FinalBits));
        }

        private void Perform16Rounds()
        {
            ApplyInitialPermutation();

            for (int i = 0; i < 16; i++)
            {
                AlgorithmPerRound(_leftHalf, _rightHalf, i);
            }
        }

        private int[] PerformFinalPermutation()
        {
            Perform16Rounds();

            int[] reversedBlock = new int[64];
            int[] finalBlock = new int[64];
            int finalBlockIndex = 0;

            reversedBlock = _rightHalf.Concat(_leftHalf).ToArray();

            for (int i = 0; i < DefaultTables.IPPowerNegativeOne.GetLength(0); i++)
            {
                for (int j = 0; j < DefaultTables.IPPowerNegativeOne.GetLength(1); j++)
                {
                    finalBlock[finalBlockIndex] = reversedBlock[DefaultTables.IPPowerNegativeOne[i, j] - 1];
                    finalBlockIndex++;
                }
            }

            return finalBlock;
        }

        public string EncryptBlock()
        {
            int[] encryptedBlock = PerformFinalPermutation();
            string stringEncryptedBlock = "";

            Console.WriteLine($"\nMessage block in hexadecimal: {HexadecimalMessage}");

            Console.Write("Message block in binary:");
            for (int i = 0; i < _binaryMessage.Length; i++)
            {
                if (i % 8 == 0)
                    Console.Write(" ");

                Console.Write(_binaryMessage[i]);

                stringEncryptedBlock += encryptedBlock[i].ToString();
            }

            return stringEncryptedBlock;
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
    }
}