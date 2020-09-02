using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;

namespace Interface_To_Probe_Comunication
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            /* Este evento se activa de forma asincronica en segundo plano cuando
            se da un click en combobox, por lo que una vez que ocurra el evento,
            escribira en consola el mensaje*/
            comboBox1.Click += async (sender, e) =>
            {
                await ClickMethodAsync();
                Console.WriteLine("\r\nPuertos actualizados\n");
            };
        }

        private async Task ClickMethodAsync()
        {
            comboBox1.Items.Clear();
            String[] puertos = SerialPort.GetPortNames();
            comboBox1.Items.AddRange(puertos);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
          
        }

        private void Label1_Click(object sender, EventArgs e)
        {

        }

        private void ProgressBar1_Click(object sender, EventArgs e)
        {

        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            /*
            if (comboBox1.Text == "" || comboBox2.Text == "")
            {

            }
            else
            {
                serialPort1.PortName = comboBox1.Text;
                serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);
                serialPort1.Open();
                progressBar1.Value = 100;
                button1.Enabled = false;
                button2.Enabled = true;
            }
            */
            
            try
            {
                serialPort1.PortName = comboBox1.Text;
                serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);
                serialPort1.Open();
                progressBar1.Value = 100;
                button1.Enabled = false;
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
                button5.Enabled = true;
                distance.Enabled = true;
                traffic_light_mode.Enabled = true;
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
            catch (System.IO.IOException)
            {
                comboBox1.Text = "";
                MessageBox.Show("Por favor revise los puertos nuevamente");
            }

        }


        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string mode = traffic_light_mode.SelectedItem.ToString();
                if (mode == "Ciclico")
                {
                    Console.WriteLine("Enviar ACTIVAR modo ciclico por UART\n");
                }
                else if(mode == "Detenerse")
                {
                    Console.WriteLine("Enviar ACTIVAR modo detenerse por UART\n");
                }
            }
            catch(NullReferenceException)
            {
                Console.WriteLine("Null\n");
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            serialPort1.Close();
            button4.Enabled = false;
            button5.Enabled = false;
            distance.Enabled = false;
            button3.Enabled = false;
            button2.Enabled = false;
            traffic_light_mode.Enabled = false;
            button1.Enabled = true;
            progressBar1.Value = 0;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            serialPort1.Write("50encender1");
            Console.WriteLine("50encender1");

        }

        private void Label4_Click(object sender, EventArgs e)
        {

        }

        private void GroupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Button6_Click(object sender, EventArgs e)
        {

        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
