using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using ImageDataProcessing;

namespace App_with_image_processing
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Label1_Click(object sender, EventArgs e)
        {

        }

        private void Button1_Click(object sender, EventArgs e)
        {

        }

        private void RadioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void RadioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void OpenFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void Button2_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string imagen = openFileDialog1.FileName;
                    //Read the contents of the file into a stream
                    var imagenBinary = openFileDialog1.OpenFile();
                    BinaryReader reader = new BinaryReader(imagenBinary);
                    Dictionary<string, string> headerData = GetImageHeaderData(reader, imagen);
                    foreach (KeyValuePair<string, string> kvp in headerData)
                    {
                        Console.WriteLine("{0} : {1}",
                            kvp.Key, kvp.Value);
                    }
                    
                    pictureBox1.Image = Image.FromFile(imagen);
                }
            }
            catch (ArgumentException)
            {
                MessageBox.Show("El archivo seleccionado no es un tipo de imagen válido");
            }
            catch (OutOfMemoryException)
            {
                MessageBox.Show("El archivo seleccionado no es un tipo de imagen válido");
            }
        }


        private Dictionary<string, string> GetImageHeaderData(BinaryReader reader, string imagen)
        {
            // Esta clase encapsula la obtención de los datos del header de una imagen
            // utilizando las clases, metodos e interfaces definidas en el dll
            FrameHeaderStrategyContext context = new FrameHeaderStrategyContext();
            IFrameHeaderFormatStrategy strategy = context.GetStrategy(reader);
            return context.ApplyStrategy(strategy, reader, imagen);
        }

        private void Button3_Click(object sender, EventArgs e)
        {

        }
    }
}
