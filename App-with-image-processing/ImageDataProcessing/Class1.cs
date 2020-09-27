using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
namespace ImageDataProcessing
{
    public class FrameHeaderStrategyContext
    {
        /* El contexto define la interfaz de interes para el usuario.
         * Mantiene una referencia a la cual acceder al momento de requerir alguna estrategia.
         * El contexto no sabe que clase concreta se implementara por
         * estrategia. Debe trabajar para todas las estrategias via 
         * IFrameHeaderFormatStrategy interface
        */
        
        Dictionary<string, IFrameHeaderFormatStrategy> strategyContext = 
            new Dictionary<string, IFrameHeaderFormatStrategy>();

        public FrameHeaderStrategyContext()
        {
            // Agrega a un diccionario las estrategias que se implementan
            // mediante la interfaz IFrameHeaderFormatStrategy
            strategyContext.Add(nameof(BMPHeaderFormat),
                new BMPHeaderFormat());
            strategyContext.Add(nameof(OtherHeaderFormat),
                    new OtherHeaderFormat());
        }

        public Dictionary<string, string> ApplyStrategy(IFrameHeaderFormatStrategy strategy, BinaryReader Data, string imagen)
        {
            /*Actualmente, ApplyStrategy tiene una implementación simple.
             * Es necesario que primero se ejecute GetStrategy para que se seleccione
             * la estrategia a utilizar al determinar dependiendo del formato de la imagen
            */
            return strategy.FrameHeaderReader(Data, imagen);
        }

        public IFrameHeaderFormatStrategy GetStrategy(BinaryReader Data)
        {
            /*
            En ausencia de este metodo se tendria que determinar el tipo de 
            archivo en cualquier lugar fuera del dll.
            Context actua como un punto unico de contacto donde el cliente puede
            que ejecutar las distintas estrategias
            */
            string bm = String.Concat(Convert.ToChar(Data.ReadByte()), Convert.ToChar(Data.ReadByte()));
            if (bm == "BM")
            {
                return strategyContext[nameof(BMPHeaderFormat)];
            }
            else
            {
                // Cualquier otro formato lo ejecutara la otra estrategia de extracción de datos
                return strategyContext[nameof(OtherHeaderFormat)];
            }
            // Si se quieren agregar especificar otras estrategias
            // especificas para otros formatos  solo se necesita saber su numero 
            // magico o el identificador de la cabecera y la logica se implementa mediante la interface
        }
    }

    public interface IFrameHeaderFormatStrategy
    {
        /* Esta interfaz recibe como input la data de una imagen
         * y dependiendo del formato se extrae la cabecera mediante 
         * distintas  estrategias para retornar un diccionario 
         * cuyo key = nombre del metadato y el value = valor extraido
         */
        Dictionary<string, string> FrameHeaderReader(BinaryReader Data, string imagen);
    }

    
    public class BMPHeaderFormat : IFrameHeaderFormatStrategy
    {
        /* Esta clase implementa la interface IFrameHeaderFormatStrategy,
         * con el fin de encapsular la lógica de crear el header para imagenes 
         * BMP utilizando el patron de diseño strategy
        */
       
        // Implementación de IFrameHeaderFormatStrategy<T> interface
        public Dictionary<string, string> FrameHeaderReader(BinaryReader Data, string imagen)
        {
            Dictionary<string, string> headerAssembly = new Dictionary<string, string>();
            Data.BaseStream.Seek(0, SeekOrigin.Begin);
            // Los siguientes parametros se hallan recorriendo la cabecera de la imagen mediante la clase BinaryReader
            string fichero = String.Concat(Convert.ToChar(Data.ReadByte()), Convert.ToChar(Data.ReadByte()));
            headerAssembly.Add("Tipo de fichero", fichero);
            headerAssembly.Add("Tamaño del archivo", Convert.ToString(Data.ReadInt32())); // Su valor es 3 veces el tamaño de la imagen + cabecera 
                                                                                          // debido a que se toman en cuenta los 3 canales de color
            Data.BaseStream.Seek(10, SeekOrigin.Begin);
            headerAssembly.Add("Inicio de los datos de la imagen", Convert.ToString(Data.ReadInt32()));
            headerAssembly.Add("Tamaño de la cabecera de bitmap", Convert.ToString(Data.ReadInt32()));
            headerAssembly.Add("Ancho", Convert.ToString(Data.ReadInt32()));
            headerAssembly.Add("Alto", Convert.ToString(Data.ReadInt32()));

            Data.BaseStream.Seek(30, SeekOrigin.Begin);
            int compresion = Data.ReadInt32();
            switch (compresion)
            {
                case 0:
                    headerAssembly.Add("compresión", "Sin compresión"); 
                    break;
                case 1:
                    headerAssembly.Add("compresión", "Compresión RLE 8 bits"); 
                    break;
                case 2:
                    headerAssembly.Add("compresión", "Compresión RLE 4 bits");  
                    break;
            }
            headerAssembly.Add("Tamaño de la imagen", Convert.ToString(Data.ReadInt32() / 3)); // Se divide entre 3 debido a que cada pixel
                                                                                               // contiene 3 bites
            headerAssembly.Add("Resolución horizontal", Convert.ToString(Data.ReadInt32()));
            headerAssembly.Add("Resolución vertical", Convert.ToString(Data.ReadInt32()));
            // Los siguientes parametros se hallan mediante lo que entrega la clase Bitmap
            Bitmap imgBit = new Bitmap(imagen);
            headerAssembly.Add("Formato de Pixel (Bitmap)", imgBit.PixelFormat.ToString());
            headerAssembly.Add("Ancho (Bitmap)", imgBit.Width.ToString());
            headerAssembly.Add("Alto (Bitmap)", imgBit.Height.ToString());
            headerAssembly.Add("Tamaño de la imagen (Bitmap)", (imgBit.Width * imgBit.Height).ToString());
            headerAssembly.Add("Resolución horizontal (Bitmap)", imgBit.HorizontalResolution.ToString());
            headerAssembly.Add("Resolución vertical (Bitmap)", imgBit.VerticalResolution.ToString());
            return headerAssembly;
        }
    }

