//-----------------------------------------------------------------------
// <copyright file="ModbusTcpDriver.cs" company="Beckhoff Automation GmbH & Co. KG">
//     Copyright (c) Beckhoff Automation GmbH & Co. KG. All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using TcHmiSrv.Core;
using TcHmiSrv.Core.General;
using TcHmiSrv.Core.Listeners;
using TcHmiSrv.Core.Tools.DynamicSymbols;
using TcHmiSrv.Core.Tools.Management;

namespace ModbusTcpDriver
{
    // Represents the default type of the TwinCAT HMI server extension.
    public class ModbusTcpDriver : IServerExtension
    {
        private readonly RequestListener requestListener = new RequestListener();
        private ICollection<ModbusTarget> targets;
        private DynamicSymbolsProvider provider;

        /*
        GETTING STARTED

        The recommended way to get started is to look at a few of the sample extensions that are available on GitHub:
        https://github.com/Beckhoff/TF2000_Server_Samples

        The full documentation for the extension API can be found in the Beckhoff Information System:
        https://infosys.beckhoff.com/english.php?content=../content/1033/te2000_tc3_hmi_engineering/10591698827.html

        An offline version of this documentation is available at this path:
        %TWINCAT3DIR%..\Functions\TE2000-HMI-Engineering\Infrastructure\TcHmiServer\docs\TcHmiSrvExtNet.Core.Documentation.chm
        */

        // Called after the TwinCAT HMI server loaded the server extension.
        public ErrorValue Init()
        {
            this.requestListener.OnRequest += this.OnRequest;
            LoadConfig();
            return ErrorValue.HMI_SUCCESS;
        }

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

            var symbols = new Dictionary<string, Symbol>();
            foreach (var t in targets)
                foreach (var r in t.Registers)
                    symbols.Add($"{t.Name}.{r.Key}", new ModbusSymbol(t, r.Value));

            provider = new DynamicSymbolsProvider(symbols);

            //System.Diagnostics.Debugger.Launch();
        }

        // Called when a client requests a symbol from the domain of the TwinCAT HMI server extension.
        private void OnRequest(object sender, TcHmiSrv.Core.Listeners.RequestListenerEventArgs.OnRequestEventArgs e)
        {
            try
            {
                e.Commands.Result = ModbusTcpDriverErrorValue.ModbusTcpDriverSuccess;

                foreach (var command in provider.HandleCommands(e.Commands))
                {
                    try
                    {
                        // Use the mapping to check which command is requested
                        switch (command.Mapping)
                        {
                            // case "YOUR_MAPPING":
                            //     Handle command
                            //     break;

                            default:
                                command.ExtensionResult = ModbusTcpDriverErrorValue.ModbusTcpDriverFail;
                                command.ResultString = "Unknown command '" + command.Mapping + "' not handled.";
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        command.ExtensionResult = ModbusTcpDriverErrorValue.ModbusTcpDriverFail;
                        command.ResultString = "Calling command '" + command.Mapping + "' failed! Additional information: " + ex.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new TcHmiException(ex.ToString(), ErrorValue.HMI_E_EXTENSION);
            }
        }
    }
}
