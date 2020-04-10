using System.Drawing;

namespace MP3
{
    public abstract class SpectrumDiagramDrawer     //abstract 한정자는 추상클래스를 만듦. /추상클래스는 미완성 클래스를 말함.
        //추상클래스는 몸체없는 함수를 하나라도 포함하고 있거나 몸체없는 함수를 포함하고 있지 않더라도 클래스를 선언할때 abstract키워드를 포함하고 있는 경우.
        //추상 클래스는 프로그램 설계 단계에서 많이 사용됨. 독자적으로 사용되는 클래스는 아니지만, 프로그램 상황에 맞는 계층적인 모델을 설계할때 추상클래스 사용.        
    {
        protected int SpectrumSamples;
        protected float[] SpectrumValues;

        protected Rectangle DisplayRectangle;
        protected FFTProvider FftProvider;
        protected WavFileData FileData;

        protected DirecBitmap Diagram;
        protected Color Color;

        protected int TrimmingFrequency;    //트리밍주파수.
        protected bool ApplyTimeThinning;

        protected bool Canceled;
        
        public void SetTrimmingFrequency(int frequency)
        {
            TrimmingFrequency = frequency;
        }

        public void SetApplyTimeThinning(bool apply)
        {
            ApplyTimeThinning = apply;
            FftProvider = new CorrectCooleyTukeyInPlaceFFTProvider(SpectrumSamples, ApplyTimeThinning);
        }

        protected int IntensityToArgb(float intesity)
        {
            int lowEnd = 0;
            int alpha = (int)(lowEnd + intesity * (byte.MaxValue - lowEnd));    //intesity가  그냥 변수 선언만 해놨는데 이게 되나?
            int red = Color.R;
            int green = Color.G;
            int blue = Color.B;
            int argb = (int)((uint)(red << 16 | green << 8 | blue | alpha << 24));
            return argb;
        }

        public abstract void Draw(Graphics g);      //System.Drawing.Grapics -> GDI+ 그리기화면을 캡슐화 함.

        public abstract void Recreate();            //아직 모름.

        public void Cancel()
        {
            Canceled = true;
        }

        public SpectrumDiagramDrawer(int spectrumSamples, Rectangle displayRectangle, WavFileData filedata)
        {
            SpectrumSamples = spectrumSamples;
            SpectrumValues = new float[SpectrumSamples];
            DisplayRectangle = displayRectangle;
            Diagram = new DirecBitmap((int)displayRectangle.Width, (int)displayRectangle.Height);
            Color = Color.OrangeRed;

            FftProvider = new CorrectCooleyTukeyInPlaceFFTProvider(SpectrumSamples, ApplyTimeThinning);
            FileData = filedata;
        }

        ~SpectrumDiagramDrawer()
        {
            Diagram.Dispose();
        }
    }
}
