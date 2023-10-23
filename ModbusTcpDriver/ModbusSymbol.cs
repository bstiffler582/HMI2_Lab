using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcHmiSrv.Core;
using TcHmiSrv.Core.Tools.DynamicSymbols;

namespace ModbusTcpDriver
{
    class ModbusSymbol : SymbolWithDirectValue
    {
        private int address = 400001;
        private ModbusTarget target;

        public ModbusSymbol(ModbusTarget target, int address) :
          base(0, null, TcHmiJSchemaGenerator.DefaultGenerator.Generate(typeof(int)))
        {
            this.target = target;
            this.address = address;
        }

        protected override Value Value => target.ReadRegister(address);

        protected override Value Write(Queue<string> elements, Value value, Context context)
        {
            target.WriteRegister(address, (int)value);
            return value;
        }
    }
}
