using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TN.NFC.Core.MifareCore;

namespace TN.NFC.Core.PcscCore
{
    internal delegate void TransmitApduDelegate(object sender, TransmitApduEventArg e);

    internal class TransmitApduEventArg : EventArgs
    {
        private byte[] data_;
        internal byte[] data
        {
            get => data_;
            set => data_ = value;
        }

        internal TransmitApduEventArg(byte[] data)
        {
            data_ = data;
        }

        internal string GetAsString(bool spaceinBetween)
        {
            if (data_ == null)
                return "";

            string tmpStr = string.Empty;
            foreach (var t in data_)
            {
                tmpStr += $"{t:X2}";

                if (spaceinBetween)
                    tmpStr += " ";
            }
            return tmpStr;
        }
    }
    internal class PcscReader
    {
        private IntPtr hCard_ = new IntPtr();
        private IntPtr hContext_ = new IntPtr();
        private int pProtocol_ = PcscProvider.SCARD_PROTOCOL_T0 | PcscProvider.SCARD_PROTOCOL_T1;
        private int pdwActiveProtocol_;
        private int shareMode_ = PcscProvider.SCARD_SHARE_SHARED;
        private uint _operationControlCode = 0;
        private string _readerName = "";
        private int lastError = 0;
        private Apdu _apduCommand = new Apdu();

        internal event TransmitApduDelegate OnSendCommand;
        internal event TransmitApduDelegate OnReceivedCommand;

        internal PcscReader()
        {
            establishContext();
        }

        internal PcscReader(string readerName)
        {
            _readerName = readerName;
            establishContext();
        }

        internal IntPtr cardHandle
        {
            get { return hCard_; }
            set { hCard_ = value; }
        }

        internal IntPtr resourceMngrContext
        {
            get { return hContext_; }
            set { hContext_ = value; }
        }

        internal int preferedProtocol
        {
            get { return pProtocol_; }
            set { pProtocol_ = value; }
        }

        internal int activeProtocol
        {
            get { return pdwActiveProtocol_; }
            set { pdwActiveProtocol_ = value; }
        }

        internal int shareMode
        {
            get { return shareMode_; }
            set { shareMode_ = value; }
        }

        internal string readerName
        {
            get { return _readerName; }
            set { _readerName = value; }
        }

        internal Apdu apduCommand
        {
            get { return _apduCommand; }
            set { _apduCommand = value; }
        }

        internal PcscReader pcscConnection
        {
            get { return this; }
        }

        internal uint operationControlCode
        {
            get { return _operationControlCode; }
            set { _operationControlCode = value; }
        }

        #region Private Methods

        internal void establishContext()
        {
            int retCode;
            retCode = PcscProvider.SCardEstablishContext(PcscProvider.SCARD_SCOPE_USER, 0, 0, ref hContext_);
            if (retCode != PcscProvider.SCARD_S_SUCCESS)
                throw new Exception("Unable to establish context - " + PcscProvider.GetScardErrMsg(retCode));
        }

        void releaseContext()
        {
            int retCode = PcscProvider.SCardReleaseContext(hContext_);
            if (retCode != PcscProvider.SCARD_S_SUCCESS)
                throw new PcscException(retCode);
        }

        void resetContext()
        {
            releaseContext();
            establishContext();
        }

        #endregion

        internal void connect()
        {
            if (_readerName.Trim() == "")
                throw new Exception("Smartacard reader is not specified");

            connect(_readerName, pProtocol_, shareMode_);
        }

        internal void connect(string readerName)
        {
            _readerName = readerName;
            connect(_readerName, pProtocol_, shareMode_);
        }

        internal void connect(string readerName, int preferedProtocol, int shareMode)
        {
            int returnCode;

            returnCode = PcscProvider.SCardConnect(hContext_, readerName, shareMode, preferedProtocol, ref hCard_, ref pdwActiveProtocol_);
            if (returnCode != PcscProvider.SCARD_S_SUCCESS)
            {
                lastError = returnCode;
                throw new PcscException(returnCode);
            }


            shareMode_ = shareMode;
            pProtocol_ = preferedProtocol;
            _readerName = readerName;
        }

        internal void connectDirect()
        {
            connect(readerName, PcscProvider.SCARD_PROTOCOL_UNDEFINED, PcscProvider.SCARD_SHARE_DIRECT);
        }

        internal void getStatusChange(ref byte[] atr)
        {
            int returnCode;

            PcscProvider.SCARD_READERSTATE state = new PcscProvider.SCARD_READERSTATE();
            state.szReader = _readerName;

            returnCode = PcscProvider.SCardGetStatusChange(hContext_, 3000, ref state, 1);
            if (returnCode != PcscProvider.SCARD_S_SUCCESS)
            {
                lastError = returnCode;
                throw new PcscException(returnCode);
            }

            atr = state.rgbAtr;
        }

