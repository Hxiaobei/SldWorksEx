using CodeStack.SwEx.AddIn.Core;
using CodeStack.SwEx.AddIn.Enums;
using SolidWorks.Interop.swconst;

namespace CodeStack.SwMsgTs.Documents {
    class SelectionFilter {
        public bool ShowSelectionEvents { get; set; } = false;

        public bool Filter(DocumentHandler h, swSelectType_e selType, SelectionState_e state) {
            if(!ShowSelectionEvents)
                return true;

            if(state != SelectionState_e.UserPreSelect)
                return AskCancel(h, $"selection ({state}) of {selType}");

            return selType != swSelectType_e.swSelFACES;
        }

        private bool AskCancel(DocumentHandler h, string msg)
            => h.App.SendMsgToUser2(
                $"'{h.Model.GetTitle()}' {msg}. Cancel?",
                (int)swMessageBoxIcon_e.swMbQuestion, (int)swMessageBoxBtn_e.swMbYesNo)
                == (int)swMessageBoxResult_e.swMbHitNo;
    }
}