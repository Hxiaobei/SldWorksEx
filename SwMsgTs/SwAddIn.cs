using System;
using System.Runtime.InteropServices;
using CodeStack.SwEx.AddIn;
using CodeStack.SwEx.AddIn.Attributes;
using CodeStack.SwEx.AddIn.Base;
using CodeStack.SwEx.AddIn.Core;
using Msg.SwMsgTs.Commands;
using Msg.SwMsgTs.Documents;
using Msg.SwMsgTs.View;

namespace Msg.SwMsgTs {
    [Guid("86EA567D-79FA-4E3B-B66E-EAB660DB3D40"), ComVisible(true)]
    [AutoRegister("SwMsgTs", "SwMsgTs for SOLIDWORKS", true)]
    public class SwAddIn : SwAddInEx {
        //private IDocumentsHandler<DataStorageDocHandler> _dataDocs;
        //private IDocumentsHandler<DocumentHandler> _docs;

        //private DocumentEventBinder _binder;
        private CommandHandlers _cmdHandlers;
        private TaskPaneHandlers _taskPaneHandlers;

        public override bool OnConnect() {
            _cmdHandlers = new CommandHandlers(App);
            _taskPaneHandlers = new TaskPaneHandlers(App);

            AddCommandGroup<Commands_e>(_cmdHandlers.OnCommandClick, _cmdHandlers.OnCommandEnable);
            AddCommandGroup<SubCommands_e>(_cmdHandlers.OnSubCommandClick);

            //_dataDocs = CreateDocumentsHandler<DataStorageDocHandler>();

            //_docs = CreateDocumentsHandler();
            //_binder = new DocumentEventBinder(App);
            //_docs.HandlerCreated += _binder.Bind;

            CreateTaskPane<TaskPaneControl, TaskPaneCommands_e>(_taskPaneHandlers.OnTaskPaneCommandClick, out _);

            return true;
        }

        //public override bool OnDisconnect() {
        //    _docs.HandlerCreated -= _binder.Bind;
        //    return base.OnDisconnect();
        //}
    }
}