        internal string[] getReaderList()
        {
            byte[] returnData;
            byte[] sReaderGroup = null;
            string[] readerList = new string[0];
            string readerString = string.Empty;
            int returnCode;
            int readerCount = 255;

            returnCode = PcscProvider.SCardEstablishContext(PcscProvider.SCARD_SCOPE_USER, 0, 0, ref hContext_);
            if (returnCode != PcscProvider.SCARD_S_SUCCESS)
            {
                lastError = returnCode;
                throw new PcscException(returnCode);
            }

            returnCode = PcscProvider.SCardListReaders(hContext_, null, null, ref readerCount);
            if (returnCode != PcscProvider.SCARD_S_SUCCESS)
            {
                lastError = returnCode;
                throw new PcscException(returnCode);
            }

            returnData = new byte[readerCount];

            returnCode = PcscProvider.SCardListReaders(hContext_, sReaderGroup, returnData, ref readerCount);
            if (returnCode != PcscProvider.SCARD_S_SUCCESS)
                throw new PcscException(returnCode);


            readerString = System.Text.ASCIIEncoding.ASCII.GetString(returnData).Trim('\0');
            readerList = readerString.Split('\0');

            return readerList;
        }

        internal void disconnect()
        {
            int returnValue = PcscProvider.SCardDisconnect(hCard_, PcscProvider.SCARD_UNPOWER_CARD);
            if (returnValue != PcscProvider.SCARD_S_SUCCESS)
                throw new PcscException(returnValue);

            releaseContext();
        }

        internal void sendCommand(ref Apdu apdu)
        {
            apduCommand = apdu;
            sendCommand();
            apdu = apduCommand;
        }

        internal void sendCommand()
        {
            byte[] sendBuff, recvBuff;
            int sendLen, recvLen, returnCode;
            PcscProvider.SCARD_IO_REQUEST ioRequest;

            ioRequest.dwProtocol = pdwActiveProtocol_;
            ioRequest.cbPciLength = 8;

            if (apduCommand.data == null)
                sendBuff = new byte[5];
            else
                sendBuff = new byte[5 + apduCommand.data.Length];


            recvLen = apduCommand.lengthExpected + 2;


            Array.Copy(new byte[] { apduCommand.instructionClass, apduCommand.instructionCode, apduCommand.parameter1, apduCommand.parameter2, apduCommand.parameter3 }, sendBuff, 5);

            if (apduCommand.data != null)
                Array.Copy(apduCommand.data, 0, sendBuff, 5, apduCommand.data.Length);

            sendLen = sendBuff.Length;

            apduCommand.statusWord = new byte[2];
            recvBuff = new byte[recvLen];

            sendCommandTriggerEvent(new TransmitApduEventArg(sendBuff));
            returnCode = PcscProvider.SCardTransmit(hCard_,
                                                ref ioRequest,
                                                sendBuff,
                                                sendLen,
                                                ref ioRequest,
                                                recvBuff,
                                                ref recvLen);
            if (returnCode == 0)
            {
                receivedCommandTriggerEvent(new TransmitApduEventArg(recvBuff.Take(recvLen).ToArray()));
                if (recvLen > 1)
                    Array.Copy(recvBuff, recvLen - 2, apduCommand.statusWord, 0, 2);

                if (recvLen > 2)
                {
                    apduCommand.response = new byte[recvLen - 2];
                    Array.Copy(recvBuff, 0, apduCommand.response, 0, recvLen - 2);
                }
            }
            else
            {
                throw new PcscException(returnCode);
            }
        }

        internal void sendCardControl(ref Apdu apdu, uint controlCode)
        {
            apduCommand = apdu;
            operationControlCode = controlCode;
            sendCardControl();
            apdu = apduCommand;
        }

        internal void sendCardControl()
        {
            byte[] sendBuff, recvbuff;
            int sendLen, recvLen, returnCode, actualLength = 0;
            PcscProvider.SCARD_IO_REQUEST ioRequest;

            ioRequest.dwProtocol = pdwActiveProtocol_;
            ioRequest.cbPciLength = 8;

            if (apduCommand.data == null)
                throw new Exception("No data specified");

            sendBuff = new byte[apduCommand.data.Length];
            recvLen = apduCommand.lengthExpected;

            Array.Copy(apduCommand.data, 0, sendBuff, 0, apduCommand.data.Length);

            sendLen = sendBuff.Length;

            apduCommand.statusWord = new byte[2];
            recvbuff = new byte[recvLen];

            sendCommandTriggerEvent(new TransmitApduEventArg(sendBuff));
            returnCode = PcscProvider.SCardControl(hCard_,
                                                operationControlCode,
                                                sendBuff,
                                                sendLen,
                                                recvbuff,
                                                recvbuff.Length,
                                                ref actualLength);

            if (returnCode == 0)
            {
                apduCommand.actualLengthReceived = actualLength;

                receivedCommandTriggerEvent(new TransmitApduEventArg(recvbuff.Take(actualLength).ToArray()));

                apduCommand.response = new byte[actualLength];
                Array.Copy(recvbuff, 0, apduCommand.response, 0, actualLength);

                if (actualLength > 1)
                    Array.Copy(recvbuff, actualLength - 2, apduCommand.statusWord, 0, 2);

                //if (apdu.actualLengthReceived >= 2)
                //{
                //    apdu.receiveData = new byte[actualLength - 2];
                //    Array.Copy(recvbuff, 0, apdu.receiveData, 0, actualLength - 2);
                //}
            }
            else
            {
                throw new PcscException(returnCode);
            }

        }

        internal virtual byte[] getFirmwareVersion()
        {
            throw new NotImplementedException();
        }

        void sendCommandTriggerEvent(TransmitApduEventArg e)
        {
            if (OnSendCommand == null)
                return;

            OnSendCommand(this, e);
        }

        void receivedCommandTriggerEvent(TransmitApduEventArg e)
        {
            if (OnReceivedCommand == null)
                return;

            OnReceivedCommand(this, e);
        }
    }
}
