﻿/*
    This file is part of HomeGenie Project source code.

    HomeGenie is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    HomeGenie is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with HomeGenie.  If not, see <http://www.gnu.org/licenses/>.  
*/

/*
 *     Author: Generoso Martello <gene@homegenie.it>
 *     Project Homepage: http://homegenie.it
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

using HomeGenie.Service;
using System.IO;

namespace HomeGenie.Automation
{

    public class CSharpAppFactory
    {

        //private HomeGenieService homegenie = null;

        public CSharpAppFactory(HomeGenieService hg)
        {
            //homegenie = hg;
        }

        public CompilerResults CompileScript(string condition, string statement, string outputDllFile)
        {
            string source = @"using System;
using System.Collections.Generic;

using HomeGenie;
using HomeGenie.Service;
using HomeGenie.Automation; using HomeGenie.Data;

using Newtonsoft.Json.Linq; using Raspberry; using Raspberry.IO.GeneralPurpose; using Raspberry.IO.GeneralPurpose.Behaviors; using Raspberry.IO.Components.Controllers.Pca9685; using Raspberry.IO.Components.Converters.Mcp4822; using Raspberry.IO.Components.Displays.Hd44780; using Raspberry.IO.Components.Expanders.Mcp23017; using Raspberry.IO.Components.Sensors.HcSr04; using Raspberry.IO.InterIntegratedCircuit; using Raspberry.IO.Components.Converters.Mcp3008;
namespace HomeGenie.Automation.Scripting
{
    public class ScriptingInstance : ScriptingHost
    {
        private void RunCode(string PROGRAM_OPTIONS_STRING)
        {
//////////////////////////////////////////////////////////////////
// NOTE: user code start line is 16 *** please add new code after this method, do not alter start line! ***
{statement}
//////////////////////////////////////////////////////////////////
        }

        private bool EvaluateConditionBlock()
        {
//////////////////////////////////////////////////////////////////
// NOTE: user code start line is ??? *** please add new code after this method, do not alter start line! ***
{condition}
//////////////////////////////////////////////////////////////////
        }

        public MethodRunResult Run(string PROGRAM_OPTIONS_STRING)
        {
            Exception ex = null;
            try
            {
                RunCode(PROGRAM_OPTIONS_STRING);
            }
            catch (Exception e)
            {
                ex = e;
            }
            return new MethodRunResult(){ Exception = ex, ReturnValue = null };
        }

        public MethodRunResult EvaluateCondition()
        {
            Exception ex = null;
            bool retval = false;
            //
            try
            {
                    retval = EvaluateConditionBlock();
            }
            catch (Exception e)
            {
                ex = e;
            }
            return new MethodRunResult(){ Exception = ex, ReturnValue = retval };
        }

    }
}
";
            source = source.Replace("{statement}", statement);
            source = source.Replace("{condition}", condition);
            //
            Dictionary<string, string> providerOptions = new Dictionary<string, string> {
                //                    { "CompilerVersion", "v4.0" }
            };
            CSharpCodeProvider provider = new CSharpCodeProvider(providerOptions);
            CompilerParameters compilerParams = new CompilerParameters {
                GenerateInMemory = true,
                GenerateExecutable = false,
                IncludeDebugInformation = false,
                TreatWarningsAsErrors = false,
                OutputAssembly = outputDllFile
            };
            //
            // Mono runtime 2/3 compatibility fix 
            bool relocateSystemAsm = false;
            Type type = Type.GetType("Mono.Runtime");
            if (type != null)
            {
                MethodInfo displayName = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
                if (displayName != null)
                {
                    int major = 0;
                    if (int.TryParse(displayName.Invoke(null, null).ToString().Substring(0, 1), out major) && major > 2)
                    {
                        relocateSystemAsm = true;
                    }
                }
            }
            if (!relocateSystemAsm)
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    var name = assembly.GetName();
                    if (name.Name.ToLower() == "system")
                    {
                        compilerParams.ReferencedAssemblies.Add(assembly.Location);
                    }
                    else if (name.Name.ToLower() == "system.core")
                    {
                        compilerParams.ReferencedAssemblies.Add(assembly.Location);
                    }
                    else if (name.Name.ToLower() == "microsoft.csharp")
                    {
                        compilerParams.ReferencedAssemblies.Add(assembly.Location);
                    }
                }
            }
            else
            {
                compilerParams.ReferencedAssemblies.Add("System.dll");
                compilerParams.ReferencedAssemblies.Add("System.Core.dll");
                compilerParams.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
            }
            //
            compilerParams.ReferencedAssemblies.Add("HomeGenie.exe");
            compilerParams.ReferencedAssemblies.Add("MIG.dll");
            compilerParams.ReferencedAssemblies.Add("Newtonsoft.Json.dll");
            //
            compilerParams.ReferencedAssemblies.Add("SerialPortLib.dll");
            compilerParams.ReferencedAssemblies.Add("TcpClientLib.dll");
            //
            //if (Raspberry.Board.Current.IsRaspberryPi)
            {
                compilerParams.ReferencedAssemblies.Add("Raspberry.IO.dll");
                compilerParams.ReferencedAssemblies.Add("Raspberry.IO.Components.dll");
                compilerParams.ReferencedAssemblies.Add("Raspberry.IO.GeneralPurpose.dll");
                compilerParams.ReferencedAssemblies.Add("Raspberry.IO.InterIntegratedCircuit.dll");
                compilerParams.ReferencedAssemblies.Add("Raspberry.IO.SerialPeripheralInterface.dll");
                compilerParams.ReferencedAssemblies.Add("Raspberry.System.dll");
            }
            //
            compilerParams.ReferencedAssemblies.Add("System.Reactive.Core.dll");
            compilerParams.ReferencedAssemblies.Add("System.Reactive.Interfaces.dll");
            compilerParams.ReferencedAssemblies.Add("System.Reactive.Linq.dll");
            compilerParams.ReferencedAssemblies.Add("System.Reactive.PlatformServices.dll");
            compilerParams.ReferencedAssemblies.Add("Nmqtt.dll");
            //
            // compile and generate script assembly
            return provider.CompileAssemblyFromSource(compilerParams, source);
        }

    }
}
