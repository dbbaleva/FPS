using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ZKEMKeeperTypeLib;

namespace Libraries
{
    public class Biometrics : Disposable
    {
        private readonly CZKEMClassWrapper _czkemClass = new CZKEMClassWrapper();
        private readonly string _deviceIP;
        private readonly int _devicePort;

        private IZKEM GetDevice() => (IZKEM) _czkemClass;

        public Biometrics(string deviceIP, int devicePort)
        {
            _deviceIP = deviceIP;
            _devicePort = devicePort;
        }

        public bool Connected { get; set; }

        public bool Connect()
        {
            if (!Connected)
                Connected = GetDevice().Connect_Net(_deviceIP, _devicePort);

            return Connected;
        }

        public void Disconnect()
        {
            if (Connected)
                GetDevice().Disconnect();
            Connected = false;
        }

        public void EnableDevice(bool flag)
        {
            GetDevice().EnableDevice(1, flag);
        }

        public bool CanReadLogs()
        {
            return GetDevice().ReadGeneralLogData(1);
        }

        public int GetLastError()
        {
            int errorCode = 0;
            GetDevice().GetLastError(ref errorCode);
            return errorCode;
        }

        public IEnumerable<TimeLog> ReadLogs()
        {
            if (!Connect())
                yield break;

            EnableDevice(false);

            if (CanReadLogs())
            {
                int enrollNumber = 0;
                int verifyMode = 0;
                int inOutMode = 0;
                string timeStr = null;

                while (GetDevice().GetGeneralLogDataStr(1, ref enrollNumber, ref verifyMode, ref inOutMode,
                    ref timeStr))
                {
                    yield return new TimeLog
                    {
                        EnrollNumber = enrollNumber,
                        Verification = verifyMode,
                        TimeStamp = DateTime.Parse(timeStr),
                        TimeCode = inOutMode
                    };
                }
            }

            EnableDevice(true);
        }

        protected override void Release()
        {
            if (Connected)
            {
                Disconnect();
            }
        }

        [ComImport]
        [Guid("00853A19-BD51-419B-9269-2DABE57EB61F")]
        public class CZKEMClassWrapper
        {
        }

        public class TimeLog
        {
            public int EnrollNumber { get; set; }
            public int TimeCode { get; set; }
            public DateTime TimeStamp { get; set; }
            public int Verification { get; set; }
        }
    }
}