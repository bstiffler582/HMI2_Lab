using System;
using System.Collections.Generic;
using EasyModbus;

namespace ModbusTcpDriver
{
    class ModbusTarget
    {
        public readonly string Name;
        public readonly string Address;
        public readonly int Port;

        private IDictionary<string, int> registers;
        public IDictionary<string, int> Registers => registers;

        private ModbusClient client;

        public ModbusTarget(string name, string address, int port)
        {
            this.Name = name;
            this.Address = address;
            this.Port = port;

            registers = new Dictionary<string, int>();

            client = new ModbusClient(address, port);
            client.Connect();
        }

        public int ReadRegister(int address)
        {
            if (!client.Connected)
                return 0;

            if (address >= 300000 && address < 400000)
            {
                var res = client.ReadInputRegisters((address - 300000), 1);
                return res[0];
            }
            else if (address >= 400000 && address < 500000)
            {
                var res = client.ReadHoldingRegisters((address - 400000), 1);
                return res[0];
            }
            else
                return 0;
        }

        public void WriteRegister(int address, int value)
        {
            if (!client.Connected)
                return;

            if (address >= 400000 && address < 500000)
                client.WriteSingleRegister((address - 400000), value);
            else
                return;
        }

        public void AddRegister(string name, int address) =>
            registers.Add(name, address);
    }
}
