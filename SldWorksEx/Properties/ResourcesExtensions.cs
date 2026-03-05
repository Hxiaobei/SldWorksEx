using System.Drawing;
using System.IO;

namespace CodeStack.SwEx.Properties {
    public partial class Resources {
        private static Bitmap LoadBitmap(string name) {
            using (var stream = typeof(Resources).Assembly.GetManifestResourceStream("CodeStack.SwEx.Resources." + name)) {
                if(stream != null) return new Bitmap(stream);
                return null;
            }
        }
        
        public static Bitmap default_icon => LoadBitmap("default_icon.png");
    }
}
