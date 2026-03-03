using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CodeStack.SwEx.Common.Attributes;
using CodeStack.SwMsgTs.Properties;

namespace CodeStack.SwMsgTs.View {
    [ComVisible(true)]
    [Icon(typeof(Resources), nameof(Resources.command_group_icon))]
    public partial class TaskPaneControl : UserControl {
        public TaskPaneControl() {
            InitializeComponent();
        }

        private void OnSendMessage(object sender, EventArgs e) {
            MessageBox.Show(txtText.Text);
        }
    }
}
