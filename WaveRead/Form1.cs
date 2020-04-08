using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WaveRead
{
    public partial class Form1 : Form
    {
        Player player;
        public Form1()
        {
            InitializeComponent();
        }

        private void Open_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog open = new OpenFileDialog())          //using문은 IDisposable 객체의 올바른 사용을 보장하는 편리한 구문을 제공해 주는 것.
                //file및 font와 같은 클래스들은 관리되지 않는 리소스에 엑세스 하는 대표적인 클래스들.
                //원래 해당클래스들을 다 사용한 후에는 적절한 시기에 해제(Dispose)하여 해당 리소스(자원)을 다시 반납해야하는것ㅇㅣ다.
                //근데 매번 관리되지 않는 리소스에 엑세스 하는 클래스들을 체크하여 Dispose하는것은 많은 시간 낭비이기 때문에, 
                //using문을 이용하여 해당 리소스 범위를 벗어나게 되면 자동으로 리소스(자원)를 해제(Disppse)ㅎㅏ여 관리를 쉽게도와줌.
            {
                open.Filter = "Wav File|*.wav";
                open.InitialDirectory = Environment.CurrentDirectory;   //현재 작업 디렉토리에 정규화된 경로를 가져오거나 설정함.~~

                if(open.ShowDialog() == DialogResult.OK)
                {
                    string fileName = open.FileName;

                    player.Open(fileName);
                }

            }
        }

        private void Play_Click(object sender, EventArgs e)
        {
            player.Play();
        }
    }
}
