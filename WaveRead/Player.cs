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
                try
                {                   

                    if (loop)
                        command += " REPEAT";
                    //mciSendString(command, null, 0, IntPtr.Zero);
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine("예외발생. 메시지 : {0}", ex.Message);
                }
                finally
                {
                    mciSendString(command, null, 0, IntPtr.Zero);
                }
            }
        }

        public int GetPosition()    //이걸로 시작 위치도 잡고 재생되는 위치도 잡고 끝나는 위치도 잡고 // form1.cs의 timer 이벤트로 갑시다.
        {
            if (isOpened)
            {
                string command = "status MediaFile position";
                mciSendString(command, returnData, returnData.Capacity, IntPtr.Zero);

                int position = int.Parse(returnData.ToString());        //ToString() ->  returnData값을 string으로 변환하고 그걸 또 Parse로 정수로 변환. 뭐하러 이러지
                //int position = int(returnData);     //이렇게 하니까 잘못된 항의 int라고 뜨네.

                return position;    //정수형 받은거를 다시 돌려줘서 어떤 위치로 가게 하네                            

            }
            else
                return 0;
        }
    }
}
