﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MP3
{
    public class TrueWaveformProvider
    {
        private static void WriteSample(DirecBitmap bitmap, int leftColor, int rightColor, int leftSample, int rightSample, int x,
            float baseLineLeft, float baseLineRight)
        {
            if(leftSample < 0)
            {
                for(float y = baseLineLeft; y < baseLineLeft - leftSample; y++)
                {
                    bitmap.SetPixel(x, (int)y, leftColor);
                }
            }
            else
            {
                for(float y = baseLineLeft - leftSample; y<baseLineLeft; y++)
                {
                    bitmap.SetPixel(x, (int)y, leftColor);
                }
            }

            if(rightSample < 0)
            {
                for(float y = baseLineRight; y<baseLineRight-rightSample; y++)
                {
                    bitmap.SetPixel(x, (int)y, rightColor);
                }
            }
            else
            {
                for(float y = baseLineRight - rightSample; y<baseLineRight; y++)
                {
                    bitmap.SetPixel(x, (int)y, rightColor);
                }
            }
        }

        private static void MapWaveForm(int leftColor, int rightColor, float[] leftChannel, float[] rightChannel, int samplesCount,
            int startSample, int endSample, float verticalScale, int takeRate, DirecBitmap direcBitmap)
        {
            float verticalHalf = direcBitmap.Height / 2f;
            float verticalQuarter = verticalHalf / 2;
            float vericalThreeQuarters = verticalHalf * 3f / 2f;

            float maxVerticalSize = verticalQuarter * verticalScale;

            for(int currentSample = startSample; currentSample<endSample; currentSample += takeRate)
            {
                int xPosition =
                    (int)(currentSample / (float)samplesCount * direcBitmap.Width);

                int valueL = (int)(leftChannel[currentSample] * maxVerticalSize);
                int valueR = (int)(rightChannel[currentSample] * maxVerticalSize);

                WriteSample(direcBitmap, leftColor, rightColor, valueL, valueR, xPosition, vericalThreeQuarters, vericalThreeQuarters);
            }
        }

        private static void MasterWaveformMapping(int leftColor, int rightColor, float[] leftChannel, float[] rightChannel,
            int inputSamplesCount, int startSample, int endSample, float verticalScale, int portions, int iterations, bool splitWorkFirst,
            DirecBitmap direcBitmap)
        {
            int sampleToMapCount = endSample - startSample;

            int samplesPerPortion = sampleToMapCount / portions;

            if (splitWorkFirst)
            {
                for(int portion = 0; portion < portion; portion++)
                {
                    int portionStartSample = portion * samplesPerPortion;
                    int portionEndSample = (portion + 1) * samplesPerPortion - 1;

                    Debug.WriteLine("Portion {0}:{1} - {2}", portion, portionStartSample, portionEndSample);

                    for(int i =0; i<iterations; i++)
                    {
                        int iterationOffset = i;
                        int takeRate = iterations;

                        MapWaveForm(leftColor, rightColor, leftChannel, rightChannel, inputSamplesCount,
                            portionStartSample + iterationOffset, portionEndSample, verticalScale, takeRate, direcBitmap);
                    }
                }
            }
            else
            {
                for(int i = 0; i<iterations; i++)
                {
                    int iterationOffset = i;
                    int takeRate = iterations;

                    Debug.WriteLine("Iteration {0}", i);

                    for(int portion = 0; portion < portions; portion++)
                    {
                        int portionStartSample = portion * samplesPerPortion;
                        int portionEndSample = (portion + 1) * samplesPerPortion - 1;

                        MapWaveForm(leftColor, rightColor, leftChannel, rightChannel, inputSamplesCount,
                            portionStartSample + iterationOffset,
                            portionEndSample, verticalScale, takeRate, direcBitmap);
                    }
                }
            }
        }

        public void Recreate(Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("leftColor") ||
                !parameters.ContainsKey("rightColor") ||
                !parameters.ContainsKey("leftChannel") ||
                !parameters.ContainsKey("rightChannel") ||
                !parameters.ContainsKey("samplesCount") ||
                !parameters.ContainsKey("verticalScale") ||
                !parameters.ContainsKey("directBitmap") ||
                !parameters.ContainsKey("takeRate") ||
                !parameters.ContainsKey("splitWorkFirst") ||
                !parameters.ContainsKey("portions") ||
                !parameters.ContainsKey("iterations"))
            {
                throw new ArgumentException("One Of Required Parameters Missing");
            }

            int leftColor = (int)parameters["leftColor"];
            int rightColor = (int)parameters["rightColor"];
            float[] leftChannel = (float[])parameters["leftChannel"];
            float[] rightChannel = (float[])parameters["rightChannel"];
            int samplesCount = (int)parameters["samplesCount"];
            float verticalScale = (float)parameters["verticalScale"];
            DirecBitmap directBitmap = (DirecBitmap)parameters["directBitmap"];
            bool splitWorkFirst = (bool)parameters["splitWorkFirst"];
            int portions = (int)parameters["portions"];
            int iterations = (int)parameters["iterations"];

            MasterWaveformMapping(leftColor, rightColor, leftChannel, rightChannel, samplesCount,
                0, samplesCount, verticalScale, portions, iterations, splitWorkFirst, directBitmap);
        }

        public void RecreateAsync(Dictionary<string, object> parameters)
        {
            Task.Run(() => Recreate(parameters));
        }
    }
}
