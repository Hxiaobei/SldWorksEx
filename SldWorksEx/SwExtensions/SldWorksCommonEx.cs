
using System;

namespace SolidWorks.Interop.sldworks {
    /// <summary>
    /// Collection of common extension methods to use in the SwEx framework
    /// </summary>
    public static partial class SldWorksCommonEx {
        /// <summary>
        /// Returns the major version of SOLIDWORKS application
        /// </summary>
        /// <param name="app">Pointer to application to return version from</param>
        /// <param name="servicePack">Version of Service Pack</param>
        /// <param name="servicePackRev">Revision of Service Pack</param>
        /// <returns>Major version of the application</returns>
        public static int GetVersion(this ISldWorks app, out int servicePack, out int servicePackRev) {
            var rev = app.RevisionNumber().Split('.');
            var majorRev = int.Parse(rev[0]);
            servicePack = int.Parse(rev[1]);
            servicePackRev = int.Parse(rev[2]);

            return majorRev - 8;
        }

        /// <inheritdoc cref="GetVersion(ISldWorks)"/>
        public static int GetVersion(this ISldWorks app)
            => int.Parse(app.RevisionNumber().Split('.')[0]) - 8;

        /// <summary>
        /// Checks if the version of the SOLIDWORKS is newer or equal to the specified parameters
        /// </summary>
        /// <param name="app">Current SOLIDWORKS application</param>
        /// <param name="version">Target minimum supported version of SOLIDWORKS</param>
        /// <param name="sp">Target minimum service pack version or null to ignore</param>
        /// <param name="spr">Target minimum revision of service pack version or null to ignore</param>
        /// <returns>True of version of the SOLIDWORKS is the same or newer</returns>
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
    }
}
