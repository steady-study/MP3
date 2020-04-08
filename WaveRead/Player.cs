using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WaveRead
{
    class Player
    {
        public bool loop { get; set; }
        public bool isOpened = false;

        private StringBuilder returnData;

        [DllImport("winmm.dll")]
        private static extern long mciSendString(string _command, StringBuilder _returnData, int _returnLenght, IntPtr _hwndCallBack);

        public Player()
        {
            returnData = new StringBuilder(128);
        }

        //fileOpen
        public void Open(string filename)
        {
            if (isOpened)
            {
                Close();
            }
            
            string command = "open \"" + filename + "\"type mpegvideo alias MediaFile";
            mciSendString(command, null,0, IntPtr.Zero);

            isOpened = true;

        }

        public void Close()
        {
            if (isOpened)
            {
                string command = "close MediaFile";
                mciSendString(command, null, 0, IntPtr.Zero);
                loop = false;
                isOpened = false;
            }
            else
            {
                loop = false;
                isOpened = false;
            }
        }

        public void Play()
        {
            if (isOpened)
            {
                string command = "play MediaFile";

                if (loop)
                    command += "REPEAT";

                mciSendString(command, null, 0, IntPtr.Zero);
            }
        }
    }
}
