using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TN.NFC.Core.Encrypt
{
    public class HexData
    {
        private string DecimalToHexadecimal(int dec)
        {
            if (dec < 1) return "0";

            int hex = dec;
            string hexStr = string.Empty;

            while (dec > 0)
            {
                hex = dec % 16;

                if (hex < 10)
                    hexStr = hexStr.Insert(0, Convert.ToChar(hex + 48).ToString());
                else
                    hexStr = hexStr.Insert(0, Convert.ToChar(hex + 55).ToString());

                dec /= 16;
            }

            return hexStr;
        }

        private int HexadecimalToDecimal(string hex)
        {
            hex = hex.ToUpper();

            int hexLength = hex.Length;
            double dec = 0;

            for (int i = 0; i < hexLength; ++i)
            {
                byte b = (byte)hex[i];

                if (b >= 48 && b <= 57)
                    b -= 48;
                else if (b >= 65 && b <= 70)
                    b -= 55;

                dec += b * Math.Pow(16, ((hexLength - i) - 1));
            }

            return (int)dec;
        }

        public string ASCIIToHexadecimal(string str)
        {
            string hex = string.Empty;

            for (int i = 0; i < str.Length; ++i)
            {
                string chex = DecimalToHexadecimal(str[i]);

                if (chex.Length == 1)
                    chex = chex.Insert(0, "0");

                hex += chex;
            }

            return hex;
        }

        public string HexadecimalToASCII(string hex)
        {
            string ascii = string.Empty;

            for (int i = 0; i < hex.Length; i += 2)
            {
                ascii += (char)HexadecimalToDecimal(hex.Substring(i, 2));
            }

            return ascii;
        }
    }
}
