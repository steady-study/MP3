using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MP3
{
    public class TrueVolumeProvider
    {
        public float LastLeft;
        public float LastRight;

        private void MaxInRegionProvider(float[] leftChannel, float[] rightChannel, int startSample, int useSamples)
        {
            float left = 0f;
            float right = 0f;

            for(int i =startSample; i<startSample + useSamples; i++)
            {
                if (Math.Abs(leftChannel[i]) > left)
                {
                    left = Math.Abs(leftChannel[i]);
                }
                if (Math.Abs(rightChannel[i]) > right)
                {
                    right = Math.Abs(rightChannel[i]);
                }
            }
        }
    }
}
