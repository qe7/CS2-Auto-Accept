using System;
using System.Drawing;
using System.Timers;
using System.Windows.Forms;
using Tesseract;

namespace Auto_Accept
{
    public partial class MainForm : Form
    {
        private CheckBox autoAcceptCheckBox;
        private System.Timers.Timer searchTimer;

        public MainForm()
        {
            InitializeUI();
            SetTessdataPrefix();
        }

        private void SetTessdataPrefix()
        {
            string tessdataPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
            Environment.SetEnvironmentVariable("TESSDATA_PREFIX", tessdataPath);
            Console.WriteLine($"TESSDATA_PREFIX set to: {tessdataPath}");

            string testPrefix = Environment.GetEnvironmentVariable("TESSDATA_PREFIX");
            Console.WriteLine($"TESSDATA_PREFIX is: {testPrefix}");
        }

        private void InitializeUI()
        {
            this.Text = "Shae's Auto Accept";
            this.Size = new Size(300, 150);

            autoAcceptCheckBox = new CheckBox()
            {
                Text = "Enable Auto Accept",
                Location = new Point(10, 10),
                AutoSize = true
            };

            autoAcceptCheckBox.CheckedChanged += AutoAcceptCheckBox_CheckedChanged;
            Controls.Add(autoAcceptCheckBox);

            searchTimer = new System.Timers.Timer(1000);
            searchTimer.Elapsed += SearchTimer_Elapsed;
        }

        private void AutoAcceptCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (autoAcceptCheckBox.Checked)
            {
                searchTimer.Start();
            }
            else
            {
                searchTimer.Stop();
            }
        }

        private void SearchTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SearchAndClickAccept();
        }

        private void SearchAndClickAccept()
        {
            using (var bmp = CaptureScreen())
            {
                string text = ExtractTextFromImage(bmp);

                if (text.Contains("ACCEPT"))
                {
                    int screenWidth = Screen.PrimaryScreen.Bounds.Width;
                    int screenHeight = Screen.PrimaryScreen.Bounds.Height;

                    int centerX = screenWidth / 2;
                    int centerY = screenHeight / 2 - 50; // hard coded to support 1080p, who tf plays stretch anymore

                    Cursor.Position = new Point(centerX, centerY);
                    MouseClick();
                }
            }
        }

        private Bitmap CaptureScreen()
        {
            int width = Screen.PrimaryScreen.Bounds.Width;
            int height = Screen.PrimaryScreen.Bounds.Height;

            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(0, 0, 0, 0, bmp.Size);
            }

            return bmp;
        }

        private string ExtractTextFromImage(Bitmap bmp)
        {
            string extractedText = string.Empty;
            using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
            {
                using (var img = PixConverter.ToPix(bmp))
                {
                    using (var page = engine.Process(img))
                    {
                        extractedText = page.GetText();
                    }
                }
            }

            return extractedText;
        }

        private void MouseClick()
        {
            MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftDown);
            MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftUp);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MainForm";
            this.ResumeLayout(false);

        }
    }

    public static class MouseOperations
    {
        [Flags]
        public enum MouseEventFlags : uint
        {
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(MouseEventFlags dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        public static void MouseEvent(MouseEventFlags value)
        {
            mouse_event(value, 0, 0, 0, 0);
        }
    }
}
