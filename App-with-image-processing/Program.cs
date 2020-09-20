using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace App_with_image_processing
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }

    //VARIABLES GLOBALES implementadas con patron de diseño Singleton
    public class SharedData
    {
        // La instancia de un singleton se almacenada en un campo statico
        private static SharedData instance;
        // El constructor siempre debe ser privado para evitar que sea llamado con el operador 'new' 
        private SharedData() { }
        // El siguiente metodo controla el acceso a la instancia singleton, en la primera corrida, se crea un objeto
        // singleton (SharedData) y se coloca dentro de ese campo estatico. En las siguientes corridas retorna al usuario
        // el objeto existente en el campo estatico
        public static SharedData Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SharedData();
                }
                return instance;
            }
        }

        // Aquí se encuentran las variables globales
        public Dictionary<string, string> headerData { get; set; }
        public Bitmap imageData { get; set; }
        public Image imagePath { get; set; }
        public byte[] headerToSend { get; set; }
        public byte[] dataToSend { get; set; }

    }
}
