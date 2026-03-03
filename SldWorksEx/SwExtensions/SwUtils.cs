
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace CodeStack.SwEx.SwExtensions {
    public static class SwUtils {
        [DllImport("ole32.dll")]
        static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        static readonly ISldWorks _swApp;
        static readonly IMathUtility _swMath;
        static readonly IModeler _swModeler;

        static SwUtils() {
            _swApp = GetSwAppFromProcess(Process.GetCurrentProcess().Id)
                ?? throw new NullReferenceException("Failed to get the pointer to ISldWorks");
            _swMath = _swApp.IGetMathUtility();
            _swModeler = _swApp.IGetModeler();
        }

        public static ISldWorks Sw => _swApp;
        public static IMathUtility Math => _swMath;
        public static IModeler Modeler => _swModeler;

        private static ISldWorks GetSwAppFromProcess(int processId) {
            var monikerName = $"SolidWorks_PID_{processId}";

            CreateBindCtx(0, out var ctx);
            ctx.GetRunningObjectTable(out var rot);
            rot.EnumRunning(out var enumMoniker);

            try {
                var arr = new IMoniker[1];

                while(enumMoniker.Next(1, arr, IntPtr.Zero) == 0) {
                    var mk = arr[0];
                    if(mk == null) continue;

                    string name;
                    try {
                        mk.GetDisplayName(ctx, null, out name);
                    } catch {
                        continue;
                    }

                    if(!string.Equals(name, monikerName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    rot.GetObject(mk, out var obj);
                    return obj as ISldWorks;
                }
            } finally {
                if(enumMoniker != null) Marshal.ReleaseComObject(enumMoniker);
                if(rot != null) Marshal.ReleaseComObject(rot);
                if(ctx != null) Marshal.ReleaseComObject(ctx);
            }

            return null;
        }
    }
}
