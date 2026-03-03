
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

        static readonly Lazy<ISldWorks> _swApp = new Lazy<ISldWorks>(() =>
            GetSwAppFromProcess(Process.GetCurrentProcess().Id)
            ?? throw new InvalidOperationException("Failed to get the pointer to ISldWorks"));
        static readonly Lazy<IMathUtility> _swMath
            = new Lazy<IMathUtility>(() => Sw.IGetMathUtility());
        static readonly Lazy<IModeler> _swModeler
            = new Lazy<IModeler>(() => Sw.IGetModeler());

        public static ISldWorks Sw => _swApp.Value;
        public static IMathUtility Math => _swMath.Value;
        public static IModeler Modeler => _swModeler.Value;

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

                    try {
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
                    } finally {
                        Marshal.ReleaseComObject(mk);
                    }
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
