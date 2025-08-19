using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FingerprintApp
{
    public partial class Form1 : Form
    {
        private byte[] _lastTemplate = null;
        private Bitmap _lastBitmap = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnInit_Click(object sender, EventArgs e)
        {
            byte[] deviceUUID = new byte[64];
            byte[] serialNumber = new byte[64];

            int rc = TMF20FPWrapper.tmfGetDeviceUniqueCode(deviceUUID, serialNumber);
            if (rc == 0)
            {
                string serial = System.Text.Encoding.ASCII.GetString(serialNumber).TrimEnd('\0');
                lblStatus.Text = $"Device connected. Serial: {serial}";
            }
            else
            {
                lblStatus.Text = "No fingerprint device detected.";
            }
        }

        private void btnCapture_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                lblStatus.Invoke(new Action(() => lblStatus.Text = "Place your finger..."));

                // Prepare image buffer
                byte[] imgData = new byte[1024 * 1024]; // adjust size if needed
                int[] width = new int[1];
                int[] height = new int[1];

                int rc = TMF20FPWrapper.tmfCaptureFingerprint(imgData, width, height, 10000, 1);
                if (rc == 0)
                {
                    lblStatus.Invoke(new Action(() => lblStatus.Text = "Fingerprint captured."));

                    // Show image
                    _lastBitmap?.Dispose();
                    _lastBitmap = Create8bppGrayscaleBitmap(imgData, width[0], height[0]);
                    pictureBox1.Invoke(new Action(() => pictureBox1.Image = _lastBitmap));

                    // Extract template (.FPT)
                    byte[] tzBuf = new byte[8192];
                    int[] fmrSize = new int[1];
                    rc = TMF20FPWrapper.tmfGetTzFMR(imgData, width[0], height[0], tzBuf, fmrSize);
                    if (rc != 0)
                    {
                        lblStatus.Invoke(new Action(() => lblStatus.Text = "Template extraction failed."));
                        return;
                    }

                    _lastTemplate = new byte[fmrSize[0]];
                    Array.Copy(tzBuf, _lastTemplate, fmrSize[0]);
                    lblStatus.Invoke(new Action(() => lblStatus.Text = "Template ready to save."));
                }
                else
                {
                    lblStatus.Invoke(new Action(() => lblStatus.Text = "Capture failed."));
                }
            });
        }

        private void btnSaveTemplate_Click(object sender, EventArgs e)
        {
            if (_lastTemplate == null)
            {
                lblStatus.Text = "No template captured.";
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Fingerprint Template (*.fpt)|*.fpt",
                FileName = "fingerprint.fpt"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllBytes(sfd.FileName, _lastTemplate);
                lblStatus.Text = "Template saved: " + sfd.FileName;
            }
        }

        private static Bitmap Create8bppGrayscaleBitmap(byte[] raw, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            var pal = bmp.Palette;
            for (int i = 0; i < 256; i++) pal.Entries[i] = Color.FromArgb(i, i, i);
            bmp.Palette = pal;

            var rect = new Rectangle(0, 0, width, height);
            var data = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);
            int stride = data.Stride;
            IntPtr scan0 = data.Scan0;

            for (int y = 0; y < height; y++)
                System.Runtime.InteropServices.Marshal.Copy(raw, y * width, scan0 + y * stride, width);

            bmp.UnlockBits(data);
            return bmp;
        }
    }
}
