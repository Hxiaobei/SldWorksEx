//**********************
//SwEx.MacroFeature - framework for developing macro features in SOLIDWORKS
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-macrofeature/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/macro-feature
//**********************

using CodeStack.SwEx.SwExtensions;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CodeStack.SwEx.MacroFeature.Base {
    /// <summary>
    /// Represents the result of macro feature where macro feature holds the body or pattern of bodies
    /// </summary>
    public class BodyResult : RebuildResult {

        private static object GetBodyResult(IBody2[] bodies) {
            _ = bodies ?? throw new ArgumentNullException(nameof(bodies));
            if(bodies.Length == 1)
                return bodies[0];
            else
                return bodies;
        }

        internal protected BodyResult(IMacroFeatureData featData,
            bool updateEntityIds, params IBody2[] bodies) : base(GetBodyResult(bodies)) {
            // Enable multi-body support for SW 2013 SP5+
            if(SwUtils.Sw.IsVersionNewerOrEqual(13, 5))
                featData.EnableMultiBodyConsume = true;

            if(updateEntityIds) return;
            _ = featData ?? throw new ArgumentNullException(nameof(featData));

            for(int i = 0; i < bodies.Length; i++) {
                featData.GetEntitiesNeedUserId(bodies[i], out var faceArray, out var edgeArray);

                var faces = faceArray.ConvertSw<Face2>();
                for(int j = 0; j < faces.Length; j++) {
                    featData.SetFaceUserId(faces[j], j, 0);
                }

                var edges = edgeArray.ConvertSw<Edge>();
                for(int j = 0; j < edges.Length; j++) {
                    featData.SetEdgeUserId(edges[j], j, 0);
                }
            }
        }
    }
}
