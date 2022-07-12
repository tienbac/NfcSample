using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TM.NFC.Core.PcscCore;

namespace TM.NFC.Core.CardCore
{
    internal delegate void CardStatusChangeDelegate(object sender, CardPollingEventArg e);
    internal delegate void CardPollingErrorDelegate(object sender, CardPollingErrorEventArg e);

    internal enum CARD_STATUS
    {
        UNKNOWN = 0,
        CARD_FOUND = 1,
        CARD_NOT_FOUND = 2,
        ERROR = 3
    }

    internal class CardPollingEventArg : EventArgs
    {
        private string _reader;
        private CARD_STATUS _status;
        private byte[] _atr;
        internal int _currentStatus;

        public string reader
        {
            get => this._reader;
            set => this._reader = value;
        }

        public CARD_STATUS status
        {
            get => this._status;
            set => this._status = value;
        }

        public int currentStatus
        {
            get => this._currentStatus;
            set => this._currentStatus = value;
        }

        public byte[] atr
        {
            get => this._atr;
            set => this._atr = value;
        }
    }

    internal class CardPollingErrorEventArg : EventArgs
    {
        private string _reader;
        private string _errorMessage;
        private int _errorCode;

        public string reader
        {
            get => this._reader;
            set => this._reader = value;
        }

        public string errorMessage
        {
            get => this._errorMessage;
            set => this._errorMessage = value;
        }

        public int errorCode
        {
            get => this._errorCode;
            set => this._errorCode = value;
        }
    }

    internal class CardPolling
    {
        bool _doCardPolling = false;
        object _threadpollStatusLock = new object();
        List<string> _readers = new List<string>();
        Dictionary<string, BackgroundWorker> _threadPoll = null;
        Dictionary<string, CARD_STATUS> _threadPollCardStatus = null;

        public event CardStatusChangeDelegate OnCardFound = delegate { };
        public event CardStatusChangeDelegate OnCardRemoved = delegate { };
        public event CardPollingErrorDelegate OnError = delegate { };

        public bool isBusy()
        {
            if (_threadPoll == null)
                return false;

            if (_threadPoll.Count < 1)
                return false;

            foreach (string key in _threadPoll.Keys)
                if (_threadPoll[key].IsBusy) return true;

            return false;
        }

        public void add(string readerName)
        {
            if (_doCardPolling)
            {
                throw new Exception("Card polling already started");
            }

            if (readerName.Trim() == "")
                return;

            if (!_readers.Contains(readerName))
                _readers.Add(readerName);
        }

        public void start(String reader)
        {
            if (_doCardPolling)
            {
                throw new Exception("Card polling already started");
            }

            if (_readers.Count < 1)
            {
                throw new Exception("No reader found");
            }

            _doCardPolling = true;

            _threadPoll = new Dictionary<string, BackgroundWorker>();
            _threadPollCardStatus = new Dictionary<string, CARD_STATUS>();

            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.RunWorkerAsync(reader);
            _threadPoll.Add(reader, bw);
            _threadPollCardStatus.Add(reader, CARD_STATUS.UNKNOWN);
        }

        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            CardPollingEventArg cardPollingEventArg;
            CardPollingErrorEventArg cardPollingErrorEventArg;

            if (e.ProgressPercentage >= 0)
            {
                cardPollingEventArg = (CardPollingEventArg)e.UserState;

                _threadPollCardStatus[cardPollingEventArg.reader] = cardPollingEventArg.status;
                switch (cardPollingEventArg.status)
                {
                    case CARD_STATUS.CARD_FOUND: cardFound(cardPollingEventArg); break;
                    case CARD_STATUS.CARD_NOT_FOUND: cardRemove(cardPollingEventArg); break;
                }
            }
            else if (e.ProgressPercentage == -1)
            {
                cardPollingErrorEventArg = (CardPollingErrorEventArg)e.UserState;

                _threadPollCardStatus[cardPollingErrorEventArg.reader] = CARD_STATUS.ERROR;
                cardError(cardPollingErrorEventArg);
            }
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                int returnCode;
                int timeout = 0;
                IntPtr context = new IntPtr();
                CardPollingEventArg cardPollingEventArg = new CardPollingEventArg();
                CardPollingErrorEventArg cardPollingErrorEventArg;
                cardPollingEventArg.reader = e.Argument.ToString();
                cardPollingEventArg.status = CARD_STATUS.UNKNOWN;
                PcscProvider.SCARD_READERSTATE state;

