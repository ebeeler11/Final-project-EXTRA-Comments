using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSV_Assignment
{
    public class Robot
    {
        public const byte STOP = (byte)'s';
        public const byte GO = (byte)'g';
        public const byte HardLeft = (byte)'l';
        public const byte liggity = (byte)'y';
        public const byte riggity = (byte)'t';
        public const byte HardRight = (byte)'r';
        public static byte Resume;
        SerialPort _serialPort;
        public bool Online { get; private set; }

        public Robot() { }

        public Robot(String port)
        {
            SetupSerialComms(port);
        }

        public void SetupSerialComms(String port)
        {
            try
            {
                _serialPort = new SerialPort(port);
                _serialPort.BaudRate = 9600;
                _serialPort.DataBits = 8;
                _serialPort.Parity = Parity.None;
                _serialPort.StopBits = StopBits.Two;
                _serialPort.Open();
                Online = true;
            }
            catch
            {
                Online = false;
            }
        }

        public void Move(byte direction)
        {
            try
            {
                if (Online)
                {
                    Resume = direction;
                    byte[] buffer = { direction };
                    _serialPort.Write(buffer, 0, 1);
                }
            }
            catch
            {
                Online = false;
            }
        }

        public void Close()
        {
            _serialPort.Close();
        }

    }
}
