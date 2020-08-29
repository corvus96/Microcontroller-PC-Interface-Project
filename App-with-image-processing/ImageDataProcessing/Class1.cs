using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
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
            strategyContext.Add(nameof(JPEGHeaderFormat),
                    new JPEGHeaderFormat());
            strategyContext.Add(nameof(PNGHeaderFormat),
                    new PNGHeaderFormat());
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
            Data.BaseStream.Seek(1, SeekOrigin.Begin);
            string png = String.Concat(Convert.ToChar(Data.ReadByte()),
                                        Convert.ToChar(Data.ReadByte()),
                                        Convert.ToChar(Data.ReadByte()));
            if (bm == "BM")
            {
                return strategyContext[nameof(BMPHeaderFormat)];
            }
            else if (png == "PNG")
            // El numero magico de PNG es 89 50 4e 47 0d 0a 1a 0a donde,
            // 50 4e 47 corresponden con "PNG" en ASCII
            {
                return strategyContext[nameof(PNGHeaderFormat)];
            }
            else
            {
                // Cualquier otro formato lo detectara como jpg
                return strategyContext[nameof(JPEGHeaderFormat)];
            }
            // Si se quieren agregar mas formatos para leer sus cabeceras solo se necesita saber su numero 
            // magico o el identificador de la cabecera y la logica se implementa mediante la la interface
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
            string fichero = String.Concat(Convert.ToChar(Data.ReadByte()), Convert.ToChar(Data.ReadByte()));
            headerAssembly.Add("Tipo de fichero", fichero);
            headerAssembly.Add("Tamaño del archivo", Convert.ToString(Data.ReadInt32()));
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
            headerAssembly.Add("Tamaño de la imagen", Convert.ToString(Data.ReadInt32()));
            return headerAssembly;
        }
    }

    public class JPEGHeaderFormat : IFrameHeaderFormatStrategy
    {
        /* Esta clase implementa la interface IFrameHeaderFormatStrategy,
         * con el fin de encapsular la lógica de crear el header para imagenes 
         * JPG utilizando el patron de diseño strategy
        */
     
        // Implementación de IFrameHeaderFormatStrategy<T> interface
        public Dictionary<string, string> FrameHeaderReader(BinaryReader Data, string imagen)
        {
            Dictionary<string, string> headerAssembly = new Dictionary<string, string>();
            Console.WriteLine("JPG Acive");
            Data.BaseStream.Seek(0, SeekOrigin.Begin);
            //string fichero = Data.ReadBytes(Data.Length);
            Console.WriteLine(imagen);
            return headerAssembly;
        }
    }

    public class PNGHeaderFormat : IFrameHeaderFormatStrategy
    {
        /* Esta clase implementa la interface IFrameHeaderFormatStrategy,
         * con el fin de encapsular la lógica de crear el header para imagenes 
         * PNG utilizando el patron de diseño strategy
        */

        // Implementación de IFrameHeaderFormatStrategy<T> interface
        public Dictionary<string, string> FrameHeaderReader(BinaryReader Data, string imagen)
        {
            Dictionary<string, string> headerAssembly = new Dictionary<string, string>();
            Console.WriteLine("PNG Acive");
            FileStream fs = File.OpenRead(imagen);
            Data.BaseStream.Seek(0, SeekOrigin.Begin);
            foreach (byte data in Data.ReadBytes((int)fs.Length))
            {
                Console.Write(data.ToString("X2") + "   ");
            }
                return headerAssembly;
        }
    }
}
