﻿using System;
using System.IO;
using System.Threading.Tasks;

namespace MP3
{
    public class WavFileData
    {
        #region Wav Data Header

        //Done, using https://audiocoding.ru/articles/2008-05-22-wav-file-structure/

        //TODO: ENCAPSULATION FIX !IMPORTANT

        /*0  -  3*/ /* 4 */
        public string chunkId; 

        /*4  -  7*/ /* 4 */
        public int chunkSize; 

        /*8  - 11*/ /* 4 */
        public string format;

        /*12 - 15*/ /* 4 */
        public string subchunk1Id;

        /*16 - 19*/ /* 4 */
        public int subchunk1Size; 

        /*20 - 21*/ /* 2 */
        public short
            audioFormat; // https://audiocoding.ru/articles/2008-05-22-wav-file-structure/wav_formats.txt

        /*22 - 23*/ /* 2 */
        public short numChannels; 
        /*24 - 27*/ /* 4 */
        public int sampleRate; 

        /*28 - 31*/ /* 4 */
        public int byteRate; 

        /*32 - 33*/ /* 2 */
        public short blockAlign; 

        /*34 - 35*/ /* 2 */
        public short bitsPerSample; 

        /*36 - 39*/ /* 4 */
        public string subchunk2Id;

        /*40 - 43*/ /* 4 */
        public int subchunk2Size; 

        /*44 -inf*/ /* 0 */
        public byte[] WavData; 

        #endregion

        #region Используемые для вычислений данные

        public int samplesCount;

        public float[] RawSamples;

        public float[][] ChannelsSamples;

        #endregion

        
        public float[] GetSpectrumForPosition(float position, FFTProvider fftProvider)
        {
            int start = (int)(samplesCount * position);
            fftProvider.Calculate(ChannelsSamples[0], start);
            float[] spectrum = fftProvider.Get();
            return spectrum;
        }

        
        public float[] GetSpectrumForPosition(int startSample, FFTProvider fftProvider)
        {
            int start = startSample;
            fftProvider.Calculate(ChannelsSamples[0], start);
            float[] spectrum = fftProvider.Get();
            return spectrum;
        }

