using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using NAudio.Wave;

class GlitchApp : Form
{
    private Random rand = new Random();
    private bool isGlitching = false;
    private DateTime glitchEndTime;
    private System.Windows.Forms.Timer glitchTimer;
    private System.Windows.Forms.Timer displayTimer;
    private int currentGlitchType = 0;

    // Windows API
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")]
    private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
    
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_LAYERED = 0x80000;
    private const int WS_EX_TRANSPARENT = 0x20;
    private const uint LWA_COLORKEY = 0x00000001;

    public GlitchApp()
    {
        this.FormBorderStyle = FormBorderStyle.None;
        this.WindowState = FormWindowState.Maximized;
        this.BackColor = Color.Black;
        this.TransparencyKey = Color.Black;
        this.TopMost = true;
        this.DoubleBuffered = true;
        this.Text = "GLITCH";
        this.KeyPreview = true;
        this.ShowInTaskbar = false;
        this.Opacity = 1.0;
        
        this.KeyDown += Form_KeyDown;
        this.Load += Form_Load;

        displayTimer = new System.Windows.Forms.Timer();
        displayTimer.Interval = 33;
        displayTimer.Tick += DisplayTimer_Tick;
        displayTimer.Start();

        ScheduleNextGlitch();

        this.Paint += Form_Paint;
    }

    private void Form_Load(object sender, EventArgs e)
    {
        // Set window to be transparent to clicks but visible
        int extendedStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
        SetWindowLong(this.Handle, GWL_EXSTYLE, extendedStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT);
        
        // Make black color transparent
        SetLayeredWindowAttributes(this.Handle, 0x000000, 255, LWA_COLORKEY);
        
        Console.WriteLine("[INIT] Window initialized and set to transparent");
    }

