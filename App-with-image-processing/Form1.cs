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
using System.IO.Ports;

namespace App_with_image_processing
{
    

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            /* Este evento se activa de forma asincronica en segundo plano cuando
           se da un click en combobox, por lo que una vez que ocurra el evento,
           escribira en consola el mensaje*/
            comboBox4.Click += async (sender, e) =>
            {
                await ClickMethodAsync();
                Console.WriteLine("\r\nPuertos actualizados\n");
            };

            
        }

        private async Task ClickMethodAsync()
        {
            comboBox4.Items.Clear();
            String[] puertos = SerialPort.GetPortNames();
            comboBox4.Items.AddRange(puertos);
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
                    // se almacena el PATH de la imagen seleccionada en "imagen"
                    string imagen = openFileDialog1.FileName;
                    var imagenBinary = openFileDialog1.OpenFile();
                    // Se crea el objeto para leer bit a bit el archivo
                    BinaryReader reader = new BinaryReader(imagenBinary);
                    // se ejecuta el metodo que devuelve un diccionario con los datos de la cabecera
                    Dictionary<string, string> headerData = GetImageHeaderData(reader, imagen);
                    // Se guardan los datos de la cabecera en una variable global para utilizarse en otro clase o evento
                    SharedData.Instance.headerData = headerData;
                    // Se imprime en consola la data extraida
                    Console.WriteLine("Metadatos de la imagen elegida");
                    foreach (KeyValuePair<string, string> kvp in headerData)
                    {
                        Console.WriteLine("{0} : {1}",
                            kvp.Key, kvp.Value);
                    }
                    textBox1.Text = imagen;
                    pictureBox1.Image = Image.FromFile(imagen);
                    comboBox3.Enabled = true;
                    // Se actualiza el ancho
                    textBox3.Enabled = true;
                    textBox3.Text = headerData["Ancho (Bitmap)"];
                    // Se actualiza la altura
                    textBox2.Enabled = true;
                    textBox2.Text = headerData["Alto (Bitmap)"];

                    textBox4.Enabled = true;
                    button1.Enabled = true;
                    button7.Enabled = true;
                    // Se actualiza la cabecera de la trama limpiandola 
                    textBox4.Text = "";
                    // Se escribe en el log para realimentar con información al usuario
                    WriteLineInRichTextBox1("---> Imagen nueva agregada:" + imagen , Color.Green);
                    // Update cabecera
                    foreach (KeyValuePair<string, string> kvp in headerData)
                    {
                        if (kvp.Key.Contains("(Bitmap)") && !kvp.Key.Contains("Resolución") && !kvp.Key.Contains("Formato"))
                        {
                            textBox4.Text += StringToHexString(kvp.Value);
                        }
                    }

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

        private string StringToHexString(string text)
        {
            /* Mediante un protocolo propio la cabecera a enviar debe ser
                 * 4 bytes para el ancho en int 32
                 * 4 bytes para el alto en int 32
                 * 4 bytes para el tamaño en int 32. En total la cabecera que se creara de acuerdo a las
                 * modificaciones del sistema tendra 12 byte, no obstante el usuario podra ingresar una 
                 cabecera propia en hexadecimal*/
            string hexString;
            try
            {
                // Convert string to int
                int intValue = Convert.ToInt32(text);
                // convert intValue to a hex in a string variable
                hexString = intValue.ToString("X2");
            }
            catch(FormatException)
            {
                byte[] ba = Encoding.Default.GetBytes(text);
                hexString = BitConverter.ToString(ba).Replace("-", string.Empty);
            }
            catch (OverflowException)
            {
                MessageBox.Show("Cuidado, Ha alcanzado el limite en ancho o alto");
                int intValue = Int32.MaxValue;
                hexString = intValue.ToString("X2");
            }
            
            return hexString;
        }

        private void WriteLineInRichTextBox1(string text, Color color)
        {
            richTextBox1.SelectionColor = color;
            richTextBox1.AppendText("  "+ text + "\n");
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

        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ProgressBar1_Click(object sender, EventArgs e)
        {

        }

        private void ComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Button6_Click(object sender, EventArgs e)
        {

        }

        private void ComboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
               
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.PortName = comboBox4.Text;
                serialPort1.BaudRate = Convert.ToInt32(comboBox1.Text);
                serialPort1.Open();
                progressBar1.Value = 100;
                button5.Enabled = true;
                button4.Enabled = false;
                comboBox1.Enabled = false;
                comboBox4.Enabled = false;
            }
            catch (UnauthorizedAccessException)
            {

            }
            catch (ArgumentException)
            {
                MessageBox.Show("El puerto no comienza con COM o el tipo de archivo en el puerto no es soportado");
            }
            catch (FormatException)
            {
                MessageBox.Show("Es necesario que especifique todos los datos, para abrir el puerto");
            }
            catch (IOException)
            {
                comboBox1.Text = "";
                MessageBox.Show("Por favor revise los puertos nuevamente");
            }
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            serialPort1.Close();
            button4.Enabled = true;
            button5.Enabled = false;
            comboBox1.Enabled = true;
            comboBox4.Enabled = true;
            progressBar1.Value = 0;
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void TextBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void TextBox2_TextChanged(object sender, EventArgs e)
        {
            //Se modifica la cabecera cuando se cambie este campo
            Dictionary<string, string> headerData = SharedData.Instance.headerData;
            headerData["Alto (Bitmap)"] = textBox2.Text;
            textBox4.Text = "";
            foreach (KeyValuePair<string, string> kvp in headerData)
            {
                if (kvp.Key.Contains("(Bitmap)") && !kvp.Key.Contains("Resolución") && !kvp.Key.Contains("Formato"))
                {
                    textBox4.Text += StringToHexString(kvp.Value);
                }
            }
        }

        private void TextBox3_TextChanged(object sender, EventArgs e)
        {
            //Se modifica la cabecera cuando se cambie este campo
            Dictionary<string, string> headerData = SharedData.Instance.headerData;
            headerData["Ancho (Bitmap)"] = textBox3.Text;
            textBox4.Text = "";
            foreach (KeyValuePair<string, string> kvp in headerData)
            {
                if (kvp.Key.Contains("(Bitmap)") && !kvp.Key.Contains("Resolución") && !kvp.Key.Contains("Formato"))
                {
                    textBox4.Text += StringToHexString(kvp.Value);
                }
            }
        }

        private void Button7_Click(object sender, EventArgs e)
        {

        }

        private void TextBox4_TextChanged(object sender, EventArgs e)
        {
            if(textBox4.Text != "")
            {
                button9.Enabled = true;
            }
            else
            {
                button9.Enabled = false;
            }
        }

        private void Button9_Click(object sender, EventArgs e)
        {

            string hex = textBox4.Text;
            int NumberChars = hex.Length;
            byte[] bytes1 = new byte[NumberChars / 2];
            try
            {
                for (int d = 0; d < NumberChars; d += 2)
                {
                    bytes1[d / 2] = Convert.ToByte(hex.Substring(d, 2), 16);

                }
                string arreglo = BitConverter.ToString(bytes1);
                WriteLineInRichTextBox1("---> Se ha construido la imagen con la cabecera:", Color.Blue);
                WriteLineInRichTextBox1(arreglo, Color.Blue);
            }
            catch(ArgumentException)
            {
                MessageBox.Show("Por favor ingrese una cabecera en formato hexadecimal valida");
            }

            
        }


        private void RichTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
