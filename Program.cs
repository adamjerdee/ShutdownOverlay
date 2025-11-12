using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        Application.Run(new ShutdownOverlayForm());
    }
}

public class ShutdownOverlayForm : Form
{
    private const int BOX_WIDTH = 600;  // Wider fixed width
    private const int TOP_MARGIN = 20;

    private readonly Label _title;
    private readonly Label _clock;
    private readonly System.Windows.Forms.Timer _secondTimer;
    private readonly System.Windows.Forms.Timer _moveTimer;
    private readonly NotifyIcon _tray;

    private TimeSpan _remaining = TimeSpan.FromMinutes(30);
    private readonly Random _rng = new Random();

    private readonly Color[] _cycle = new[]
    {
        Color.Lime,
        Color.Yellow,
        Color.Fuchsia,
        Color.FromArgb(128, 200, 255),
        Color.FromArgb(200, 160, 255)
    };
    private int _cycleIndex = 0;

    public ShutdownOverlayForm()
    {
        // ===== Window =====
        Text = "Shutdown Overlay";
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        Opacity = 0.5;
        BackColor = Color.Black;
        ForeColor = _cycle[_cycleIndex];
        Padding = new Padding(28);
        Width = BOX_WIDTH;                  // Fixed width
        AutoSize = false;                   // Disable autosize to prevent cutoffs
        Height = 200;                       // initial height (adjusts later)
        ShowInTaskbar = false;
        TopMost = true;

        // ===== Labels =====
        _title = new Label
        {
            Text = "Machine will shutdown soon",
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 60,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
        };

        _clock = new Label
        {
            Text = Format(_remaining),
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 100,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 52, FontStyle.Bold),
        };

        var panel = new TableLayoutPanel
        {
            ColumnCount = 1,
            RowCount = 2,
            Dock = DockStyle.Fill
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
        panel.Controls.Add(_title, 0, 0);
        panel.Controls.Add(_clock, 0, 1);
        Controls.Add(panel);

        Shown += (s, e) =>
        {
            ApplyCycleColor();
            PlaceTopCenter();
        };

        // ===== Timers =====
        _secondTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _secondTimer.Tick += OnSecondTick;
        _secondTimer.Start();

        _moveTimer = new System.Windows.Forms.Timer { Interval = 10_000 };
        _moveTimer.Tick += (s, e) => MoveRandomWithinPrimary();
        _moveTimer.Start();

        // ===== Tray Icon =====
        var menu = new ContextMenuStrip();
        var cancelItem = new ToolStripMenuItem("Cancel shutdown (Exit)");
        cancelItem.Click += (s, e) => { _secondTimer.Stop(); _moveTimer.Stop(); Application.Exit(); };
        menu.Items.Add(cancelItem);

        _tray = new NotifyIcon
        {
            Visible = true,
            Text = $"Shutdown in {Format(_remaining)}",
            ContextMenuStrip = menu,
            Icon = SystemIcons.Warning
        };
    }

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            const int WS_EX_TRANSPARENT = 0x20;
            const int WS_EX_TOOLWINDOW = 0x80;
            const int WS_EX_TOPMOST = 0x00000008;
            var cp = base.CreateParams;
            cp.ExStyle |= WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_TOPMOST;
            return cp;
        }
    }

    protected override void WndProc(ref Message m)
    {
        const int WM_NCHITTEST = 0x0084;
        const int HTTRANSPARENT = -1;
        if (m.Msg == WM_NCHITTEST)
        {
            m.Result = (IntPtr)HTTRANSPARENT;
            return;
        }
        base.WndProc(ref m);
    }

    private void OnSecondTick(object? sender, EventArgs e)
    {
        if (_remaining.TotalSeconds <= 0)
        {
            _secondTimer.Stop();
            _moveTimer.Stop();
            TryShutdown();
            return;
        }

        _remaining = _remaining.Add(TimeSpan.FromSeconds(-1));
        var text = Format(_remaining);
        _clock.Text = text;
        _tray.Text = $"Shutdown in {text}";
        CycleColor();
    }

    private static string Format(TimeSpan ts)
    {
        if (ts.TotalSeconds < 0) ts = TimeSpan.Zero;
        return $"{(int)ts.TotalMinutes:00}:{ts.Seconds:00}";
    }

    private void TryShutdown()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "shutdown",
                Arguments = "/s /t 0",
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas"
            };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to initiate shutdown:\n" + ex.Message,
                "Shutdown Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _tray.Visible = false;
            Application.Exit();
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _tray.Visible = false;
        base.OnFormClosed(e);
    }

    private void PlaceTopCenter()
    {
        var wa = Screen.PrimaryScreen!.WorkingArea;
        Location = new Point(
            x: wa.Left + (wa.Width - Width) / 2,
            y: wa.Top + TOP_MARGIN
        );
    }

    private void MoveRandomWithinPrimary()
    {
        var wa = Screen.PrimaryScreen!.WorkingArea;
        int maxX = Math.Max(wa.Left, wa.Right - Width);
        int maxY = Math.Max(wa.Top, wa.Bottom - Height);
        int x = _rng.Next(wa.Left, maxX + 1);
        int y = _rng.Next(wa.Top, maxY + 1);
        Location = new Point(x, y);
    }

    private void CycleColor()
    {
        _cycleIndex = (_cycleIndex + 1) % _cycle.Length;
        ApplyCycleColor();
    }

    private void ApplyCycleColor()
    {
        var c = _cycle[_cycleIndex];
        ForeColor = c;
        _title.ForeColor = c;
        _clock.ForeColor = c;
    }
}