    private void Form_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            e.Handled = true;
            this.Close();
        }
    }

    private void DisplayTimer_Tick(object sender, EventArgs e)
    {
        this.Invalidate();
    }

    private void ScheduleNextGlitch()
    {
        int delaySeconds = rand.Next(15, 45);
        Console.WriteLine("[SCHEDULER] Next glitch in " + delaySeconds + "s...");
        
        glitchTimer = new System.Windows.Forms.Timer();
        glitchTimer.Interval = delaySeconds * 1000;
        glitchTimer.Tick += GlitchTimer_Tick;
        glitchTimer.Start();
    }

    private void GlitchTimer_Tick(object sender, EventArgs e)
    {
        glitchTimer.Stop();
        
        currentGlitchType = PickGlitchType();
        
        Console.WriteLine("[SCHEDULER] *** GLITCH TRIGGERED - TYPE " + currentGlitchType + " ***");
        PlayGlitchSound();
        
        isGlitching = true;
        int duration = rand.Next(0, 2001);
        glitchEndTime = DateTime.Now.AddMilliseconds(duration);
        Console.WriteLine("[SCHEDULER] Duration: " + duration + "ms");
        
        ScheduleNextGlitch();
    }

    private int PickGlitchType()
    {
        int chance = rand.Next(0, 100);
        
        if (chance < 20)
            return 4;  // Horizontal Tear - 20%
        else if (chance < 35)
            return 2;  // Vertical Shift - 15%
        else if (chance < 50)
            return 6;  // Color Shift - 15%
        else if (chance < 65)
            return 1;  // Pixel Noise - 15%
        else if (chance < 75)
            return 5;  // Scanline Flash - 10%
        else if (chance < 90)
            return 3;  // Partial Screen - 15%
        else
            return 7;  // Data Mosaic - 10%
    }

    private void Form_Paint(object sender, PaintEventArgs e)
    {
        if (isGlitching && DateTime.Now >= glitchEndTime)
        {
            isGlitching = false;
            Console.WriteLine("[DISPLAY] Glitch ended");
        }

        if (isGlitching)
        {
            DrawGlitch(e.Graphics);
        }
    }

    private void DrawGlitch(Graphics g)
    {
        int h = this.Height;
        int w = this.Width;
        
        switch (currentGlitchType)
        {
            case 1:
                GlitchType_HorizontalTear(g, h, w);
                break;
            case 2:
                GlitchType_VerticalShift(g, h, w);
                break;
            case 3:
                GlitchType_ColorShift(g, h, w);
                break;
            case 4:
                GlitchType_PixelNoise(g, h, w);
                break;
            case 5:
                GlitchType_ScanlineFlash(g, h, w);
                break;
            case 6:
                GlitchType_PartialScreen(g, h, w);
                break;
            case 7:
                GlitchType_DataMosaic(g, h, w);
                break;
        }
    }

    private void GlitchType_HorizontalTear(Graphics g, int h, int w)
    {
        for (int i = 0; i < 10; i++)
        {
            int y = rand.Next(0, h);
            int height = rand.Next(3, 20);
            int shift = rand.Next(-150, 150);
            
            using (Brush brush = new SolidBrush(Color.FromArgb(150, Color.Red)))
            {
                g.FillRectangle(brush, shift, y, w, height);
            }
        }
    }

    private void GlitchType_VerticalShift(Graphics g, int h, int w)
    {
        for (int i = 0; i < 8; i++)
        {
            int x = rand.Next(0, w);
            int width = rand.Next(5, 40);
            int shift = rand.Next(-100, 100);
            
            using (Brush brush = new SolidBrush(Color.FromArgb(120, Color.Cyan)))
            {
                g.FillRectangle(brush, x, shift, width, h);
            }
        }
    }

    private void GlitchType_ColorShift(Graphics g, int h, int w)
    {
        for (int i = 0; i < 15; i++)
        {
            int x = rand.Next(0, w - 100);
            int y = rand.Next(0, h - 50);
            int size = rand.Next(30, 150);
            
            Color[] colors = { Color.Red, Color.Green, Color.Blue, Color.Magenta, Color.Yellow };
            Color col = colors[rand.Next(colors.Length)];
            
            using (Brush brush = new SolidBrush(Color.FromArgb(100, col)))
            {
                g.FillRectangle(brush, x, y, size, size);
            }
        }
    }

    private void GlitchType_PixelNoise(Graphics g, int h, int w)
    {
        for (int i = 0; i < 500; i++)
        {
            int x = rand.Next(0, w);
            int y = rand.Next(0, h);
            Color col = Color.FromArgb(rand.Next(0, 256), rand.Next(0, 256), rand.Next(0, 256));
            
            using (Brush brush = new SolidBrush(col))
            {
                g.FillRectangle(brush, x, y, 2, 2);
            }
        }
    }

    private void GlitchType_ScanlineFlash(Graphics g, int h, int w)
    {
        using (Pen pen = new Pen(Color.FromArgb(180, Color.White), 2))
        {
            for (int y = 0; y < h; y += rand.Next(2, 8))
            {
                g.DrawLine(pen, 0, y, w, y);
            }
        }
        
        for (int i = 0; i < 5; i++)
        {
            int x = rand.Next(0, w);
            int y = rand.Next(0, h);
            
            using (Brush brush = new SolidBrush(Color.FromArgb(200, Color.White)))
            {
                g.FillRectangle(brush, x, y, rand.Next(50, 200), rand.Next(30, 100));
            }
        }
    }

    private void GlitchType_PartialScreen(Graphics g, int h, int w)
    {
        int glitchX = rand.Next(0, w - 300);
        int glitchY = rand.Next(0, h - 200);
        int glitchW = rand.Next(200, 500);
        int glitchH = rand.Next(150, 400);
        
        using (Pen pen = new Pen(Color.FromArgb(255, Color.Lime), 3))
        {
            g.DrawRectangle(pen, glitchX, glitchY, glitchW, glitchH);
        }
        
        for (int i = 0; i < 20; i++)
        {
            int x = glitchX + rand.Next(0, glitchW);
            int y = glitchY + rand.Next(0, glitchH);
            
            using (Brush brush = new SolidBrush(Color.FromArgb(150, Color.Red)))
            {
                g.FillRectangle(brush, x, y, rand.Next(20, 80), rand.Next(10, 40));
            }
        }
    }

    private void GlitchType_DataMosaic(Graphics g, int h, int w)
    {
        int blockSize = rand.Next(10, 40);
        
        for (int y = 0; y < h; y += blockSize)
        {
            for (int x = 0; x < w; x += blockSize)
            {
                if (rand.Next(0, 2) == 0)
                {
                    Color col = Color.FromArgb(rand.Next(0, 256), rand.Next(0, 256), rand.Next(0, 256));
                    using (Brush brush = new SolidBrush(Color.FromArgb(120, col)))
                    {
                        g.FillRectangle(brush, x, y, blockSize, blockSize);
                    }
                }
            }
        }
    }

    private void PlayGlitchSound()
    {
        try
        {
            string soundFile = @"C:\Users\hp\Music\virtual_vibes-glitch-sound-effect-hd-379466.mp3";
            
            if (File.Exists(soundFile))
            {
                Console.WriteLine("[SOUND] Playing: " + soundFile);
                using (var audioFileReader = new AudioFileReader(soundFile))
                using (var waveOutDevice = new WaveOutEvent())
                {
                    waveOutDevice.Init(audioFileReader);
                    waveOutDevice.Play();
                    System.Threading.Thread.Sleep(500);
                }
            }
            else
            {
                Console.WriteLine("[SOUND] File not found: " + soundFile);
                System.Media.SystemSounds.Beep.Play();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[SOUND] Error: " + ex.Message);
            System.Media.SystemSounds.Beep.Play();
        }
    }

    static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new GlitchApp());
    }
}