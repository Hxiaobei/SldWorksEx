using System;
using CodeStack.SwEx.AddIn.Enums;
using Msg.SwMsgTs.Features;
using SolidWorks.Interop.sldworks;

namespace Msg.SwMsgTs.Commands {
    class CommandHandlers {
        private readonly ISldWorks _app;

        public CommandHandlers(ISldWorks app) { _app = app; }

        public void OnCommandClick(Commands_e cmd) {
            switch(cmd) {
                case Commands_e.Command1:
                    break;
                case Commands_e.Command2:
                    break;
                case Commands_e.Command3:
                    break;
                case Commands_e.Command4:
                    break;
                case Commands_e.ParamsMacroFeature:
                    CreateBoundingCylinderMacroFeature();
                    return;
                    break;
                default:
                    break;
            }
            _app.SendMsgToUser($"{cmd} clicked!");
        }

        public void OnSubCommandClick(SubCommands_e cmd) {
            switch(cmd) {
                case SubCommands_e.SubCommand1:
                    break;
                case SubCommands_e.SubCommand2:
                    break;
                default:
                    break;
            }
            _app.SendMsgToUser($"{cmd} clicked!");
        }

        public void OnCommandEnable(Commands_e cmd, ref CommandItemEnableState_e state) {
            if(cmd == Commands_e.Command1 &&
                state == CommandItemEnableState_e.DeselectEnable &&
                _app.IActiveDoc2?.ISelectionManager?.GetSelectedObjectCount2(-1) == 0) {
                state = CommandItemEnableState_e.DeselectDisable;
            }
        }


        public void CreateBoundingCylinderMacroFeature() {
            var body = _app.IActiveDoc2.ISelectionManager.GetSelectedObject6(1, -1) as IBody2;

            if(body != null) {
                _app.IActiveDoc2.FeatureManager.InsertComFeature<BoundingCylinderMacroFeature, BoundingCylinderMacroFeatureParams>(
                    new BoundingCylinderMacroFeatureParams() {
                        InputBody = body
                    });
            } else {
                _app.SendMsgToUser("Please select solid body");
            }
        }
    }
}