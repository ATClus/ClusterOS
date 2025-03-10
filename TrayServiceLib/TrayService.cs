namespace TrayServiceLib
{
    public class TrayService : IDisposable
    {
        private NotifyIcon notifyIcon;

        public event EventHandler RestoreRequested;
        public event EventHandler ExitRequested;

        public TrayService()
        {
            notifyIcon = new NotifyIcon
            {
                Icon = ByteArrayToIcon(Properties.Resource.AppIcon),
                Text = "ClusterOS", 
                Visible = true
            };

            notifyIcon.DoubleClick += (s, e) => RestoreRequested?.Invoke(this, EventArgs.Empty);

            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
            ToolStripMenuItem restoreItem = new ToolStripMenuItem("Restore");
            restoreItem.Click += (s, e) => RestoreRequested?.Invoke(this, EventArgs.Empty);
            ToolStripMenuItem exitItem = new ToolStripMenuItem("Quit");
            exitItem.Click += (s, e) => ExitRequested?.Invoke(this, EventArgs.Empty);

            contextMenuStrip.Items.Add(restoreItem);
            contextMenuStrip.Items.Add(exitItem);

            notifyIcon.ContextMenuStrip = contextMenuStrip;
        }

        public void Dispose()
        {
            notifyIcon.Dispose();
        }

        private Icon ByteArrayToIcon(byte[] iconBytes)
        {
            using (MemoryStream ms = new MemoryStream(iconBytes))
            {
                return new Icon(ms);
            }
        }

        public void ShowBalloonTip(string title, string message, ToolTipIcon icon)
        {
            notifyIcon.BalloonTipTitle = title;
            notifyIcon.BalloonTipText = message;
            notifyIcon.BalloonTipIcon = icon;
            notifyIcon.ShowBalloonTip(3000);
        }
    }
}