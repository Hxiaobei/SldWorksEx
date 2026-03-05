//**********************
//SwEx.MacroFeature - framework for developing macro features in SOLIDWORKS
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-macrofeature/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/macro-feature
//**********************

using CodeStack.SwEx.MathEx;
using CodeStack.SwEx.SwExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolidWorks.Interop.sldworks {
    public static class ModelerEx {
        static readonly IModeler swModeler = SwUtils.Modeler;
        public static IBody2 CreateBox(this IModeler modeler, Vector3 center, Vector3 dir, ref Vector3 refDir,
            double width, double length, double height) {

            var surf = CreatePlanarSurface(modeler, center, dir, out _);

            var xVec = refDir;
            var yVec = Vector3.Cross(xVec, dir);

            Vector3 GetPoint(double x, double y) => center.Move(xVec, x).Move(yVec, y);

            var corners = new[]
            {
                GetPoint(-width / 2, -length / 2),
                GetPoint(-width / 2,  length / 2),
                GetPoint( width / 2,  length / 2),
                GetPoint( width / 2, -length / 2)
            };

            ICurve CreateCurve(Vector3 p1, Vector3 p2) => ((ICurve)modeler.CreateLine(p1.ToArray(), (p1 - p2).ToArray()))
                .CreateTrimmedCurve2(p1.X, p1.Y, p1.Z, p2.X, p2.Y, p2.Z);

            var curves = new[]
            {
                CreateCurve(corners[0], corners[1]),
                CreateCurve(corners[1], corners[2]),
                CreateCurve(corners[2], corners[3]),
                CreateCurve(corners[3], corners[0])
            };

            return ExtrudedBoundary(surf, curves, dir, height);
        }
        public static IBody2 CreateBox(this IModeler modeler, Vector3 center, Vector3 dir,
            double width, double length, double height) {
            Vector3 refDir = Vector3.Zero;

            return CreateBox(modeler, center, dir, ref refDir, width, length, height);
        }
        public static IBody2 CreateCylinder(this IModeler modeler, Vector3 center, Vector3 axis, double radius, double height) {

            var surf = CreatePlanarSurface(modeler, center, axis, out var refDir);

            var refPt = center.Move(refDir, radius);

            var arc = modeler.CreateArc(center.ToArray(), axis.ToArray(), radius, refPt.ToArray(), refPt.ToArray()) as ICurve;

            arc = arc?.CreateTrimmedCurve2(refPt.X, refPt.Y, refPt.Z, refPt.X, refPt.Y, refPt.Z);

            return ExtrudedBoundary(surf, new ICurve[] { arc }, axis, height);
        }

        private static IBody2 ExtrudedBoundary(ISurface surf, IEnumerable<ICurve> boundary, in Vector3 dir, double height)
            => ExtrudedSheet(TrimmedSheet(surf, boundary, false), dir, height);

        private static ISurface CreatePlanarSurface(IModeler modeler, Vector3 center, Vector3 dir,out Vector3 refDir) {
            Vector3 zAxis = new Vector3(0, 0, 1);
            if(Math.Abs(Vector3.Dot(zAxis, dir)) > 0.999) {
                zAxis = new Vector3(1, 0, 0);
            }
            var ts = GetTransformBetweenVectors(zAxis, dir, center);
            refDir = ts.TransVector(Vector3.UnitX);

            return modeler.CreatePlanarSurface2(center.ToArray(), dir.ToArray(), refDir.ToArray()) as ISurface;
        }

        private static Transform GetTransformBetweenVectors(Vector3 first, Vector3 second, in Vector3 point) {
            return Transform.FromLocalToWorld(first, second, point);
        }

        public static IBody2 TrimmedSheet(ISurface planar, IEnumerable<ICurve> curves, bool preserveAnalytic, double tol = 1e-6)
          => (IBody2)planar.CreateTrimmedSheet5(curves.ToArray(), preserveAnalytic, tol);
        public static IBody2 ExtrudedSheet(this IBody2 sheet, in Vector3 dir, double length)
           => swModeler.CreateExtrudedBody((Body2)sheet, dir.ToSw(), length);
    }
}