                returnCode = PcscProvider.SCardEstablishContext(PcscProvider.SCARD_SCOPE_USER, 0, 0, ref context);
                if (returnCode != PcscProvider.SCARD_S_SUCCESS)
                {
                    throw new Exception("Unable to establish context",
                        new Exception(PcscProvider.getScardErrMsg(returnCode)));
                }

                BackgroundWorker bwOwner = (BackgroundWorker)sender;

                while (!bwOwner.CancellationPending)
                {
                    state = new PcscProvider.SCARD_READERSTATE();
                    state.szReader = e.Argument.ToString();

                    returnCode = PcscProvider.SCardGetStatusChange(context, timeout, ref state, 1);
                    if (returnCode != 0)
                    {
                        if (cardPollingEventArg.status != CARD_STATUS.ERROR)
                        {
                            cardPollingEventArg.status = CARD_STATUS.ERROR;
                            cardPollingErrorEventArg = new CardPollingErrorEventArg();
                            cardPollingErrorEventArg.errorCode = returnCode;
                            cardPollingErrorEventArg.errorMessage = PcscProvider.getScardErrMsg(returnCode);
                            cardPollingErrorEventArg.reader = e.Argument.ToString();
                            bwOwner.ReportProgress(-1, cardPollingErrorEventArg);
                        }
                    }
                    else
                    {
                        if ((state.dwEventState & PcscProvider.SCARD_STATE_PRESENT) == PcscProvider.SCARD_STATE_PRESENT)
                        {
                            if (cardPollingEventArg.status != CARD_STATUS.CARD_FOUND)
                            {
                                if (state.cbAtr == 0)
                                {
                                    cardPollingEventArg.status = CARD_STATUS.CARD_NOT_FOUND;
                                    bwOwner.ReportProgress(0, cardPollingEventArg);
                                }
                                else
                                {
                                    cardPollingEventArg.status = CARD_STATUS.CARD_FOUND;

                                    cardPollingEventArg.atr = state.rgbAtr.Take(state.cbAtr).ToArray();

                                    bwOwner.ReportProgress(0, cardPollingEventArg);
                                }
                            }
                        }
                        else
                        {
                            if (cardPollingEventArg.status != CARD_STATUS.CARD_NOT_FOUND)
                            {
                                cardPollingEventArg.status = CARD_STATUS.CARD_NOT_FOUND;
                                bwOwner.ReportProgress(0, cardPollingEventArg);
                            }
                        }
                    }
                }

                PcscProvider.SCardReleaseContext(context);
            }
            catch (Exception)
            {
                //
            }
        }

        public void stop()
        {
            if (_threadPoll == null)
                return;

            foreach (string key in _threadPoll.Keys)
                _threadPoll[key].CancelAsync();

            _doCardPolling = false;
        }

        public CARD_STATUS getCardStatus(string readername)
        {
            try
            {
                if (!_threadPollCardStatus.ContainsKey(readername))
                    throw new Exception("Reader not found");

                return _threadPollCardStatus[readername];
            }
            catch (Exception)
            {
                return CARD_STATUS.UNKNOWN;
            }
        }

        void cardFound(CardPollingEventArg e)
        {
            if (OnCardFound == null)
                return;


            OnCardFound(this, e);
        }

        void cardRemove(CardPollingEventArg e)
        {
            if (OnCardFound == null)
                return;

            OnCardRemoved(this, e);
        }

        void cardError(CardPollingErrorEventArg e)
        {
            if (OnError == null)
                return;

            OnError(this, e);
        }
    }
}
