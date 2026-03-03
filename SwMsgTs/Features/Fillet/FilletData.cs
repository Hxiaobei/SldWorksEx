using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using CodeStack.SwEx.Common.Attributes;
using CodeStack.SwEx.MacroFeature.Attributes;
using CodeStack.SwEx.PMPage;
using CodeStack.SwEx.PMPage.Attributes;
using CodeStack.SwMsgTs.Properties;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace Msg.SwMsgTs.Features.Fillet
{
    [ComVisible(true)]
    [CodeStack.SwEx.Common.Attributes.Icon(typeof(Resources), nameof(Resources.fillet))]
    [Title("Fillet")]
    public class FilletData : PropertyManagerPageHandlerEx {
        [ControlOptions(swAddControlOptions_e.swControlOptions_Visible)]
        public List<Edge> Edges { get; set; }

        [Description("Fillet Radius")]
        [NumberBoxOptions(swNumberboxUnitType_e.swNumberBox_Length, 0, 1000, 0.001, false, 0.02, 0.001)]
        [ParameterDimension(swDimensionType_e.swRadialDimension)]
        public double Radius { get; set; } = 5 * 1e-3;

        [Title("预览")]
        [ControlOptions(align: swPropertyManagerPageControlLeftAlign_e.swControlAlign_Indent)]
        public bool IsPreview { get; set; }

        [ParameterEditBody]
        public List<IBody2> EditBodies { get; set; }
    }
}
