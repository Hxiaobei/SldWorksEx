using System;
using System.Collections.Generic;
using CodeStack.SwEx.AddIn.Enums;
using CodeStack.SwMsgTs.Commands;
using Msg.SwMsgTs.Features.Cube;
using Msg.SwMsgTs.Features.Fillet;
using SolidWorks.Interop.sldworks;

namespace Msg.SwMsgTs.Commands
{
    class CommandHandlers
    {
        private readonly ISldWorks _app;

        public CommandHandlers(ISldWorks app) { _app = app; }

        public void OnCommandClick(Commands_e cmd)
        {
            switch (cmd)
            {
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
                case Commands_e.CubeMacroFeature:
                    CreateCubeMacroFeature();
                    return;
                default:
                    break;
            }
            _app.SendMsgToUser($"{cmd} clicked!");
        }

        public void OnSubCommandClick(SubCommands_e cmd)
        {
            switch (cmd)
            {
                case SubCommands_e.SubCommand1:
                    break;
                case SubCommands_e.SubCommand2:
                    break;
                default:
                    break;
            }
            _app.SendMsgToUser($"{cmd} clicked!");
        }

        public void OnCommandEnable(Commands_e cmd, ref CommandItemEnableState_e state)
        {
            if (cmd == Commands_e.Command1 &&
                state == CommandItemEnableState_e.DeselectEnable &&
                _app.IActiveDoc2?.ISelectionManager?.GetSelectedObjectCount2(-1) == 0)
            {
                state = CommandItemEnableState_e.DeselectDisable;
            }
        }


        public void CreateBoundingCylinderMacroFeature()
        {
            if (_app.IActiveDoc2.ISelectionManager.GetSelectedObject6(1, -1) is IBody2 body)
            {
                _app.IActiveDoc2.FeatureManager.InsertComFeature<FilletDefinition, FilletData>(
                    new FilletData() { EditBodies = new List<IBody2> { body } });
            }
            else
            {
                _app.SendMsgToUser("Please select solid body");
            }
        }

        public void CreateCubeMacroFeature()
        {
            var selMgr = _app.IActiveDoc2?.ISelectionManager;
            if(selMgr == null) {
                _app.SendMsgToUser("请先打开零件文档");
                return;
            }

            var selectedObj = selMgr.GetSelectedObject6(1, -1);
            if(selectedObj is IFace2 face) {
                var surf = face.IGetSurface();
                if(surf == null || !surf.IsPlane()) {
                    _app.SendMsgToUser("请选择一个平面");
                    return;
                }

                _app.IActiveDoc2.FeatureManager.InsertComFeature<CubeDefinition, CubeData>(
                    new CubeData() { SelectedFace = face });
            }
            else {
                _app.SendMsgToUser("请先选择一个平面");
            }
        }
    }
}