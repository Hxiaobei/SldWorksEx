using System;
using System.Runtime.InteropServices;
using CodeStack.SwEx.AddIn;
using CodeStack.SwEx.AddIn.Attributes;
using CodeStack.SwEx.AddIn.Base;
using CodeStack.SwEx.AddIn.Core;
using CodeStack.SwMsgTs.Commands;
using CodeStack.SwMsgTs.Documents;
using CodeStack.SwMsgTs.View;
using Msg.SwMsgTs.Commands;

namespace Msg.SwMsgTs
{
    [Guid("86EA567D-79FA-4E3B-B66E-EAB660DB3D40"), ComVisible(true)]
    [AutoRegister("SwMsgTs", "SwMsgTs for SOLIDWORKS", true)]
    public class SwAddIn : SwAddInEx {
        private CommandHandlers _cmdHandlers;
        private TaskPaneHandlers _taskPaneHandlers;

        static SwAddIn() {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
                if(args.Name.StartsWith("System.Resources.Extensions")) {
                    var location = System.IO.Path.GetDirectoryName(typeof(SwAddIn).Assembly.Location);
                    var dllPath = System.IO.Path.Combine(location, "System.Resources.Extensions.dll");
                    if(System.IO.File.Exists(dllPath)) {
                        return System.Reflection.Assembly.LoadFrom(dllPath);
                    }
                }
                return null;
            };
        }

        public override bool OnConnect() {
            try {
                var listener = new System.Diagnostics.TextWriterTraceListener(@"g:\code\SldWorksEx\sw_addin_log.txt");
                System.Diagnostics.Trace.Listeners.Add(listener);
                System.Diagnostics.Trace.AutoFlush = true;
                System.Diagnostics.Trace.WriteLine("--- OnConnect Started ---");
            } catch { }

            _cmdHandlers = new CommandHandlers(App);
            _taskPaneHandlers = new TaskPaneHandlers(App);

            AddCommandGroup<Commands_e>(_cmdHandlers.OnCommandClick, _cmdHandlers.OnCommandEnable);
            AddCommandGroup<SubCommands_e>(_cmdHandlers.OnSubCommandClick);



            CreateTaskPane<TaskPaneControl, TaskPaneCommands_e>(_taskPaneHandlers.OnTaskPaneCommandClick, out _);

            return true;
        }


    }
}
