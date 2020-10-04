### Universidad Central de Venezuela
### Facultad de Ingeniería
### Escuela de Ingeniería Eléctrica
### Microcontroladores II




# **Aplicación para el procesamiento y envío de imágenes**
***


### Autor: Guillermo Raven 

## 1. Descripción 
La siguiente aplicación fue desarrollada en visual studio code 2019 mediante C#, para probar su funcionamiento se puede conectar un microcontrolador a la PC con pantalla LCD, TFT, etc  ó en caso de que no se posea alguno, se pueden utilizar los programas:

* VSPD [Virtual serial port website](https://www.eltima.com/es/products/vspdxp/)
* Docklight [Simulador de comunicación serial/RS232](https://docklight.de)

### **Requisitos iniciales**
- Identificar y configurar el puerto serial por el que se quiera transmitir información.
- Modificación de tasa de transmisión de las tramas enviadas.
- Crear la trama de cabecera en función al protocolo propuesto para el envió de imagen.
- Verificar si las medidas de ancho, alto y tamaño total que entrega el sistema operativo son las mismas que se calculan mediante la división de la trama total.
- Envio de tramas de imagenes via serial.
- Transposición de canales de color para imagenes RGB24.
- Rotacion y flips en X y Y de imagen.

La aplicación se encuentra estructurada de la siguiente manera:

![alt text](https://github.com/corvus96/Microcontrollers-projects/blob/master/App-with-image-processing/Diagrama%20Estructural%20de%20la%20app.svg "Diagrama estructural de la app")

La aplicación de escritorio como se puede ver en la Figura previa consta de un dll para la extracción de datos de la cabecera y la transposición de canales de color y el resto del programa se encuentra en un formulario de windows donde se manejan los eventos debido a las acciones del usuario. 

## 2. Funcionamiento de la aplicación 
### **Aplicación: Program.cs**
#### **SharedData class**
Es una clase estatica implementada mediante el patron de diseño Singleton diseñada para almacenar las variables globales que almacenan:
- headerData: es un diccionario que almacena los datos de la cabecera  extraidos de la imagen.
- imageData: es un objeto de clase Bitmap el cual podra ser manipulado por todas las funciones, eventos y clases de la aplicación.
- imagePath: es un string que almacena la dirección de la imagen seleccionada.
- headerToSend: es un arreglo de bytes que codifica la data de la cabecera a enviar. 

### **Aplicación: Form1.cs**
A continuación se presenta una breve descripción del comportamiento de cada metodo, función y evento en Form1.cs.
| Metodos, funciones y eventos        | Descripción           | Parametros de entrada  | Parametros de salida  |
| ------------------------------------|:---------------------:| :---------------------:|----------------------:|
| Form1                            | Es una función que se ejecuta al abrir el programa, de modo que plasma el formulario diseñado en pantalla, inicializa los componentes y contiene un Task que se activa asincronicamente cada vez que alguien selecciona el combo box de los puertos  | No |No |
| Task ClickMethodAsync            | Es una tarea que actualiza los puertos en segundo plano cada vez que alguien clickea el combobox de los puertos      |   No | No |
| Button2_Click_1                  | Es un evento que se ejecuta al presionar "Examinar" el cual abrira una ventana para seleccionar la imagen, extraera su respectiva data y actualizara el estado del formulario      |    No | No |
| NormalizeInputBitmap                       | Una función que encapsula la transposición del canal Rojo con el azul debido a que cuando una imagen se convierte en bitmap esta ordena los arreglos de pixeles como BGR      |    Bitmap bmp | Bitmap bmp  |
| StringToHexString                       | Una función que encapsula la conversión de un string en arreglos de bytes hexadecimales y convierte el valor en bytes en un string para su manipulación     |    String text | String hexString  |
| WriteLineInRichTextBox1                       | Una función que encapsula la escritura en una richTextBox con colores diferentes     | string text, Color color    | No  |
| GetImageHeaderData | Una función que encapsula la obtención de datos del header mediante las clases descritas en el dll | BinaryReader reader, string imagen    | Un diccionario con los datos del header  |
| PlaceFormat                       | Una función que permite seleccionar un elemento del combo box de formato dependiendo del formato de la imagen seleccionada | string format    | No  |
| Button3_Click                       | Es un evento que se ejecuta al presionar "Guardar imagen", el cual permite guardar la imagen en la previsualización | No    | No  |
| ComboBox3_SelectedIndexChanged                       | Es un evento que se ejecuta al cambiar el elemento seleccionado en el combo box de formato, el cual permite actualizar el formato de la imagen cargada en la aplicación | No    | No  |
| Button6_Click                       | Es un evento que se ejecuta al presionar "Enviar imagen", el cual envia el arreglo de bytes de la cabecera y los datos de la imagen a la velocidad y puerto seleccionado | No    | No  |
| imageResize                       | Función que permite redimensionar la imagen mediante interpolación, manteniendo la calidad de la imagen | Bitmap bitMap, int width, int height, Image image    | Bitmap newBitmap  |
| imageResize                       | Función que permite redimensionar la imagen mediante interpolación, manteniendo la calidad de la imagen | Bitmap bitMap, int width, int height, Image image    | Bitmap newBitmap  |
|  Button4_Click       | Es un evento que se ejecuta al presionar "Conectar" el cual configura el puerto seleccionado y actuliza el formulario| No    | No  |
|  Button5_Click       | Es un evento que se ejecuta al presionar "Desconectar" el cual desconecta el puerto que se encuentre conectado| No    | No  |
|  TextBox2_TextChanged       | Es un evento que se ejecuta al modificar el alto de una imagen, actualizando la data en el diccionario que almacena los datos de la cabecera y en el cuadro de texto que contiene la cabera en hexadecimal| No    | No  |
|  TextBox3_TextChanged       | Es un evento que se ejecuta al modificar el ancho de una imagen, actualizando la data en el diccionario que almacena los datos de la cabecera y en el cuadro de texto que contiene la cabera en hexadecimal| No    | No  |
|  Button7_Click      | Es un evento que se ejecuta al presionar "Rotar Imagen", este modifica los datos del diccionario de la cabecera y la data de la imagen cargada en la aplicación rotandola 90°| No    | No  |
|  TextBox4_TextChanged      | Es un evento que se ejecuta al modificar el cuadro de texto de la cabecera, este controla la habilitación del boton de "construir imagen" y la deshabilitación del los botones de enviar y guardar| No    | No  |
|  Button9_Click      | Es un evento que se ejecuta al presionar "Construir Imagen", este codifica la cabecera en arreglos de bytes hexadecimales y habilita el envio y guardado| No    | No  |
|  Button10_Click     | Es un evento que se ejecuta al presionar "Flip X", este modifica los datos de la imagen invirtiendo la posición de los pixeles respecto X| No    | No  |
|  Button8_Click      | Es un evento que se ejecuta al presionar "Flip Y", este modifica los datos de la imagen invirtiendo la posición de los pixeles respecto Y| No    | No  |
### **Aplicación dll**
* El dll se realizo de forma que pueda ser reutilizable y mantenible empleando el patron de diseño strategy para la obtención de los datos del header, debido a que dependiendo del formato de la imagen los datos que se pueden obtener son muy distintos, por ejemplo de un mapa de bits es posible obtener mayor cantidad de información que de una imagen .jpeg (debido a la longitud de las cabeceras el .bmp contiene muchos más datos), por lo que para no reducir la posibilidad a obtener a unicamente lo que los metodos propios de la libreria System.Drawing entrega, se decidio emplear esta metodología que permita modificar la lógica de algun formato en específico de forma realmente sencilla. Para ver en que se fundamenta el patron strategy se recomienda este video en que se explica como funciona dicho patron: [Patron de diseño STRATEGY]: http://www.reddit.com

* Por otro lado para la transposición de los mapa de bits se implemento mediante el patron decorator con el fin de hacer el codigo lo más sencillo posible y evitar el uso de statement blocks (if o switch).  Para ver en que se fundamenta el patron strategy se recomienda este video en que se explica como funciona dicho patron: [Patron de diseño DECORATOR]: https://www.youtube.com/watch?v=nLy4x_LPPWU

## 3. Interfaz 
A continuación se muestran algunas imagenes que muestran la interfaz de usuario diseñada
![alt text](https://github.com/corvus96/Microcontrollers-projects/blob/master/App-with-image-processing/Interfaz1.jpg "UI")

**Interfaz de usuario**

![alt text](https://github.com/corvus96/Microcontrollers-projects/blob/master/App-with-image-processing/Transpose.jpg "Transpose")

**Transposición de canales**

![alt text](https://github.com/corvus96/Microcontrollers-projects/blob/master/App-with-image-processing/flipY.jpg "flipY")

**Flip en Y**

![alt text](https://github.com/corvus96/Microcontrollers-projects/blob/master/App-with-image-processing/Seleccionar_Puerta.jpg "selección de puertos")

**Seleccionar puerto**

![alt text](https://github.com/corvus96/Microcontrollers-projects/blob/master/App-with-image-processing/Seleccionar_Baud.jpg "Selección de baud rate")

**Selección de baud rate**

![alt text](https://github.com/corvus96/Microcontrollers-projects/blob/master/App-with-image-processing/headerBMP.jpg "headerBMP")

**Header extraido de BMP**

![alt text](https://github.com/corvus96/Microcontrollers-projects/blob/master/App-with-image-processing/Rotacion.jpg "Rotacion")

**Rotación de imágenes**

## 4. Trama Enviada 
La aplicación a medida que se alteran las dimensiones actualizara la cabecera e enviar, aunque por defecto la trama tendra siguiente estructura:
![alt text](https://github.com/corvus96/Microcontrollers-projects/blob/master/App-with-image-processing/Estructura%20de%20Trama.png "Estructura de Trama")
Sin embargo es posible ingresar valores en hexadecimales para el envio, de forma que la cabecera sea personalizada por el usuario

## 5. Importante
Se recomienda emplear imagenes en RGB24 con formato BMP
