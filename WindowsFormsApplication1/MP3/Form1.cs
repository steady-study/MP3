using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

namespace MP3
{
    
    public partial class Form1 : Form
    {
        MP3Player mp3Player;            //여기서의 MP3Player는 MP3의 MP3Payer.

        bool isScrolled = false;
        int trackBarBlanckSize = 14;        //trackbar 양 옆 공간 선언.
        int trackBarLength = 0;              // trackbar 길이 초기화
        int trackBarMouseX = 0;            // trackbar에서 마우스 클릭 위치 초기화.


        //플레이어
        private WindowsMediaPlayerProvider _playerProvider;

        //현재 열린 wav파일
        private WavFileData _currentWavFileData;
        private FFTProvider _fftProvider;
        private SpectrumDiagramDrawer _spectrumDiagramDrawer;

        private int _lastSamplePosition;
        public bool ApplyTimeThinning = true;
        

        private Dictionary<string, object> _volumeProviderParameters;
        private Dictionary<string, object> _waveformParameters;
        private DirecBitmap _waveformBitmap;

        public int SpectrumUseSamples = 4096;
        private DirecBitmap _spectrumBitmap;
        private DirecBitmap _volumeBitmap;
        private Dictionary<string, object> _realtimeSpectrumParameters;
        private Dictionary<string, object> _volumeDrawerParameters;

        public int FramesDrawn;
        public Form1()
        {
            InitializeComponent();

            mp3Player = new MP3Player();            //여기서 MP3Player()는 MP3Player의MP3Player.
            trackBarLength = trackBar1.Size.Width - (trackBarBlanckSize * 2);       //트랙바길이에서 양옆 두공간뺀 값을 trackBarLength에.

        }

        //지정된 시간이 지날때마다 발생되는 이벤트
        private void timer1_Tick(object sender, EventArgs e)            
        {
            if (!mp3Player.isOpened)        //MP3Player의 mp3Player의 값이 isOpened이 아니면 반환...
                                                        //근데 isOpened=false라고 되있는데 그럼 반환되는건가,..
                return;


            if (isScrolled == false)        
                trackBar1.Value = mp3Player.GetPosition();      //스크롤의 위치가 트랙바와 같아짐.

            if (!mp3Player.loop && mp3Player.GetPosition() == mp3Player.GetLength())      //...  
                mp3Player.Stop();

            SetMusicTimer();
        }

        private void SetMusicTimer()
        {
            if (mp3Player.isOpened)
            {
                TimeSpan t = TimeSpan.FromMilliseconds(mp3Player.GetPosition());
                label1.Text = string.Format("{0:D2}:{1:D2}:{2:D3}", t.Minutes, t.Seconds, t.Milliseconds);
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImageUnscaled(_spectrumBitmap, 0, 0);
        }
        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImageUnscaled(_volumeBitmap, 0, 0);
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            _spectrumBitmap = new DirecBitmap(pictureBox1.Width, pictureBox1.Height);
            _volumeBitmap = new DirecBitmap(pictureBox2.Width, pictureBox2.Height);
        }

        //파일재생
        private void button1_Click(object sender, EventArgs e)
        {
            mp3Player.Play();
        }

