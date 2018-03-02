using System;
using System.Linq;

namespace SNTP_server
{
    internal enum Mode
    {
        Reserved = 0,
        SymmetricPassive = 1,
        SymmetricActive = 2,
        Client = 3,
        Server = 4,
        Broadcast = 5,
        ManagmentNtpMessage = 6,
        PrivateUseReserved = 7
    }

    internal struct SntpFrame
    {
        public LeapIndicator LI { get; private set; }
        public int VersionNumber { get; }
        public Mode Mode { get; private set; }
        public byte Stratum { get; set; }
        public byte PollInterval { get; private set; }
        public byte Precision { get; private set; }
        public byte[] RootDelay { get; private set; }
        public byte[] RootDispersion { get; private set; }
        public byte[] ReferenceIdentifier { get; private set; }
        public Timestamp ReferenceTimestamp { get; private set; }
        public Timestamp OriginateTimestamp { get; private set; }
        public Timestamp ReceiveTimestamp { get; private set; }
        public Timestamp TransmitTimestamp { get; private set; }

        internal enum LeapIndicator
        {
            NoWarning = 0,
            LastMinut61Seconds = 1,
            LastMinut59Seconds = 2,
            NoSyncronization = 3
        }

        public SntpFrame(byte[] bytes)
        {
            if (bytes.Length < 48)
                throw new ArgumentException($"SNTP frame should be at least 48 bytes long but was {bytes.Length}");
            
            LI = ParseLi(bytes);
            VersionNumber = ParseVersionNumber(bytes);
            Mode = ParseMode(bytes);
            Stratum = bytes[1];
            PollInterval = bytes[2];
            Precision = bytes[3];
            RootDelay = bytes.Skip(4).Take(4).ToArray();
            RootDispersion = bytes.Skip(8).Take(4).ToArray();
            ReferenceIdentifier = bytes.Skip(12).Take(4).ToArray();
            ReferenceTimestamp = new Timestamp(bytes.Skip(16).Take(8).ToArray());
            OriginateTimestamp = new Timestamp(bytes.Skip(24).Take(8).ToArray());
            ReceiveTimestamp = new Timestamp(bytes.Skip(32).Take(8).ToArray());
            TransmitTimestamp = new Timestamp(bytes.Skip(40).Take(8).ToArray());
            // TODO extra info
        }

        private static string ParseReferenceIdentifier(byte[] bytes)
        {
            return new string(bytes.Skip(12).Take(4).Select(b => (char) b).ToArray());
        }

        private static Mode ParseMode(byte[] bytes)
        {
            return (Mode) ((bytes[0] & 0b0000_0111) >> 5);
        }

        private static int ParseVersionNumber(byte[] bytes)
        {
            return (bytes[0] & 0b0011_1000) >> 3;
        }

        private static LeapIndicator ParseLi(byte[] bytes)
        {
            return (LeapIndicator) (bytes[0] & 0b1100_0000 >> 6);
        }

        public void RearrangeForResponse(TimeSpan delay)
        {
            LI = LeapIndicator.NoWarning;
            Stratum = 2;
            PollInterval = 4;
            ReferenceIdentifier = new byte[]{0x80, 0x8a, 0x8d, 0xac};
            RootDelay = new byte[]{0, 0, 0x0e, 0x66};
            RootDispersion = new byte[]{0, 0, 0x04, 0x24};
            Mode = Mode.Server;
            Precision = 0xe9;
            OriginateTimestamp = TransmitTimestamp;

            var now = new Timestamp(DateTime.Now + delay);
            ReceiveTimestamp = now;
            TransmitTimestamp = now;

            ReferenceTimestamp = new Timestamp(DateTime.Now - new TimeSpan(0,1,0,0));
        }

        public byte[] ToBytes()
        {
            var bytes = new byte[48];
            bytes.Initialize();

            bytes[0] = (byte)((uint)LI << 6 | (uint)VersionNumber << 3 | (uint)Mode);
            bytes[1] = Stratum;
            bytes[2] = PollInterval;
            bytes[3] = Precision;

            RootDelay.CopyTo(bytes, 4);
            RootDispersion.CopyTo(bytes, 8);
            ReferenceIdentifier.Select(c => (byte)c).ToArray().CopyTo(bytes, 12);
            ReferenceTimestamp.ToBytes().CopyTo(bytes, 16);
            OriginateTimestamp.ToBytes().CopyTo(bytes, 24);
            ReceiveTimestamp.ToBytes().CopyTo(bytes, 32);
            TransmitTimestamp.ToBytes().CopyTo(bytes, 40);

            return bytes;
        }
    }
}