    public class OtherHeaderFormat : IFrameHeaderFormatStrategy
    {
        /* Esta clase implementa la interface IFrameHeaderFormatStrategy,
         * con el fin de encapsular la lógica de crear el header para imagenes 
         * distintas a .BMP utilizando el patron de diseño strategy
        */
        // Implementación de IFrameHeaderFormatStrategy<T> interface
        public Dictionary<string, string> FrameHeaderReader(BinaryReader Data, string imagen)
        {
            Dictionary<string, string> headerAssembly = new Dictionary<string, string>();
            
            Bitmap imgBit = new Bitmap(imagen);
            headerAssembly.Add("Formato de Pixel (Bitmap)", imgBit.PixelFormat.ToString());
            headerAssembly.Add("Ancho (Bitmap)", imgBit.Width.ToString());
            headerAssembly.Add("Alto (Bitmap)", imgBit.Height.ToString());
            headerAssembly.Add("Tamaño de la imagen (Bitmap)", (imgBit.Width * imgBit.Height).ToString());
            headerAssembly.Add("Resolución horizontal (Bitmap)", imgBit.HorizontalResolution.ToString());
            headerAssembly.Add("Resolución vertical (Bitmap)", imgBit.VerticalResolution.ToString());
            return headerAssembly;
        }
    }

    // Esta es una implementación del patron de diseño decorator para la transposición de canales de color
    // PixelOrderDecorator de acuerdo con este patron sería el componente, este contiene 
    // la funcionalidad compartida entre las clases siguientes 
    public abstract class PixelOrderDecorator
    {
        public abstract Bitmap Transpose(string imageInput);
    }

    // Esta clase es un "componente concreto" de PixelOrderDecorator
    public class PixelRGB : PixelOrderDecorator
    {
        // La forma en la que lo hace es sobreescribiendo la funcionalidad
        // del metodo abstracto Transpose
        public override Bitmap Transpose(string imageInput)
        {
            Bitmap image = new Bitmap(imageInput);
           
            // Retorna la misma imagen ya que por defecto la librería esta hecha para recibir imagenes en RGB
            return image;
        }
    } 
    // Esta clase es un "componente concreto" de PixelOrderDecorator
    public class PixelRBG : PixelOrderDecorator
    {
        // La forma en la que lo hace es sobreescribiendo la funcionalidad
        // del metodo abstracto Transpose
        public override Bitmap Transpose(string imageInput)
        {
            Bitmap image = new Bitmap(imageInput);
            //Conversion a mapa de bits en 'data'
            System.Drawing.Imaging.BitmapData imageData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                                                        System.Drawing.Imaging.ImageLockMode.ReadWrite, image.PixelFormat);
            // Tamaño del bitmap
            int length = Math.Abs(imageData.Stride) * image.Height;
            // Obtener la dirección de la primera linea
            IntPtr ptr = imageData.Scan0;
            byte[] rgbValues = new byte[length];
            
            // Se copian los valores RGB en un array
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, length);
                  
            for (int counter = 0; counter < rgbValues.Length; counter += 3)
            {
                byte dummy = rgbValues[counter];
                rgbValues[counter] = rgbValues[counter + 1];
                rgbValues[counter + 1] = dummy;
            }
            // Se copian los valores del devuelta al bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, length);
            // Se desbloquean los bits
            image.UnlockBits(imageData);