        //파일열기
        private void button4_Click(object sender, EventArgs e)
        {
            using(OpenFileDialog open = new OpenFileDialog())
            {
                open.Filter = "Mp3 File|*.mp3";

                open.InitialDirectory = Environment.CurrentDirectory;

                if(open.ShowDialog() == DialogResult.OK)
                {
                    string fileName = open.FileName;

                    mp3Player.Open(fileName);
                    trackBar1.Maximum = mp3Player.GetLength();
                    timer1.Enabled = true;
                }
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            GeneralUpdate();
            GeneralRedraw();
        }

        private void SetPlayerProvider()
        {
            _playerProvider = new WindowsMediaPlayerProvider();
            _lastSamplePosition = 0;
        }

        private void SetVolumeProvider()
        {
            _volumeProviderParameters = new Dictionary<string, object>();
            _volumeProviderParameters["leftChannel"] = _currentWavFileData.ChannelsSamples[0];
            _volumeProviderParameters["rightChannel"] = _currentWavFileData.ChannelsSamples[1];
            _volumeProviderParameters["startsample"] = 0;
            _volumeProviderParameters["useSamples"] = SpectrumUseSamples;
            _volumeProviderParameters["type"] = 1;
        }
        private void SetWaveformProvider()
        {
            _waveformParameters = new Dictionary<string, object>();

            _waveformBitmap.Clear();

            _waveformParameters["direcBitmap"] = _waveformBitmap;
            _waveformParameters["leftColor"] = (int)(0x7cfc00 | (0xFF << 24));
            _waveformParameters["rightColor"] = (int)(0xff4500 | (0xFF << 24));
            _waveformParameters["leftChannel"] = _currentWavFileData.ChannelsSamples[0];
            _waveformParameters["samplesCount"] = _currentWavFileData.ChannelsSamples[1];
            _waveformParameters["vericalScale"] = 0.9f;
            _waveformParameters["takeRate"] = 3;
            _waveformParameters["iteration"] = 2;
            _waveformParameters["splitWorkFirst"] = true;
            _waveformParameters["portions"] = 2;

            new TrueWaveformProvider().RecreateAsync(_waveformParameters);
        }

        private void SetFFTProvider()
        {
            _fftProvider = new CorrectCooleyTukeyInPlaceFFTProvider(SpectrumUseSamples, ApplyTimeThinning);
        }

        private void SetSpectrumDrawer()
        {
            _realtimeSpectrumParameters = new Dictionary<string, object>();

            _realtimeSpectrumParameters["directBitmap"] = _spectrumBitmap;
            _realtimeSpectrumParameters["baselineY"] = pictureBox1.Height - 1;
            _realtimeSpectrumParameters["width"] = pictureBox1.Width;
            _realtimeSpectrumParameters["height"] = pictureBox1.Height;
            _realtimeSpectrumParameters["color"] = (int)(0xff4500 | (0xFF << 24)); //OrangeRed
        }

        private void SetVolumeDrawer()
        {
            _volumeDrawerParameters = new Dictionary<string, object>();
            _volumeDrawerParameters["leftColor"] = (int)(0x7cfc00 | (0xFF << 24)); //LawnGreen
            _volumeDrawerParameters["rightColor"] = (int)(0xff4500 | (0xFF << 24)); //OrangeRed
            _volumeDrawerParameters["bandWidth"] = pictureBox2.Width / 2;
            _volumeDrawerParameters["height"] = pictureBox2.Height;
            _volumeDrawerParameters["baselineY"] = pictureBox2.Height - 1;
            _volumeDrawerParameters["directBitmap"] = _volumeBitmap;
        }

        public void SetSpectrumDiagramDrawer()
        {
            _spectrumDiagramDrawer?.Cancel();
            //_spectrumDiagramDrawer = new IterationableSpectrumDiagramDrawer(SpectrumUseSamples, Rectangle.FromPictureBox(pictureBoxSpectrumDiagram), _currentWavFileData, 50);
            _spectrumDiagramDrawer = new BasicSpectrumDiagramDrawer(SpectrumUseSamples,
                Rectangle.FromPictureBox(pictureBoxSpectrumDiagram), _currentWavFileData);
            _spectrumDiagramDrawer.SetTrimmingFrequency(TrimFrequency);
            _spectrumDiagramDrawer.SetApplyTimeThinning(ApplyTimeThinning);
            _spectrumDiagramDrawer.Recreate();
        }

        private void GeneralUpdate()
        {
            float currentPosition = _playerProvider.GetElapsedSeconds();
            float duration = _playerProvider.GetDurationSeconds();

            Tuple<int, int, int> currentTime = TimeProvider.SecondsAsTime(currentPosition);
            Tuple<int, int, int> durationTime = TimeProvider.SecondsAsTime(duration);


        }

        public void GeneralRedraw()
        {
            pictureBox1.Refresh();
            pictureBox2.Refresh();
            FramesDrawn++;
        }
    }
}
