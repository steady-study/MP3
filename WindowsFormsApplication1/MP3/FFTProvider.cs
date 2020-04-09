using System;

namespace MP3
{
    public abstract class FFTProvider
    {
        protected float[] Frequencies;
        protected Complex[] ValuesBuffer;
        protected int Samples;
        private bool _applyTimeThinning;

        protected abstract void Algorithm();

        public void Calculate(float[] values, int start)
        {
            CopyValues(values, start);

            Algorithm();

            TakeValues();
        }

        private void CopyValues(float[] values, int start)
        {
            if (_applyTimeThinning)
            {
                for (int i = 0; i < Samples; i++)
                {
                    if (i % 2 == 0)
                    {
                        ValuesBuffer[i].Re = values[start + i / 2];
                    }
                    else
                    {
                        ValuesBuffer[i].Re = 0;
                    }

                    ValuesBuffer[i].Im = 0;
                }
            }
            else
            {
                for (int i = 0; i < Samples; i++)
                {
                    ValuesBuffer[i].Re = values[start + i];
                    ValuesBuffer[i].Im = 0;
                }
            }
        }

        private void TakeValues()
        {
            if (_applyTimeThinning)
            {
                for (int i = 0; i < Samples; i++)
                {
                    Frequencies[i] = (float)ValuesBuffer[i].Magnitude / (Samples / 2f);
                }
            }
            else
            {
                for (int i = 0; i < Samples; i++)
                {
                    
                    Frequencies[i] = (float)ValuesBuffer[i].Magnitude / (Samples);
                }
            }
        }

        public float[] Get()
        {
            return Frequencies;
        }

        protected FFTProvider(int samples, bool applyTimeThinning)
        {
            Samples = samples;
            _applyTimeThinning = applyTimeThinning;
            Frequencies = new float[Samples];
            ValuesBuffer = new Complex[Samples];
        }
    }
}