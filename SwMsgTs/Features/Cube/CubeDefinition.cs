using System;
using System.Runtime.InteropServices;
using CodeStack.SwEx.Common.Attributes;
using CodeStack.SwEx.MacroFeature;
using CodeStack.SwEx.MacroFeature.Attributes;
using CodeStack.SwEx.MacroFeature.Base;
using CodeStack.SwEx.MacroFeature.Data;
using CodeStack.SwEx.MathEx;
using CodeStack.SwEx.SwExtensions;
using CodeStack.SwMsgTs.Properties;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace Msg.SwMsgTs.Features.Cube {
    [ComVisible(true)]
    [FeatureIcon(typeof(Resources), nameof(Resources.command_group_icon))]
    [Title("Cube")]
    public class CubeDefinition : MacroFeatureEx<CubeData> {

        /// <summary>
        /// 从平面实体获取中心点和法线
        /// </summary>
        private bool TryGetFaceCenterAndNormal(IFace2 face, out Vector3 center, out Vector3 normal) {
            center = Vector3.Zero;
            normal = Vector3.Zero;

            var surf = face.IGetSurface();
            if(surf == null || !surf.IsPlane()) return false;

            var uvBounds = (double[])face.GetUVBounds();
            double uMid = (uvBounds[0] + uvBounds[1]) / 2.0;
            double vMid = (uvBounds[2] + uvBounds[3]) / 2.0;

            // ISurface.Evaluate 返回 double[]:
            // [x,y,z, dU_x,dU_y,dU_z, dV_x,dV_y,dV_z, normal_x,normal_y,normal_z]
            var evalResult = (double[])surf.Evaluate(uMid, vMid, 0, 0);

            center = new Vector3(evalResult[0], evalResult[1], evalResult[2]);
            normal = Vector3.Normalize(new Vector3(evalResult[9], evalResult[10], evalResult[11]));
            return true;
        }

        protected override RebuildResult OnRebuild(ISldWorks app, IModelDoc2 model, IFeature feature, CubeData parameters) {
            var face = parameters.SelectedFace;
            if(face == null) return RebuildResult.FromStatus(false, "No face selected");

            var side = parameters.SideLength;
            if(side <= 0) return RebuildResult.FromStatus(false, "Side length must be greater than 0");

            if(!TryGetFaceCenterAndNormal(face, out var center, out var normal))
                return RebuildResult.FromStatus(false, "Selected face must be planar");

            try {
                // 使用 ModelerEx.CreateBox 扩展方法创建正方体
                var modeler = SwUtils.Modeler;
                var box = modeler.CreateBox(center, normal, side, side, side);

                if(box == null)
                    return RebuildResult.FromStatus(false, "Failed to create cube body");

                var featData = feature.GetDefinition() as IMacroFeatureData;
                return RebuildResult.FromBody(box, featData, true);
            } catch(Exception ex) {
                System.Diagnostics.Trace.WriteLine($"Exception in CubeDefinition.OnRebuild: {ex}");
                return RebuildResult.FromStatus(false, $"Exception: {ex.Message}");
            }
        }

        protected override void OnSetDimensions(ISldWorks app, IModelDoc2 model, IFeature feature,
            DimensionDataCollection dims, CubeData parameters) {

            if(dims.Count == 0) return;

            var face = parameters.SelectedFace;
            if(face == null) return;

            if(!TryGetFaceCenterAndNormal(face, out var center, out var normal))
                return;

            // 尺寸沿法线方向显示
            dims[nameof(CubeData.SideLength)].SetOrientation(center, normal);
        }
    }
}
