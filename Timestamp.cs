using System;

namespace SNTP_server
{
    internal struct Timestamp
    {
        public readonly uint seconds, fraction;

        public Timestamp(DateTime dateTime)
        {
            var time = dateTime.ToUniversalTime() - new DateTime(1900, 1, 1);
            seconds = (uint) time.TotalSeconds;
            fraction = (uint) ((time.TotalSeconds - seconds) * (1UL << 32));
        }

        public Timestamp(uint seconds, uint fraction)
        {
            this.seconds = seconds;
            this.fraction = fraction;
        }

        public Timestamp(byte[] bytes)
        {
            if (bytes.Length < 8)
                throw new ArgumentException($"Timestamp constructor requires at least 8 bytes but {bytes.Length} given.");
            seconds = (uint) bytes[3] | (uint) bytes[2] << 8 | (uint) bytes[1] << 16 | (uint) bytes[0] << 24;
            fraction = (uint) bytes[7] | (uint) bytes[6] << 8 | (uint) bytes[5] << 16 | (uint) bytes[4] << 24;
        }

        public byte[] ToBytes()
        {
            var bytes = new byte[8];
            bytes[0] = (byte)((seconds & 0b11111111000000000000000000000000U) >> 24);
            bytes[1] = (byte)((seconds & 0b00000000111111110000000000000000U) >> 16);
            bytes[2] = (byte)((seconds & 0b00000000000000001111111100000000U) >> 8);
            bytes[3] = (byte)(seconds & 0b00000000000000000000000011111111U);
            bytes[4] = (byte)((fraction & 0b11111111000000000000000000000000U) >> 24);
            bytes[5] = (byte)((fraction & 0b00000000111111110000000000000000U) >> 16);
            bytes[6] = (byte)((fraction & 0b00000000000000001111111100000000U) >> 8);
            bytes[7] = (byte)(fraction & 0b00000000000000000000000011111111U);

//            bytes.Initialize();
//            for (var i = 0; i < 4; i++)
//                bytes[i] = (byte)((seconds & 255 << ((4 - i) * 8)) >> ((4 - i) * 8));
//            for (var i = 0; i < 4; i++)
//                bytes[i + 4] = (byte)((fraction & 255 << ((4 - i) * 8)) >> ((4 - i) * 8));
            return bytes;
        }
    }
}