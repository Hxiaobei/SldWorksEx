using CodeStack.SwEx.AddIn.Core;
using CodeStack.SwEx.AddIn.Delegates;
using CodeStack.SwEx.AddIn.Enums;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace CodeStack.SwMsgTs.Documents {
    class DocumentEventHandlers {
        private readonly ISldWorks _app;
        private readonly SelectionFilter _filter = new SelectionFilter();

        public DocumentEventHandlers(ISldWorks app) {
            _app = app;
        }

        public void OnActivated(DocumentHandler h)
            => _app.SendMsgToUser2($"'{h.Model.GetTitle()}' activated",
                (int)swMessageBoxIcon_e.swMbInformation, (int)swMessageBoxBtn_e.swMbOk);

        public void OnInitialized(DocumentHandler h)
            => _app.SendMsgToUser2($"'{h.Model.GetTitle()}' initialized",
                (int)swMessageBoxIcon_e.swMbInformation, (int)swMessageBoxBtn_e.swMbOk);

        public void OnDestroyed(DocumentHandler h)
            => _app.SendMsgToUser2($"'{h.Model.GetTitle()}' destroyed",
                (int)swMessageBoxIcon_e.swMbInformation, (int)swMessageBoxBtn_e.swMbOk);

        public bool OnSelection(DocumentHandler h, swSelectType_e selType, SelectionState_e state)
            => _filter.Filter(h, selType, state);

        public bool OnSave(DocumentHandler h, string file, SaveState_e type)
            => AskCancel(h, $"saving ({type})");

        public bool OnRebuild(DocumentHandler h, RebuildState_e type)
            => AskCancel(h, $"rebuilt ({type})");

        public void OnDimensionChange(DocumentHandler h, IDisplayDimension dim)
            => _app.SendMsgToUser2(
                $"'{h.Model.GetTitle()}' dimension change: {dim.IGetDimension().FullName} = {dim.IGetDimension().Value}",
                (int)swMessageBoxIcon_e.swMbInformation, (int)swMessageBoxBtn_e.swMbOk);

        public void OnItemModified(DocumentHandler h, ItemModificationAction_e type, swNotifyEntityType_e ent, string name, string old = "")
            => _app.SendMsgToUser2(
                $"'{h.Model.GetTitle()}' item modified ({type}) of {ent}. Name: {name} (from {old}).",
                (int)swMessageBoxIcon_e.swMbInformation, (int)swMessageBoxBtn_e.swMbOk);

        public void OnCustomPropertyModified(DocumentHandler h, CustomPropertyModifyData[] mods) {
            foreach(var m in mods) {
                _app.SendMsgToUser2(
                    $"'{h.Model.GetTitle()}' custom property '{m.Name}' changed ({m.Action}) in '{m.Configuration}' to '{m.Value}'",
                    (int)swMessageBoxIcon_e.swMbInformation, (int)swMessageBoxBtn_e.swMbOk);
            }
        }

        public void OnConfigurationChanged(DocumentHandler h, ConfigurationChangeState_e type, string conf)
            => _app.SendMsgToUser2(
                $"'{h.Model.GetTitle()}' configuration {conf} changed ({type})",
                (int)swMessageBoxIcon_e.swMbInformation, (int)swMessageBoxBtn_e.swMbOk);

        private bool AskCancel(DocumentHandler h, string msg)
            => _app.SendMsgToUser2(
                $"'{h.Model.GetTitle()}' {msg}. Cancel?",
                (int)swMessageBoxIcon_e.swMbQuestion, (int)swMessageBoxBtn_e.swMbYesNo)
                == (int)swMessageBoxResult_e.swMbHitNo;
    }
}