            return image;
        }
    } 
    // Esta clase es un "componente concreto" de PixelOrderDecorator
    public class PixelBGR : PixelOrderDecorator
    {
        // La forma en la que lo hace es sobreescribiendo la funcionalidad
        // del metodo abstracto Transpose
        public override Bitmap Transpose(string imageInput)
        {
            Bitmap image = new Bitmap(imageInput);
            //Conversion a mapa de bits en 'data'
            System.Drawing.Imaging.BitmapData imageData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                                                        System.Drawing.Imaging.ImageLockMode.ReadWrite, image.PixelFormat);
            // Tamaño del bitmap
            int length = Math.Abs(imageData.Stride) * image.Height;
            // Obtener la dirección de la primera linea
            IntPtr ptr = imageData.Scan0;
            byte[] rgbValues = new byte[length];
            // Se copian los valores RGB en un array
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, length);
                  
            for (int counter = 0; counter < rgbValues.Length; counter += 3)
            {
                byte dummy = rgbValues[counter];
                rgbValues[counter] = rgbValues[counter + 2];
                rgbValues[counter + 2] = dummy;
            }
            // Se copian los valores del devuelta al bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, length);
            // Se desbloquean los bits
            image.UnlockBits(imageData);

            return image;
        }
    } 
    // Esta clase es un "componente concreto" de PixelOrderDecorator
    public class PixelBRG : PixelOrderDecorator
    {
        // La forma en la que lo hace es sobreescribiendo la funcionalidad
        // del metodo abstracto Transpose
        public override Bitmap Transpose(string imageInput)
        {
            Bitmap image = new Bitmap(imageInput);
            //Conversion a mapa de bits en 'data'
            System.Drawing.Imaging.BitmapData imageData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                                                        System.Drawing.Imaging.ImageLockMode.ReadWrite, image.PixelFormat);
            // Tamaño del bitmap
            int length = Math.Abs(imageData.Stride) * image.Height;
            // Obtener la dirección de la primera linea
            IntPtr ptr = imageData.Scan0;
            byte[] rgbValues = new byte[length];
            // Se copian los valores RGB en un array
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, length);

            for (int counter = 0; counter < rgbValues.Length; counter += 3)
            {
                byte dummy = rgbValues[counter];
                rgbValues[counter] = rgbValues[counter + 1];
                rgbValues[counter + 1] = rgbValues[counter + 2];
                rgbValues[counter + 2] = dummy;
            }
            // Se copian los valores del devuelta al bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, length);
            // Se desbloquean los bits
            image.UnlockBits(imageData);

            return image;
        }
    } 
    // Esta clase es un "componente concreto" de PixelOrderDecorator
    public class PixelGBR : PixelOrderDecorator
    {
        // La forma en la que lo hace es sobreescribiendo la funcionalidad
        // del metodo abstracto Transpose
        public override Bitmap Transpose(string imageInput)
        {
            Bitmap image = new Bitmap(imageInput);
            //Conversion a mapa de bits en 'data'
            System.Drawing.Imaging.BitmapData imageData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                                                        System.Drawing.Imaging.ImageLockMode.ReadWrite, image.PixelFormat);
            // Tamaño del bitmap
            int length = Math.Abs(imageData.Stride) * image.Height;
            // Obtener la dirección de la primera linea
            IntPtr ptr = imageData.Scan0;
            byte[] rgbValues = new byte[length];
            // Antes de transponer canales se debe comprobar que formato se posee actualmente
            // Luego se aplica la estrategia adecuada
            // Se copian los valores RGB en un array
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, length);

            for (int counter = 0; counter < rgbValues.Length; counter += 3)
            {
                byte dummy = rgbValues[counter];
                rgbValues[counter] = rgbValues[counter + 2];
                rgbValues[counter + 2] = rgbValues[counter + 1];
                rgbValues[counter + 1] = dummy;
            }
            // Se copian los valores del devuelta al bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, length);
            // Se desbloquean los bits
            image.UnlockBits(imageData);

            return image;
        }
    } 
    // Esta clase es un "componente concreto" de PixelOrderDecorator
    public class PixelGRB : PixelOrderDecorator
    {
        // La forma en la que lo hace es sobreescribiendo la funcionalidad
        // del metodo abstracto Transpose
        public override Bitmap Transpose(string imageInput)
        {
            Bitmap image = new Bitmap(imageInput);
            //Conversion a mapa de bits en 'data'
            System.Drawing.Imaging.BitmapData imageData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                                                        System.Drawing.Imaging.ImageLockMode.ReadWrite, image.PixelFormat);
            // Tamaño del bitmap
            int length = Math.Abs(imageData.Stride) * image.Height;
            // Obtener la dirección de la primera linea
            IntPtr ptr = imageData.Scan0;
            byte[] rgbValues = new byte[length];
            // Se copian los valores RGB en un array
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, length);

            for (int counter = 0; counter < rgbValues.Length; counter += 3)
            {
                byte dummy = rgbValues[counter + 1];
                rgbValues[counter + 1] = rgbValues[counter + 2];
                rgbValues[counter + 2] = dummy;
            }
            // Se copian los valores del devuelta al bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, length);
            // Se desbloquean los bits
            image.UnlockBits(imageData);

            return image;
        }
    }
}
