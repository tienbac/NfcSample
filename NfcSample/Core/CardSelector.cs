using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfcSample.Core
{
    internal class CardSelector
    {
		private PcscReader pcscReader_;
		public PcscReader pcscReader
		{
			get { return this.pcscReader_; }
			set { this.pcscReader_ = value; }
		}

		public string readCardType(byte[] atr, byte atrLen)
		{
			byte[] aRid = new byte[] { 0xA0, 0x00, 0x00, 0x03, 0x06 };
			byte[] aCardIdentifier = new byte[2];
			byte[] aUid = new byte[300];
			string sCardName = "Unknown";

			// Check if Part 3 or Part 4
			if (atr[4] == 0x80 && atr[5] == 0x4F) // Part 3
			{
				// Check RID                
				if (aRid.SequenceEqual(atr.Skip(7).Take(5).ToArray()))
				{
					// Get Card Name
					Array.Copy(atr, 13, aCardIdentifier, 0, 2);

					switch (aCardIdentifier[1])
					{
						case 0:
							sCardName = "Unknown";
							break;
						case 1:
							sCardName = "Mifare Standard 1K";
							break;
						case 2:
							sCardName = "Mifare Standard 4K";
							break;
						case 3:
							sCardName = "Mifare Ultralight";
							break;
						case 4:
							sCardName = "SLE55R_XXXX";
							break;
						case 6:
							sCardName = "SR176";
							break;
						case 7:
							sCardName = "SRI X4K";
							break;
						case 8:
							sCardName = "AT88RF020";
							break;
						case 9:
							sCardName = "AT88SC0204CRF";
							break;
						case 10:
							sCardName = "AT88SC0808CRF";
							break;
						case 11:
							sCardName = "AT88SC1616CRF";
							break;
						case 12:
							sCardName = "AT88SC3216CRF";
							break;
						case 13:
							sCardName = "AT88SC6416CRF";
							break;
						case 14:
							sCardName = "SRF55V10P";
							break;
						case 15:
							sCardName = "SRF55V02P";
							break;
						case 16:
							sCardName = "SRF55V10S";
							break;
						case 17:
							sCardName = "SRF55V02S";
							break;
						case 18:
							sCardName = "TAG IT";
							break;
						case 19:
							sCardName = "LR1512";
							break;
						case 20:
							sCardName = "ICODESLI";
							break;
						case 21:
							sCardName = "TEMPSENS";
							break;
						case 22:
							sCardName = "I.CODE1";
							break;
						case 23:
							sCardName = "PicoPass 2K";
							break;
						case 24:
							sCardName = "PicoPass 2KS";
							break;
						case 25:
							sCardName = "PicoPass 16K";
							break;
						case 26:
							sCardName = "PicoPass 16Ks";
							break;
						case 27:
							sCardName = "PicoPass 16K(8x2)";
							break;
						case 28:
							sCardName = "PicoPass 16Ks(8x2)";
							break;
						case 29:
							sCardName = "PicoPass 32KS(16+16)";
							break;
						case 30:
							sCardName = "PicoPass 32KS(16+8x2)";
							break;
						case 31:
							sCardName = "PicoPass 32KS(8x2+16)";
							break;
						case 32:
							sCardName = "PicoPass 32KS(8x2+8x2)";
							break;
						case 33:
							sCardName = "LRI64";
							break;
						case 34:
							sCardName = "I.CODE UID";
							break;
						case 35:
							sCardName = "I.CODE EPC";
							break;
						case 36:
							sCardName = "LRI12";
							break;
						case 37:
							sCardName = "LRI128";
							break;
						case 38:
							sCardName = "Mifare Mini";
							break;
						case 39:
							sCardName = "my-d move (SLE 66R01P)";
							break;
						case 40:
							sCardName = "my-d NFC (SLE 66RxxP)";
							break;
						case 41:
							sCardName = "my-d proximity 2 (SLE 66RxxS)";
							break;
						case 42:
							sCardName = "my-d proximity enhanced (SLE 55RxxE)";
							break;
						case 43:
							sCardName = "my-d light (SRF 55V01P))";
							break;
						case 44:
							sCardName = "PJM Stack Tag (SRF 66V10ST)";
							break;
						case 45:
							sCardName = "PJM Item Tag (SRF 66V10IT)";
							break;
						case 46:
							sCardName = "PJM Light (SRF 66V01ST)";
							break;
						case 47:
							sCardName = "Jewel Tag";
							break;
						case 48:
							sCardName = "Topaz NFC Tag";
							break;
						case 49:
							sCardName = "AT88SC0104CRF";
							break;
						case 50:
							sCardName = "AT88SC0404CRF";
							break;
						case 51:
							sCardName = "AT88RF01C";
							break;
						case 52:
							sCardName = "AT88RF04C";
							break;
						case 53:
							sCardName = "i-Code SL2";
							break;
						case 54:
							sCardName = "Mifare Plus SL1_2K";
							break;
						case 55:
							sCardName = "Mifare Plus SL1_4K";
							break;
						case 56:
							sCardName = "Mifare Plus SL2_2K";
							break;
						case 57:
							sCardName = "Mifare Plus SL2_4K";
							break;
						case 58:
							sCardName = "Mifare Ultralight C";
							break;
						case 59:
							sCardName = "FeliCa";
							break;
						case 60:
							sCardName = "Melexis Sensor Tag (MLX90129)";
							break;
						case 61:
							sCardName = "Mifare Ultralight EV1";
							break;
						default:
							sCardName = "Unknown";
							break;
					}
				}
				else
				{
					sCardName = "Unknown";
				}
			}
			else // Part 4
			{
				if (atr[4] == 0x00)
				{
				}
				else
				{
					try
					{
						//Send FF CA 01 00 00 to determine if Type A or Type B
						//Success = Type A
						//Fail = Type B
						return getUid();
					}
					catch (PcscException pcscException)
					{
						throw pcscException;
					}
				}
			}

			return sCardName;
		}

		string getUid()
		{
			byte[] aResponse = new byte[100];
			byte[] aCommand = new byte[] { 0xFF, 0xCA, 0x01, 0x00, 0x00 };
			byte[] aStatusWord = new byte[2];

			Apdu cApdu = new Apdu();

			try
			{
				cApdu.setCommand(aCommand);
				cApdu.lengthExpected = 255;

				this.pcscReader_.sendCommand(ref cApdu);

				if (cApdu.statusWord[0] == 0x6A) // Type B
					return "ISO 14443 Part 4 Type B";
				else // ISO 14443 Part 4 Type A; includes ACOS cards
					return "ISO 14443 Part 4 Type A";
			}
			catch (CardException cException)
			{
				throw cException;
			}
			catch (PcscException cException)
			{
				throw cException;
			}

		}
	}
}
