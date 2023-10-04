## Lab 1: Server Extension

### Contents
1. [Getting Started](#getting-started)
2. [Configuration Setup](#configuration-setup)
3. [Read Configuration](#read-config)
4. [Debugging](#debugging)
5. [Symbols](#symbols)
6. [Modbus TCP](#modbus)

---

### Introduction

We will be making a Modbus TCP driver for TwinCAT HMI. 

This is an industry relevant application which extends the product in a meaningful way. It also highlights the architecture of the server extension SDK and namespaces.

When we are done, we will be able to define registers in the extension configuration and interface with them just as we do with ADS symbols. Finally, we are outsourcing the hard part and utilizing a third-party library for communications.


<a id="getting-started"></a>

### 1. Getting Started

- Clone this repo
- Activate and run the PLC program
- Create a new Server Extension Project
  - Project type = *TwinCAT HMI Empty Server Extension (CSharp)*
  - Let's call our extension '**ModbusTcpDriver**'

<a id="configuration-setup"></a>

### 2. Configure the Extension

Append the following to the *config\ModbusTcpDriver.Config.json* file. This tells the framework that this extension will provide symbol information (similar to the ADS extension):

```json
"symbols": {
  "ListSymbols": {
    "readValue": {
      "function": true
    }
  },
  "GetDefinitions": {
    "readValue": {
      "function": true
    }
  },
    "GetSchema": {
      "readValue": {
        "function": true
    },
    "writeValue": {
      "type": "string"
    }
  }
}
```

Next, add the following to the *config\ModbusTcpDriver.Schema.json* file. This defines the format of the extension's configuration data. It also tells TcHMI how to render the extension config page. You can replace the full file with this content:

```json
{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "definitions": {
    "target": {
      "type": "object",
      "properties": {
        "targetAddress": {
          "type": "string",
          "description": "IP or hostname of Modbus TCP server"
        },
        "targetPort": {
          "type": "integer",
          "description": "Port number",
          "default": 502
        },
        "registers": {
          "type": "object",
          "additionalProperties": {
            "type": "integer",
            "default": 400001
          }
        }
      },
      "required": ["targetAddress", "targetPort"]
    }
  },
  "properties": {
    "targets": {
      "configDescription": "Modbus TCP master targets",
      "type": "object",
      "additionalProperties": {
        "$ref": "#/definitions/target"
      }
    }
  },
  "additionalProperties": false
}
```

Now we are ready to build the extension project. Once built, we can add the extension as a reference to our HMI project. Make sure our little check mark turns <span style="color: green">green</span>!

We should now be able to load and modify the extension configuration page. We will add a target that looks something like this:
- Address = 127.0.0.1
- Port = 502
- Registers:
    - input_register_1 : 332768
    - input_register_2 : 332769
    - holding_register_1: 432768
    - holding_register_2: 432772

<a id="read-config"></a>

### 3. Read Configuration

Our extension is running. Let's disable it (via the right-click menu) before doing anything else.

Create a new class file called *ModbusTarget.cs* to store all that configuration data:
```cs
class ModbusTarget
{
    public readonly string Name;
    public readonly string Address;
    public readonly int Port;

    private IDictionary<string, int> registers;
    public IDictionary<string, int> Registers => registers;

    public ModbusTarget(string name, string address, int port) 
    {
        this.Name = name;
        this.Address = address;
        this.Port = port;

        registers = new Dictionary<string, int>();
    }

    public void AddRegister(string name, int address) => 
        registers.Add(name, address);
}
```

Now we can read in our runtime config, and populate an instance of our new class. 

First, define a collection of targets (remember we can configure as many as we want) as a field in the `ModbusTcpDriver` class:
```cs
private ICollection<ModbusTarget> targets;
```

Then populate it with the following method:

```cs
private void LoadConfig()
{
    // get config value
    var config = TcHmiApplication.AsyncHost.GetConfigValue(TcHmiApplication.Context, "targets");

    // parse target config, create targets
    targets = new List<ModbusTarget>();
    foreach (var t in config as IDictionary<string, Value>)
    {
        var obj = t.Value as IDictionary<string, Value>;
        var target = new ModbusTarget(t.Key, obj["targetAddress"], obj["targetPort"]);

        foreach (var reg in obj["registers"] as IDictionary<string, Value>)
            target.AddRegister(reg.Key, reg.Value);

        targets.Add(target);
    }
}
```
Finally, to make sure this method executes on startup, we will call it from the `Init()` method!

If our build is happy, we are happy 😊.

<a id="debugging"></a>

### 4. Debugging

Debugging is pretty easy:
- Set the build configuration to Debug in Visual Studio.
- Rebuild the project.
- Enable the extension.
- Set a breakpoint.
- Via the Debug menu, select 'Attach to Process'.
- Find your extension process.

However, our code execution has come and gone - we will never hit our breakpoint. 

If we need to debug our configuration logic, we will need to do something like this:
```cs
System.Diagnostics.Debugger.Launch();
```
This will cause our extension to timeout on initialization, but it's OK. We can just remove the line when we're finished.

<a id="symbols"></a>

### 5. Symbols

We can take advantage of the SDK to use a lot of the same functionality that the ADS extension uses for handling symbols. We are doing simple integer reads and writes, so this will be really straight-forward.

Create a new class file called *ModbusSymbol.cs*:
```cs
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

    protected override Value Value => 0;

    protected override Value Write(Queue<string> elements, Value value, Context context) 
    {
        return value;
    }
}
```

Notice how our symbol inherits from the `SymbolWithDirectValue` class, and we are overriding the `Value` property getter, as well as the `Write()` method.

Now we need to append a few more things to the `ModbusTcpDriver` class so we can load the extension with our new symbols. First, add the field:
```cs
private DynamicSymbolsProvider provider;
```
Then, the following logic to the bottom of our `LoadConfig()` method:
```cs
var symbols = new Dictionary<string, Symbol>();
foreach (var t in targets)
    foreach (var r in t.Registers)
        symbols.Add($"{t.Name}.{r.Key}", new ModbusSymbol(t, r.Value));

provider = new DynamicSymbolsProvider(symbols);
```
Finally, change the following line in the `OnRequest()` method from:
```cs
foreach (Command command in e.Commands)
```
to
```cs
foreach (var command in provider.HandleCommands(e.Commands))
```

Again, since our symbol class inherits from `SymbolWithDirectValue`, the framework (using the `DynamicSymbolProvider` we've just implemented) already knows how and when to process the requests for these symbols from our clients.

<a id="modbus"></a>

### 6. Modbus TCP

We are finally ready to actually implement the Modbus protocol. Luckily, someone else has already done all the heavy lifting! Let's just bring in a package and integrate it.

We will be using the [EasyModbusTCP](https://github.com/rossmann-engineering/EasyModbusTCP.NET) library, and fortunately it is available right on Nuget.

Once you've added the package, we can implement the Modbus TCP client in our `ModbusTcpTarget` class as follows.

Add the namespace:
```cs
using EasyModbus;
```
Add the client field:
```cs
private ModbusClient client;
```
Instantiate and connect in the constructor:
```cs
client = new ModbusClient(address, port);
client.Connect();
```
Add read/write methods:
```cs
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
```

Note: The EasyModbus library expects addresses without the leading register type prefix - hence the subtract operation.

Finally, we just need to call these read/write methods when our client requests one of our `ModbusSymbol`s!

Read:
```cs
protected override Value Value => target.ReadRegister(address);
```
Write:
```cs
protected override Value Write(Queue<string> elements, Value value, Context context)
{
    target.WriteRegister(address, (int)value);
    return value;
}
```

Let's build, enable and test our extension! We will need to map and bind the symbols to the NumericInput controls in Desktop.view. 

>Note: Remember, the input registers (3x) are read only, and the holding registers (4x) are read/write. The PLC program is writing dummy data to the first 3 of each type of register (32768 - 32770).

- How can this extension be improved for use in production scenarios? 
- What simple steps can we take to facilitate reading and writing coils?