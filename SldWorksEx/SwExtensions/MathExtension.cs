using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeStack.SwEx.MathEx;
using CodeStack.SwEx.SwExtensions;

namespace SolidWorks.Interop.sldworks {
    public static class MathExtension {
        public static Vector3 ToNd(this ISketchPoint sketchPoint) => new Vector3(sketchPoint.X, sketchPoint.Y, sketchPoint.Z);
        public static Vector3 ToNd(this IVertex vertex) => new Vector3((double[])vertex.GetPoint());
        public static Vector3 ToNd(this IMathPoint point) => new Vector3((double[])point.ArrayData);
        public static Vector3 ToNd(this IMathVector vector) => new Vector3((double[])vector.ArrayData);
        public static Transform ToTransform(this IMathTransform matrix) {
            var data = (double[])matrix.ArrayData;
            //var scale = data[12];始终为1
            var m = new Matrix3(
                data[0], data[3], data[6],  // 第1行 X
                data[1], data[4], data[7],  // 第2行 Y
                data[2], data[5], data[8]   // 第3行 Z
                );
            var v = new Vector3(data[9], data[10], data[11]);
            return new Transform(m, v);
        }

        #region ToSwMath
        public static MathVector ToSw(this Vector3 vec) => (MathVector)SwUtils.Math.CreateVector(vec.ToArray());
        public static MathTransform ToSw(this Transform t, double scale = 1.0) {
            var m = t.Rotation; // Matrix3 类型
            var v = t.Trans; // Vector3 类型

            double[] data = new double[16];

            // 注意：IMathTransform 是列优先存储
            data[0] = m.M11 / scale;
            data[1] = m.M21 / scale;
            data[2] = m.M31 / scale;

            data[3] = m.M12 / scale;
            data[4] = m.M22 / scale;
            data[5] = m.M32 / scale;

            data[6] = m.M13 / scale;
            data[7] = m.M23 / scale;
            data[8] = m.M33 / scale;

            data[9] = v.X;
            data[10] = v.Y;
            data[11] = v.Z;

            data[12] = scale;

            return (MathTransform)SwUtils.Math.CreateTransform(data);
        }

        #endregion

        #region For SolidWorks API
        internal static bool GetExtremePoint(this Body2 swBody, Vector3 vector, out Vector3 point)
           => swBody.GetExtremePoint(vector.X, vector.Y, vector.Z, out point.X, out point.Y, out point.Z);

        #endregion
    }
}
