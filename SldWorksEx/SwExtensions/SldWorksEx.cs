
using System;
using System.Diagnostics;

namespace SolidWorks.Interop.sldworks {
    public static partial class SldWorksEx {
        const int SW_2016_REV = 17;
        const int SW_2017_REV = 17;
        internal enum HighResIconsScope_e {
            CommandManager,
            TaskPane
        }

        public static int GetVersion(this ISldWorks app, out int servicePack, out int servicePackRev) {
            var rev = app.RevisionNumber().Split('.');
            var majorRev = int.Parse(rev[0]);
            servicePack = int.Parse(rev[1]);
            servicePackRev = int.Parse(rev[2]);

            return majorRev - 8;
        }

        public static int GetVersion(this ISldWorks app)
            => int.Parse(app.RevisionNumber().Split('.')[0]) - 8;

        public static bool IsVersionNewerOrEqual(this ISldWorks app, int version, int? sp = null, int? spr = null) {
            if(spr.HasValue && !sp.HasValue)
                throw new ArgumentException("servicePack must be specified when servicePackRev is specified");

            var cur = GetVersion(app, out int curSp, out int curSpr);

            // 版本号比较
            if(cur != version)
                return cur > version;

            // SP 比较
            if(sp.HasValue && curSp != sp.Value)
                return curSp > sp.Value;

            // SPR 比较
            if(spr.HasValue)
                return curSpr >= spr.Value;

            return true;
        }

        internal static bool SupportsHighResIcons(this ISldWorks app) 
            => GetVersion(app) >= SW_2017_REV;
        internal static bool SupportsHighResIcons(this ISldWorks app, HighResIconsScope_e scope) {
            var majorRev = GetVersion(app);
            switch(scope) {
                case HighResIconsScope_e.CommandManager:
                    return majorRev >= SW_2016_REV;

                case HighResIconsScope_e.TaskPane:
                    return majorRev >= SW_2017_REV;

                default:
                    Debug.Assert(false, "Not supported scope");
                    return false;
            }
        }
    }
}
