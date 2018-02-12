using System.Windows.Forms;
using System;
namespace Player
{
    public partial class TrayControl : UserControl
    {
        public event EventHandler SwitchViewClicked;
        public event EventHandler ClearViewClicked;
        public event EventHandler SettingsClicked;
        public event EventHandler CallGCClicked;
        public event EventHandler ExitClicked;
        public event EventHandler PlayPauseClicked;
        public event EventHandler NextClicked;
        public event EventHandler PreviousClicked;

        public TrayControl()
        {
            InitializeComponent();
            SwitchViewStrip.Click += (sender, e) => SwitchViewClicked?.Invoke(sender, e);
            ClearViewStrip.Click += (sender, e) => ClearViewClicked?.Invoke(sender, e);
            SettingsStrip.Click += (sender, e) => SettingsClicked?.Invoke(sender, e);
            GCStrip.Click += (sender, e) => CallGCClicked?.Invoke(sender, e);
            ExitStrip.Click += (sender, e) => ExitClicked?.Invoke(sender, e);
            PlayPauseStrip.Click += (sender, e) => PlayPauseClicked?.Invoke(sender, e);
            NextStrip.Click += (sender, e) => NextClicked.Invoke(sender, e);
            PreviousStrip.Click += (sender, e) => PreviousClicked.Invoke(sender, e);
        }
         new void Dispose()
        {
            TrayIcon.Visible = false;
            base.Dispose(true);
            GC.SuppressFinalize(this);
            return;
        }
        public void EmulateClick(int ConIndex) => Context.Items[ConIndex].PerformClick();
        public void EmulateCheckChange(int ConIndex) => ((ToolStripMenuItem)Context.Items[ConIndex]).Checked ^= true;
        public ToolStripMenuItem this[int index] { get => (ToolStripMenuItem)Context.Items[index]; }

        private void TrayControl_Load(object sender, System.EventArgs e)
        {

        }
    }
}
