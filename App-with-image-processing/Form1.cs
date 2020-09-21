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
using System.Text.RegularExpressions;

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
                    // Se guardan los datos de la cabecera en una variable global para utilizarse en otra clase o evento
                    SharedData.Instance.headerData = headerData;
                    // Se guardan los datos de la imagen en una variable global para utilizarse en otra clase o evento
                    Bitmap bmp = new Bitmap(imagen);
                    SharedData.Instance.imageData = bmp;
                    Image image = Image.FromFile(imagen);
                    SharedData.Instance.imagePath = image;
                    // Se imprime en consola la data extraida
                    Console.WriteLine("Metadatos de la imagen elegida");
                    foreach (KeyValuePair<string, string> kvp in headerData)
                    {
                        Console.WriteLine("{0} : {1}",
                            kvp.Key, kvp.Value);
                    }
                    textBox1.Text = imagen;
                    pictureBox1.Image = bmp;
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
                    button8.Enabled = true;
                    button10.Enabled = true;
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
                    PlaceFormat(headerData["Formato de Pixel (Bitmap)"]);
                    //SharedData.Instance.imageData = NormalizeInputBitmap(bmp);
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
        private Bitmap NormalizeInputBitmap(Bitmap bmp)
        {
            // Esta función es necesaria debido a que los bitmaps modifican el RGB a BGR
            //Conversion a mapa de bits en 'data'
            System.Drawing.Imaging.BitmapData imageData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                                        System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
            // Tamaño del bitmap
            int length = Math.Abs(imageData.Stride) * bmp.Height;
            // Obtener la dirección de la primera linea
            IntPtr ptr = imageData.Scan0;
            byte[] rgbValues = new byte[length];
            // Se copian los valores RGB en un array
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, length);
            // Transponemos R -> B y B -> R
            for (int counter = 0; counter < rgbValues.Length; counter += 3)
            {
                byte dummy = rgbValues[counter];
                rgbValues[counter] = rgbValues[counter + 2];
                rgbValues[counter + 2] = dummy;
            }
            // Copiamos los valores RGB transpuestos en el bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, length);

            // Unlock the bits.
            bmp.UnlockBits(imageData);
            return bmp;
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
                while (hexString.Length < 8)
                {
                    hexString = hexString.Insert(0, "0"); // Se agregan ceros hasta tener 8 caracteres
                }
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
        // Esto función encapsula la asignación del combobox dependiendo del formato de la imagen
        private void PlaceFormat(string format)
        {
            if(format.ToLower().Contains("rgb"))
            {
                comboBox3.Text = comboBox3.Items[0].ToString();
            }
            else if (format.ToLower().Contains("rbg"))
            {
                comboBox3.Text = comboBox3.Items[1].ToString();
            }
            else if (format.ToLower().Contains("bgr"))
            {
                comboBox3.Text = comboBox3.Items[2].ToString();
            }
            else if (format.ToLower().Contains("brg"))
            {
                comboBox3.Text = comboBox3.Items[3].ToString();
            }
            else if (format.ToLower().Contains("gbr"))
            {
                comboBox3.Text = comboBox3.Items[4].ToString();
            }
            else if (format.ToLower().Contains("grb"))
            {
                comboBox3.Text = comboBox3.Items[5].ToString();
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            Bitmap bmp = SharedData.Instance.imageData;
            Dictionary<string, string> headerData = SharedData.Instance.headerData;
            // Se guarda la imagen 
            SaveFileDialog dialog = new SaveFileDialog();
            // Se filtran imagenes de tipo
            dialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*";
            try
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    // Se ajusta la nueva imagen con ls cambios realizados
                    Bitmap newImage = new Bitmap(bmp, Convert.ToInt32(headerData["Ancho (Bitmap)"]),
                                                        Convert.ToInt32(headerData["Alto (Bitmap)"]));
                    newImage.Save(dialog.FileName);
                }
                WriteLineInRichTextBox1("---> Se ha guardado la imagen en: " + dialog.FileName, Color.Blue);
            }
            catch (ArgumentNullException)
            {
                MessageBox.Show("Por favor ingrese un nombre");
            }
            
        }

        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ProgressBar1_Click(object sender, EventArgs e)
        {

        }

        private void ComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            Dictionary<string, string> headerData = SharedData.Instance.headerData;
            Bitmap bmp = SharedData.Instance.imageData;
            // Como variable de control se usa el formato de pixel, un ejemplo de esto seria cuando entra
            // un formato bgr o Bgr pasa a ser Rgb, la expresión regular [RGBrgb](bg|gr|rg|br|rb)
            // compara con cualquier formato 
            if (comboBox3.Text == "RGB")
            {
                Bitmap newOrder = new PixelRGB().Transpose(bmp, headerData["Formato de Pixel (Bitmap)"]);
                pictureBox1.Image = newOrder;
                headerData["Formato de Pixel (Bitmap)"] = Regex.Replace(headerData["Formato de Pixel (Bitmap)"],
                                                            "[RGBrgb](bg|gr|rg|br|rb)", "Rgb");
                Console.WriteLine("Formato de pixeles fijado en: " + headerData["Formato de Pixel (Bitmap)"]);
                // Se guardan los cambios en la variable global
                SharedData.Instance.imageData = newOrder;
                SharedData.Instance.headerData = headerData;

            }
            else if (comboBox3.Text == "RBG")
            {
                Bitmap newOrder = new PixelRBG().Transpose(bmp, headerData["Formato de Pixel (Bitmap)"]);
                pictureBox1.Image = newOrder;
                headerData["Formato de Pixel (Bitmap)"] = Regex.Replace(headerData["Formato de Pixel (Bitmap)"],
                                                            "[RGBrgb](gb|gr|rg|br|rb)", "Rbg");
                Console.WriteLine("Formato de pixeles fijado en: " + headerData["Formato de Pixel (Bitmap)"]);
                // Se guardan los cambios en la variable global
                SharedData.Instance.imageData = newOrder;
                SharedData.Instance.headerData = headerData;
            }
            else if (comboBox3.Text == "BGR")
            {
                Bitmap newOrder = new PixelBGR().Transpose(bmp, headerData["Formato de Pixel (Bitmap)"]);
                pictureBox1.Image = newOrder;
                headerData["Formato de Pixel (Bitmap)"] = Regex.Replace(headerData["Formato de Pixel (Bitmap)"], 
                                                            "[RGBrgb](gb|bg|rg|br|rb)", "Bgr");
                Console.WriteLine("Formato de pixeles fijado en: " + headerData["Formato de Pixel (Bitmap)"]);
                // Se guardan los cambios en la variable global
                SharedData.Instance.imageData = newOrder;
                SharedData.Instance.headerData = headerData;
            }
            else if (comboBox3.Text == "BRG")
            {
                Bitmap newOrder = new PixelBRG().Transpose(bmp, headerData["Formato de Pixel (Bitmap)"]);
                pictureBox1.Image = newOrder;
                headerData["Formato de Pixel (Bitmap)"] = Regex.Replace(headerData["Formato de Pixel (Bitmap)"],
                                                            "[RGBrgb](gb|bg|gr|br|rb)", "Brg");
                Console.WriteLine("Formato de pixeles fijado en: " + headerData["Formato de Pixel (Bitmap)"]);
                // Se guardan los cambios en la variable global
                SharedData.Instance.imageData = newOrder;
                SharedData.Instance.headerData = headerData;
            }
            else if (comboBox3.Text == "GBR")
            {
                Bitmap newOrder = new PixelGBR().Transpose(bmp, headerData["Formato de Pixel (Bitmap)"]);
                pictureBox1.Image = newOrder;
                headerData["Formato de Pixel (Bitmap)"] = Regex.Replace(headerData["Formato de Pixel (Bitmap)"],
                                                            "[RGBrgb](gb|bg|gr|rg|rb)", "Gbr");
                Console.WriteLine("Formato de pixeles fijado en: " + headerData["Formato de Pixel (Bitmap)"]);
                // Se guardan los cambios en la variable global
                SharedData.Instance.imageData = newOrder;
                SharedData.Instance.headerData = headerData;
            }
            else if (comboBox3.Text == "GRB")
            {
                Bitmap newOrder = new PixelGRB().Transpose(bmp, headerData["Formato de Pixel (Bitmap)"]);
                pictureBox1.Image = newOrder;
                headerData["Formato de Pixel (Bitmap)"] = Regex.Replace(headerData["Formato de Pixel (Bitmap)"],
                                                            "[RGBrgb](gb|bg|gr|rg|br)", "Grb");
                Console.WriteLine("Formato de pixeles fijado en: " + headerData["Formato de Pixel (Bitmap)"]);
                // Se guardan los cambios en la variable global
                SharedData.Instance.imageData = newOrder;
                SharedData.Instance.headerData = headerData;
            }
            // Se desactivan los botones de guardado y de envio
            button6.Enabled = false;
            button3.Enabled = false;
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> headerData = SharedData.Instance.headerData;
            byte[] headerToSend = SharedData.Instance.headerToSend;
            // Para imprimir en consola
            string arreglo = BitConverter.ToString(headerToSend);
            // Se ajusta el tamaño del mapa de bits al tamaño seleccionado
            Bitmap bmp = SharedData.Instance.imageData;
            Bitmap newImage = new Bitmap(bmp, Convert.ToInt32(headerData["Ancho (Bitmap)"]),
                                                        Convert.ToInt32(headerData["Alto (Bitmap)"]));
            // Se arreglan los canales intercambiados R -> B y B -> R
            newImage = NormalizeInputBitmap(newImage);
            //Conversion a mapa de bits en 'data'
            System.Drawing.Imaging.BitmapData imageData = newImage.LockBits(new Rectangle(0, 0, newImage.Width, newImage.Height),
                                                        System.Drawing.Imaging.ImageLockMode.ReadWrite, newImage.PixelFormat);
            // Tamaño del bitmap
            Console.WriteLine(imageData.Stride);
            //int length = Math.Abs(imageData.Stride) * newImage.Height;
            int length = newImage.Width * newImage.Height;
            // Obtener la dirección de la primera linea
            IntPtr ptr = imageData.Scan0;
            byte[] rgbValues = new byte[length];
            // Se copian los valores RGB en un array
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, length);
            // Copiamos los valores rgb de nuevo en el bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, length);
            // Unlock the bits.
            newImage.UnlockBits(imageData);
            // Se pasan los valores rgb de bytes a string para imprimir en consola
            string imagen1 = BitConverter.ToString(rgbValues).Replace("-", string.Empty);
            int fullLength = headerToSend.Length + rgbValues.Length;
            WriteLineInRichTextBox1("---> Enviando...", Color.Blue);
            //Envío de la cabecera por el puerto serial
            serialPort1.Write(headerToSend, 0, headerToSend.Length); // bytes 

            //Envío de la imagen por el puerto serial
            serialPort1.Write(rgbValues, 0, rgbValues.Length); // bytes 

            Console.WriteLine("La imagen enviada fue:\n" + imagen1);
            Console.WriteLine("\nLongitud total de la trama es: " + fullLength.ToString());
            Console.WriteLine("Longitud de la cabecera enviada: " + headerToSend.Length.ToString());
            Console.WriteLine("Longitud de los datos enviados: " + rgbValues.Length.ToString());
            Console.WriteLine("Tamaño deseado: " + headerData["Tamaño de la imagen (Bitmap)"]);
            WriteLineInRichTextBox1("---> Se ha enviado la imagen", Color.Red);
            Console.WriteLine("La cabecera enviada fue:" + arreglo);
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
            try
            {
                if (Convert.ToInt32(textBox2.Text) < 100000) // Tamaño maximo de 100.000 
                {
                    headerData["Alto (Bitmap)"] = textBox2.Text;
                    // Nuevo tamaño de imagen
                    headerData["Tamaño de la imagen (Bitmap)"] = (Convert.ToInt32(headerData["Alto (Bitmap)"]) * 
                                                                    Convert.ToInt32(headerData["Ancho (Bitmap)"])).ToString();
                    textBox4.Text = "";
                    foreach (KeyValuePair<string, string> kvp in headerData)
                    {
                        if (kvp.Key.Contains("(Bitmap)") && !kvp.Key.Contains("Resolución") && !kvp.Key.Contains("Formato"))
                        {
                            textBox4.Text += StringToHexString(kvp.Value);
                        }
                    }
                    Console.WriteLine("Las nuevas dimensiones son: (" + headerData["Ancho (Bitmap)"] + ","
                                        + headerData["Alto (Bitmap)"] + ")");
                    SharedData.Instance.headerData = headerData;
                }
                else
                {
                    // En caso de que se desborde
                    MessageBox.Show("Se ha superado el limite de Alto (100.000 px)");
                    textBox2.Text = headerData["Alto (Bitmap)"];
                }
            }
            catch (FormatException)
            {
                if(textBox2.Text == "")
                {
                    textBox2.Text = "";
                }
                else
                {
                    MessageBox.Show("Ingrese solo numeros");
                    textBox2.Text = headerData["Alto (Bitmap)"];
                }
            }
            
            
        }

        private void TextBox3_TextChanged(object sender, EventArgs e)
        {
            //Se modifica la cabecera cuando se cambie este campo
            Dictionary<string, string> headerData = SharedData.Instance.headerData;
            try
            {
                if (Convert.ToInt32(textBox3.Text) < 100000) // Tamaño maximo de 100.000 
                {
                    headerData["Ancho (Bitmap)"] = textBox3.Text;
                    // Nuevo tamaño de imagen
                    headerData["Tamaño de la imagen (Bitmap)"] = (Convert.ToInt32(headerData["Alto (Bitmap)"]) *
                                                                    Convert.ToInt32(headerData["Ancho (Bitmap)"])).ToString();
                    textBox4.Text = "";
                    foreach (KeyValuePair<string, string> kvp in headerData)
                    {
                        if (kvp.Key.Contains("(Bitmap)") && !kvp.Key.Contains("Resolución") && !kvp.Key.Contains("Formato"))
                        {
                            textBox4.Text += StringToHexString(kvp.Value);
                        }
                    }
                    Console.WriteLine("Las nuevas dimensiones son: (" + headerData["Ancho (Bitmap)"] + ","
                                        + headerData["Alto (Bitmap)"] + ")");
                    SharedData.Instance.headerData = headerData;
                }
                else
                {
                    // En caso de que se desborde
                    MessageBox.Show("Se ha superado el limite de Ancho (100.000 px)");
                    textBox3.Text = headerData["Ancho (Bitmap)"];
                }

            }
            catch (FormatException)
            {
                if (textBox3.Text == "")
                {
                    textBox3.Text = "";
                }
                else
                {
                    MessageBox.Show("Ingrese solo numeros");
                    textBox3.Text = headerData["Ancho (Bitmap)"];
                }
            }
            
        }

        private void Button7_Click(object sender, EventArgs e)
        {
            Bitmap bmp = SharedData.Instance.imageData;
            Dictionary<string, string> headerData = SharedData.Instance.headerData;

            bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
            pictureBox1.Image = bmp;
            // Con cada rotación de 90 ° el alto pasa a ser el ancho y el ancho pasa a ser el alto
            string dummy = headerData["Alto (Bitmap)"];
            headerData["Alto (Bitmap)"] = headerData["Ancho (Bitmap)"];
            headerData["Ancho (Bitmap)"] = dummy;
            // Guardar los cambios en las variables globales
            SharedData.Instance.headerData = headerData;
            SharedData.Instance.imageData = bmp;
            // Se coloca al final por que un cambio en estos textbox llama a los correspondientes eventos
            textBox2.Text = headerData["Alto (Bitmap)"];
            textBox3.Text = headerData["Ancho (Bitmap)"];
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
            // Se desactivan los botones de guardado y de envio
            button6.Enabled = false;
            button3.Enabled = false;
        }

        private void Button9_Click(object sender, EventArgs e)
        {
            // Aqui se prepara la información para enviar y se activan los botones correspondientes
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
                // Se guarda la cabecera en su forma de bytes en una variable global
                SharedData.Instance.headerToSend = bytes1;
                WriteLineInRichTextBox1("---> Se ha construido la imagen con la cabecera:", Color.Blue);
                WriteLineInRichTextBox1(arreglo, Color.Blue);
                // Se activa el boton de envio en caso de que se encuentre conectado
                if (serialPort1.IsOpen)
                {
                    button6.Enabled = true;
                }
                // Al crear la cabecera se habilita para guardar
                button3.Enabled = true;
                
            }
            catch(ArgumentException)
            {
                MessageBox.Show("Por favor ingrese una cabecera en formato hexadecimal valida");
            }
            
        }


        private void RichTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Button10_Click(object sender, EventArgs e)
        {
            Bitmap bmp = SharedData.Instance.imageData;
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
            pictureBox1.Image = bmp;
            // Se guardan los cambios en la variable global 
            SharedData.Instance.imageData = bmp;
            // Se desactivan los botones de guardado y de envio
            button6.Enabled = false;
            button3.Enabled = false;
        }

        private void Button8_Click(object sender, EventArgs e)
        {
            Bitmap bmp = SharedData.Instance.imageData;
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            pictureBox1.Image = bmp;
            // Se guardan los cambios en la variable global 
            SharedData.Instance.imageData = bmp;
            // Se desactivan los botones de guardado y de envio
            button6.Enabled = false;
            button3.Enabled = false;
        }
    }
}
