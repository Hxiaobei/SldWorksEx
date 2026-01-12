using SolidWorks.Interop.sldworks;

namespace Msg.SwMsgTs.Commands {
    class TaskPaneHandlers {
        private readonly ISldWorks _app;

        public TaskPaneHandlers(ISldWorks app) {
            _app = app;
        }

        public void OnTaskPaneCommandClick(TaskPaneCommands_e cmd) {
            _app.SendMsgToUser($"TaskPane {cmd} clicked!");
        }
    }
}