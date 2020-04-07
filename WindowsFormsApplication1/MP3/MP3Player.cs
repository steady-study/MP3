using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MP3
{
    class MP3Player
    {
        public bool loop { get; set; } //음악 반복재생 변수. true면 반복재생O
        public bool isOpened = false;

        private StringBuilder returnData;

        [DllImport("winmm.dll")]
        private static extern long mciSendString
            (string _command, StringBuilder _returnData, int _returnLength, IntPtr _hwndCallBack);

        public MP3Player()
        {
            returnData = new StringBuilder(128);
        }

        // 파일 열기
        public void Open(string fileName)
        {
            if (isOpened)
            {
                Close();
            }

            string command = "open \"" + fileName + "\" type mpegvideo alias MediaFile";
            mciSendString(command, null, 0, IntPtr.Zero);

            isOpened = true;
        }

        // 파일 닫기
        public void Close()
        {
            if (isOpened)
            {
                string command = "close MediaFile";
                mciSendString(command, null, 0, IntPtr.Zero);
                loop = false;
                isOpened = false;
            }
        }

        // 음악 재생
        public void Play()
        {
            if (isOpened)
            {
                string command = "play MediaFile";

                if (loop)
                    command += " REPEAT";

                mciSendString(command, null, 0, IntPtr.Zero);
            }
        }

        // 음악 일시정지
        public void Pause()
        {
            if (isOpened)
            {
                string command = "pause MediaFile";
                mciSendString(command, null, 0, IntPtr.Zero);
            }
        }

        // 음악 정지
        public void Stop()
        {
            if (isOpened)
                Seek(0);
        }

        // 재생지점을 설정하는 메소드
        public void Seek(int time)
        {
            string command = "seek MediaFile to " + time.ToString();
            mciSendString(command, null, 0, IntPtr.Zero);
        }

        // 현재 상태를 얻는 메소드 (playing, pause, stopped)
        public string GetStatus()
        {
            returnData.Clear();

            string command = "status MediaFile mode";
            mciSendString(command, returnData, returnData.Capacity, IntPtr.Zero);

            return returnData.ToString();
        }

        // 음악 길이 얻는 메소드
        public int GetLength()
        {
            returnData.Clear();

            if (isOpened)
            {
                string command = "status MediaFile length";
                mciSendString(command, returnData, returnData.Capacity, IntPtr.Zero);

                int length = int.Parse(returnData.ToString());

                return length;
            }
            else
                return 0;
        }

        // 현재 재생지점(경과 시간)을 얻는 메소드
        public int GetPosition()
        {
            if (isOpened)
            {
                string command = "status MediaFile position";
                mciSendString(command, returnData, returnData.Capacity, IntPtr.Zero);

                int position = int.Parse(returnData.ToString());

                return position;
            }
            else
                return 0;
        }
    }
}

