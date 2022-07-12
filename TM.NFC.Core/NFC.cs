using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TM.NFC.Core.CardCore;
using TM.NFC.Core.PcscCore;

namespace TM.NFC.Core
{
    public class NFC
    {
        private PcscReader _pcscReader = new PcscReader();
        private CardPolling _cardPolling;
        private static string PICC = "";
        public void Initial()
        {
            try
            {

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
