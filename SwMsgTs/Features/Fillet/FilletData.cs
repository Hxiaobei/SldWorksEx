using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CodeStack.SwEx.Common.Attributes;
using CodeStack.SwEx.MacroFeature;
using CodeStack.SwEx.MacroFeature.Attributes;
using CodeStack.SwEx.MacroFeature.Base;
using CodeStack.SwEx.MacroFeature.Data;
using CodeStack.SwEx.MathEx;
using CodeStack.SwEx.PMPage;
using CodeStack.SwEx.PMPage.Attributes;
using CodeStack.SwEx.SwExtensions;
using CodeStack.SwMsgTs.Properties;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;

namespace CodeStack.SwMsgTs.Features.Fillet {

    [ComVisible(true)]
    [CodeStack.SwEx.Common.Attributes.Icon(typeof(Resources), nameof(Resources.fillet))]
    [Title("Fillet")]
    public class FilletData : PropertyManagerPageHandlerEx {
        List<Edge> _edges;
        [ControlOptions(swAddControlOptions_e.swControlOptions_Visible)]
        public List<Edge> Edges {
            get => _edges;
            set {
                value = _edges;
                var selBodies = _edges.Select(e => e.GetBody());
                List<Body2> bodies = new List<Body2>();
                foreach(var body in selBodies) {
                    if(bodies.Any(b => SwUtils.Sw.IsSame(b, body) == 0))
                        bodies.Add(body);
                }
            }
        }

        [Description("Fillet Radius")]
        //[Icon(typeof(Resources), nameof(Resources.radius))]
        [NumberBoxOptions(swNumberboxUnitType_e.swNumberBox_Length, 0, 1000, 0.001, false, 0.02, 0.001)]
        [ParameterDimension(swDimensionType_e.swRadialDimension)]
        public double Radius { get; set; } = 5 * 1e-3;

        [Title("‘§¿¿")]
        [ControlOptions(align: swPropertyManagerPageControlLeftAlign_e.swControlAlign_Indent)]
        public bool IsPreview { get; set; }

        [ParameterEditBody]
        public List<IBody2> EditBodies { get; set; }
    }
}
