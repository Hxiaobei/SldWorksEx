using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
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

namespace CodeStack.SwMsgTs.Features.Fillet {
    [ComVisible(true)]
    [FeatureIcon(typeof(Resources), nameof(Resources.fillet))]
    [Title("Fillet")]
    public class FilletDefinition : MacroFeatureEx<FilletData> {
        const int PreviewCurvesCount = 10;
        const int EdgeId = 22;

        IBody2[] CreateFillets(ISldWorks app, IEnumerable<IEdge> filletEdges, double radius, bool isPreview, out IReadOnlyList<IFace2> filletFaces) {

            if(filletEdges == null) throw new Exception("Select entities to add fillets to");
            if(radius <= 0) throw new Exception("Specify radius more than zero");

            var bodies = new List<IBody2>();
            var createdFaces = new List<IFace2>();

            Dictionary<Body2, List<IEdge>> edgeDict = new Dictionary<Body2, List<IEdge>>();

            foreach(var edge in filletEdges) {
                foreach(var dict in edgeDict) {
                    var eq = SwUtils.Sw.IsSame(dict.Key, edge.GetBody());
                    if(eq == 0) {
                        dict.Value.Add(edge);
                    } else if(eq == 1) {
                        edgeDict.Add(edge.GetBody(), new List<IEdge>() { edge });
                    }
                }
            }

            foreach(var group in edgeDict) {
                IBody2 body = group.Key;
                var edges = group.Value;

                if(isPreview) CreateBodyForPreview(EdgeId, ref edges, ref body);
                if(body.GetType() != (int)swBodyType_e.swSolidBody) throw new Exception("Fillet can only be added to solid bodies");
                var faces = body.AddConstantFillets(radius, edges.ToArray()).ConvertSw<IFace2>();
                if(!faces.Any()) throw new Exception("Failed to create fillet for specified entities due to geometrical conditions");

                createdFaces.AddRange(faces);
                bodies.Add(body);
            }

            filletFaces = createdFaces;

            return bodies.ToArray();
        }

        void CreateBodyForPreview(int trackID, ref List<IEdge> edges, ref IBody2 body) {
            var inputEdges = edges;

            try {
                for(int i = 0; i < inputEdges.Count; i++) { inputEdges[i].SetId(trackID); }

                body = body.ICopy();
                var copyEdges = body.GetEdges()
                    .ConvertSw<IEdge>()
                    .Where(e => e.GetID() == trackID)
                    .ToList();

                if(copyEdges.Count == inputEdges.Count) edges = copyEdges;
                else throw new Exception("Failed to track entity");

            } catch(Exception ex) {
                Logger.Log(ex);
                throw;
            } finally {
                inputEdges.ForEach(e => e.RemoveId());
            }
        }

        static ICurve[] UVCurves(IFace2 face, int curvesCount, bool vOrU) {
            var surf = face.IGetSurface();
            var uv = (double[])face.GetUVBounds();

            // U/V 范围
            double minU = uv[0], maxU = uv[1];
            double minV = uv[2], maxV = uv[3];

            // 当前方向与另一方向的范围
            double thisMin = vOrU ? minV : minU;
            double thisMax = vOrU ? maxV : maxU;
            double otherMin = vOrU ? minU : minV;
            double otherMax = vOrU ? maxU : maxV;

            double step = (thisMax - thisMin) / (curvesCount - 1);
            var curves = new List<ICurve>();

            for(int i = 1; i < curvesCount - 1; i++) {
                double par = thisMin + i * step;
                var curve = surf.MakeIsoCurve2(vOrU, ref par);

                // 起点
                double u = vOrU ? otherMin : par;
                double v = vOrU ? par : otherMin;
                var sp = (double[])surf.Evaluate(u, v, 0, 0);

                // 终点
                u = vOrU ? otherMax : par;
                v = vOrU ? par : otherMax;
                var ep = (double[])surf.Evaluate(u, v, 0, 0);

                curves.Add(curve.CreateTrimmedCurve2(sp[0], sp[1], sp[2], ep[0], ep[1], ep[2]));
            }

            return curves.ToArray();
        }
    }
}
