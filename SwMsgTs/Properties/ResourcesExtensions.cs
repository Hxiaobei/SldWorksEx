using System.Drawing;
using System.IO;

namespace CodeStack.SwMsgTs.Properties {
    public partial class Resources {
        private static Bitmap LoadBitmap(string name) {
            using (var stream = typeof(Resources).Assembly.GetManifestResourceStream("Msg.SwMsgTs.Resources." + name)) {
                if(stream != null) return new Bitmap(stream);
                return null;
            }
        }
        
        public static Bitmap command_group_icon => LoadBitmap("command-group-icon.png");
        public static Bitmap command1_icon => LoadBitmap("command1-icon.png");
        public static Bitmap command2_icon => LoadBitmap("command2-icon.png");
        public static Bitmap fillet => LoadBitmap("fillet.png");
    }
}