        private static float[] ExtractSamples(byte[] data, int bitsPerSample)
        {
            int length = data.Length;
            float[] samples;

            //REMEMBER: we subtract 1, because 1 bit is a sign and doesn't influence on sample value
            double pow2_7 = 1 << 7;
            double pow2_15 = 1 << 15;
            double pow2_31 = 1L << 31;
            double pow2_63 = 1UL << 63;

            //TODO: Add BitShifting Is Faster Than Division
            switch (bitsPerSample)
            {
                case 8:
                    samples = new float[length];
                    for (int i = 0; i < samples.Length; i++)
                    {
                        //Invert First Bit For Bitcast Types (slightly faster than subtracting)
                        int sample = (sbyte)(data[i * 1 + 0] ^ 0x80);

                        samples[i] = (float)(sample / pow2_7);
                    }

                    break;
                case 16:
                    samples = new float[length / 2];
                    for (int i = 0; i < samples.Length; i++)
                    {
                        short sample = (short)(
                            ((data[i * 2 + 0] & 0xff) << 0) | ((data[i * 2 + 1] & 0xff) << 8));
                        samples[i] = (float)(sample / pow2_15);
                    }

                    break;
                case 24:
                    samples = new float[length / 3];
                    for (int i = 0; i < samples.Length; i++)
                    {
                       
                        int sample
                            = (((data[i * 3 + 0] & 0xff) << 0) | ((data[i * 3 + 1] & 0xff) << 8) |
                               ((data[i * 3 + 2] & 0xff) << 16)) << 8;
                       
                        samples[i] =
                            (float)(sample / pow2_31);
                    }

                    break;
                case 32:
                    samples = new float[length / 4];
                    for (int i = 0; i < samples.Length; i++)
                    {
                        int sample
                            = ((data[i * 4 + 0] & 0xff) << 0) | ((data[i * 4 + 1] & 0xff) << 8) |
                              ((data[i * 4 + 2] & 0xff) << 16) | ((data[i * 4 + 3] & 0xff) << 24);
                        samples[i] = (float)(sample / pow2_31);
                    }

                    break;
                case 64:
                    samples = new float[length / 8];
                    for (int i = 0; i < samples.Length; i++)
                    {
                        long sample =
                            ((long)(data[i * 8 + 0] & 0xff) << 0) | ((long)(data[i * 8 + 1] & 0xff) << 8) |
                            ((long)(data[i * 8 + 2] & 0xff) << 16) | ((long)(data[i * 8 + 3] & 0xff) << 24) |
                            ((long)(data[i * 8 + 4] & 0xff) << 32) | ((long)(data[i * 8 + 5] & 0xff) << 40) |
                            ((long)(data[i * 8 + 6] & 0xff) << 48) | ((long)(data[i * 8 + 7] & 0xff) << 56);
                        samples[i] = (float)(sample / pow2_63);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unknown BitDepth");
            }

            return samples;
        }

        public static async Task<WavFileData> LoadWavFile(byte[] fileData)
        {
            return await Task<WavFileData>.Factory.StartNew(() => new WavFileData(fileData));
        }

        public WavFileData(byte[] fileData)
        {
            using (var ms = new MemoryStream(fileData))
            {
                BinaryReader reader = new BinaryReader(ms);

                chunkId = "" + (char)reader.ReadByte() + (char)reader.ReadByte() + (char)reader.ReadByte() +
                          (char)reader.ReadByte(); // 0x52494646
                chunkSize = reader.ReadInt32();
                format = "" + (char)reader.ReadByte() + (char)reader.ReadByte() + (char)reader.ReadByte() +
                         (char)reader.ReadByte(); // 0x57415645
                subchunk1Id = "" + (char)reader.ReadByte() + (char)reader.ReadByte() + (char)reader.ReadByte() +
                              (char)reader.ReadByte(); // 0x666d7420
                subchunk1Size = reader.ReadInt32();
                if (subchunk1Id == "JUNK")
                {
                    reader.BaseStream.Seek(subchunk1Size, SeekOrigin.Current);
                    subchunk1Id = "" + (char)reader.ReadByte() + (char)reader.ReadByte() + (char)reader.ReadByte() +
                                  (char)reader.ReadByte(); // 0x666d7420
                    subchunk1Size = reader.ReadInt32();
                }

                audioFormat = reader.ReadInt16();
                numChannels = reader.ReadInt16();
                sampleRate = reader.ReadInt32();
                byteRate = reader.ReadInt32();
                blockAlign = reader.ReadInt16();
                bitsPerSample = reader.ReadInt16();
                subchunk2Id = "" + (char)reader.ReadByte() + (char)reader.ReadByte() + (char)reader.ReadByte() +
                              (char)reader.ReadByte(); // 0x64617461
                subchunk2Size = reader.ReadInt32();

                WavData = reader.ReadBytes((int)(ms.Length - ms.Position));

                RawSamples = ExtractSamples(WavData, bitsPerSample);

                samplesCount = RawSamples.Length / numChannels;

                ChannelsSamples = ExtractChannels(RawSamples, numChannels);
            }
        }

        private static float[][] ExtractChannels(float[] samples, int numChannels)
        {
            //Here we take for granted that samples.Length is divisible by numChannels. 
            //TODO: Should rethink and check numbers for division or exclude completely

            //Create an array of channels
            float[][] channels = new float[numChannels][];
            for (int i = 0; i < numChannels; i++)
            {
                channels[i] = new float[samples.Length / numChannels];
            }

            int actualSamplesWritten = samples.Length >> 1 << 1; //Trim Very Last '1' even if not exists
            for (int i = 0; i < actualSamplesWritten; i++)
            {
                channels[i % numChannels][i / numChannels] = samples[i];
            }

            return channels;
        }
    }
}