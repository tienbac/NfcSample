using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFID;

namespace RFID
{
    class PICCClass : PcscReader
    {
        public PICCClass()
        {
        }

        public byte[] getData(byte uIso14443A)
        {
            if (pcscConnection == null)
                throw new Exception("PCSC Connection is not yet established");

            Apdu apdu = new Apdu();
            apdu.setCommand(new byte[] { 0xFF, 0xCA, uIso14443A, 0x00, 0x00 });
            apdu.lengthExpected = 255;

            pcscConnection.sendCommand(ref apdu);

            if (!apdu.statusWordEqualTo(new byte[] { 0x90, 0x00 }))
                throw new CardException(GetErrorMessage(apdu.statusWord), apdu.statusWord);

            return apdu.response;
        }

        public byte[] sendCommand(int iCaseType, byte uCla, byte uIns, byte uP1, byte uP2, byte uLc, byte uLe, byte[] uData)
        {
            if (pcscConnection == null)
                throw new Exception("PCSC Connection is not yet established");

            Apdu apdu = new Apdu();

            /////////////////////////////////////////////////
            // | CLA | INS | P1 | P2 | Lc | Data Field | Le |
            // |<----- Header ------>|<------- Body ------->|
            /////////////////////////////////////////////////

            switch (iCaseType)
            {
                case 0: // Header
                case 2: // Header-Lc-Data
                case 3: // Header-Lc-Data-Le
                    apdu.setCommand(new byte[] { uCla, uIns, uP1, uP2, uLc });
                    break;

                case 1: // Header Le
                    apdu.setCommand(new byte[] { uCla, uIns, uP1, uP2, uLe });
                    break;

                default:
                    apdu.setCommand(new byte[] { uCla, uIns, uP1, uP2, uLc });
                    break;
            }

            apdu.data = uData;
            apdu.lengthExpected = uLe;

            pcscConnection.sendCommand(ref apdu);

            return apdu.response;
        }

        public string GetErrorMessage(byte[] sw1sw2)
        {
            if (sw1sw2.Length < 2)
                return "Unknown Status Word (statusWord)";

            else if (sw1sw2[0] == 0x6A && sw1sw2[1] == 0x81)
                return "The function is not supported.";

            else if (sw1sw2[0] == 0x63 && sw1sw2[1] == 0x00)
                return "The operation failed.";

            else
                return "Unknown Status Word (" + Helper.byteAsString(sw1sw2, false) + ")";
        }
    }
}
