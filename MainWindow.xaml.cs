using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Net.NetworkInformation;
using System.Speech.Recognition;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace CasaDomotica
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Constantes diversas
        private const string rutaFijaImg = "..\\..\\..\\imagenes\\";
        private const string rutaFijaSnd = "..\\..\\..\\Sonidos\\";

        // Atributos diversos
        private bool escuchando = false;
        private string elemento = "";
        private string accion = "";


        // Diccionarios
        private Dictionary<string, Image> elementos = new Dictionary<string, Image>();
        private Dictionary<string, SoundPlayer> sonidos = new Dictionary<string, SoundPlayer>();
        private Dictionary<string, string> acciones = new Dictionary<string, string>();

        // Declaramos recognizer para la escucha
        private SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine();

        // DECLARO VARIABLES PARA EL TEMPORIZADOR 

        private Stopwatch reloj;
        private TimeSpan duracionTotal;
        private DispatcherTimer temporizador;

        public MainWindow()
        {
            InitializeComponent();
            ConfigDiccionarios();
            ConfigReconocedor();
        }

        // Este método configura unos diccionarios que se usan para automatizar algunos algoritmos
        public void ConfigDiccionarios()
        {
            // Diccionario de Elementos
            elementos.Add("Puerta", imgPuerta);
            elementos.Add("Luz", imgLuz);
            elementos.Add("Aire", imgAire);
            elementos.Add("Temporizador", imgTemporizador);
            elementos.Add("Television", imgTele);
            elementos.Add("Tele", imgTele);
            elementos.Add("Tiempo", imgTele);
            elementos.Add("Noticias", imgTele);
            elementos.Add("Vegeta", imgTele);

            // Diccionario de acciones
            acciones.Add("enciende", "01");
            acciones.Add("pon", "01");
            acciones.Add("abre", "01");
            acciones.Add("cierra", "00");
            acciones.Add("quita", "00");
            acciones.Add("apaga", "00");

            // Diccionario de sonidos
            sonidos.Add("Paco00", new SoundPlayer(rutaFijaSnd + "Paco00.wav"));
            sonidos.Add("Paco01", new SoundPlayer(rutaFijaSnd + "Paco01.wav"));
            sonidos.Add("Temporizador00", new SoundPlayer(rutaFijaSnd + "Temporizador00.wav"));
            sonidos.Add("Temporizador01", new SoundPlayer(rutaFijaSnd + "Temporizador01.wav"));
            sonidos.Add("Puerta01", new SoundPlayer(rutaFijaSnd + "Puerta.wav"));
            sonidos.Add("Puerta00", new SoundPlayer(rutaFijaSnd + "Puerta.wav"));
            sonidos.Add("Luz01", new SoundPlayer(rutaFijaSnd + "Luz.wav"));
            sonidos.Add("Luz00", new SoundPlayer(rutaFijaSnd + "Luz.wav"));
            sonidos.Add("Aire01", new SoundPlayer(rutaFijaSnd + "Aire01.wav"));
            sonidos.Add("Aire00", new SoundPlayer(rutaFijaSnd + "Aire00.wav"));
            sonidos.Add("Tele00", new SoundPlayer(rutaFijaSnd + "Tele00.wav"));
            sonidos.Add("Tele01", new SoundPlayer(rutaFijaSnd + "Tele01.wav"));
            sonidos.Add("Tele10", new SoundPlayer(rutaFijaSnd + "Tele10.wav"));
            sonidos.Add("Tele11", new SoundPlayer(rutaFijaSnd + "Tele11.wav"));
            sonidos.Add("Tele12", new SoundPlayer(rutaFijaSnd + "Tele12.wav"));


            // Bucle NECESARIO para cargar los sonidos en memoria siempre, si no pueden fallar
            foreach (var s in sonidos)
            {
                try
                {
                    s.Value.Load();
                }
                catch (Exception)
                {
                    // No bloquear la inicialización de la UI si falta un archivo de sonido.
                    // Se podría loguear el error aquí.
                }
            }
        }
        // Este método configura el Recognizer para la escucha
        private void ConfigReconocedor()
        {
            // Grammar para la llamada Paco
            GrammarBuilder gbPaco = new GrammarBuilder();
            gbPaco.Append("Paco");

            Grammar grammarPaco = new Grammar(gbPaco);

            // Grammar para los comandos
            Choices accionesChoices = new Choices();
            accionesChoices.Add(acciones.Keys.ToArray<string>());
            Choices elementosChoices = new Choices();
            elementosChoices.Add(this.elementos.Keys.ToArray());

            GrammarBuilder gbComandos = new GrammarBuilder();
            gbComandos.Append(accionesChoices);
            gbComandos.Append(elementosChoices);

            Grammar grammarComandos = new Grammar(gbComandos);

            // Grammar de numeros
            Choices numerosChoices = new Choices();
            numerosChoices.Add(new string[]
            {
                "1","2","3","4","5","6","7","8","9","10",
                "11","12","13","14","15","16","17","18","19","20",
                "21","22","23","24","25","26","27","28","29","30",
                "31","32","33","34","35","36","37","38","39","40",
                "41","42","43","44","45","46","47","48","49","50",
                "51","52","53","54","55","56","57","58","59","60",

            });
            GrammarBuilder gbNumeros = new GrammarBuilder();
            gbNumeros.Append(numerosChoices);
            Grammar grammarNumeros = new Grammar(gbNumeros);

            // Añadimos las dos Grammar y deshabilitamos la de comandos
            recognizer.LoadGrammar(grammarComandos);
            recognizer.LoadGrammar(grammarPaco);
            recognizer.LoadGrammar(grammarNumeros);
            grammarNumeros.Enabled = false;
            grammarComandos.Enabled = false;
            recognizer.EndSilenceTimeout = TimeSpan.FromSeconds(0.1);
            recognizer.SetInputToDefaultAudioDevice();

            // Logica de escucha

            recognizer.SpeechRecognized += (s, e) =>
            {
                Grammar resultGrammar = e.Result.Grammar;
                float confidence = e.Result.Confidence;

                // 1º Caso de escucha: LLAMADA PACO
                if (!escuchando && resultGrammar == grammarPaco && confidence > 0.70)
                {
                    escuchando = true;
                    string frase = e.Result.Text.ToString();
                    lblTextoPaco.Content = frase;

                    // Cambiamos la gramatica para pasar al 2º Caso de escucha
                    imgPaco.Source = new ImageSourceConverter().ConvertFromString(rutaFijaImg + "Paco01.png") as ImageSource;
                    grammarPaco.Enabled = false;
                    grammarComandos.Enabled = true;
                }

                // 2º Caso de escucha: COMANDO
                else if (escuchando && resultGrammar == grammarComandos && confidence > 0.60)
                {
                    lblTextoPaco.Content = "Escuchando comando";
                    string frase = e.Result.Text.ToString();
                    lblTexto.Content = frase;
                    string[] fraseSplit = frase.Split(' ');

                    accion = fraseSplit[0];
                    elemento = fraseSplit[1];

                    // Comprobación para usar o no el 3º caso de escucha
                    if ((elemento == "Aire" || elemento == "Temporizador") && accion == "pon")
                    {
                        // Ejecutamos el comando
                        EjecutarComando();
                        // Escuchamos un numero
                        grammarComandos.Enabled = false;
                        grammarNumeros.Enabled = true;
                    }
                    else
                    {
                        // Ejecutamos el comando
                        EjecutarComando();

                        // Cambiamos la gramatica para pasar al 1º Caso de escucha
                        escuchando = false;
                        grammarComandos.Enabled = false;
                        grammarPaco.Enabled = true;
                        imgPaco.Source = new ImageSourceConverter().ConvertFromString(rutaFijaImg + "Paco00.png") as ImageSource;
                    }
                }

                // 3º Caso de escucha: NUMEROS
                else if (escuchando && resultGrammar == grammarNumeros && confidence > 0.50)
                {
                    string numero = e.Result.Text.ToString();

                    if (elemento == "Aire")
                    {
                        TxtTemperatura.Text = numero + "°C";
                        TxtTemperatura.Visibility = Visibility.Visible;
                    }

                    if (elemento == "Temporizador")
                    {
                        IniciarTemporizador(Int32.Parse(numero));
                        lblNumerosTemp.Content = numero;
                    }

                    // Cambiamos la gramatica para pasar al 1º Caso de escucha
                    escuchando = false;
                    grammarNumeros.Enabled = false;
                    grammarPaco.Enabled = true;
                    imgPaco.Source = new ImageSourceConverter().ConvertFromString(rutaFijaImg + "Paco00.png") as ImageSource;
                }

                // 4º Caso de escucha: no reconocido (No se activa si el escuchador está en el 1º caso)
                else if ((resultGrammar != grammarPaco && confidence < 0.70) ||
                (resultGrammar == grammarNumeros && confidence < 0.60) ||
                (resultGrammar == grammarComandos && confidence < 0.50))
                {
                    // Por si no lo entiende que suene un sonido de error
                    sonidos["Paco00"].Play();

                    // Cambiamos la gramatica para pasar al 1º caso de escucha
                    escuchando = false;
                    grammarComandos.Enabled = false;
                    grammarNumeros.Enabled = false;
                    grammarPaco.Enabled = true;
                    imgPaco.Source = new ImageSourceConverter().ConvertFromString(rutaFijaImg + "Paco10.png") as ImageSource;
                }
            };
            recognizer.RecognizeAsync(RecognizeMode.Multiple);
        }


        public void EjecutarComando()
        {
            // Cogemos el objeto de imagen accediendo al diccionario
            Image img = elementos[elemento];
            string accionCod = acciones[accion];

            // Iniciamos la ruta  con la ruta fija y el nombre descriptivo de la imagen sin prefijo
            string ruta = img.Name.Substring(3);

            // Como el caso de la televisión es especial ya que tiene más estados comprobamos si el estado es un programa de televisión
            if (img.Name == "imgTele" && elemento != "Television" && accionCod == "01")
            {
                ruta += elemento switch
                {
                    "Noticias" => "10",
                    "Tiempo" => "11",
                    "Vegeta" => "12",
                    _ => "01",
                };
            }

            // Si no es un estado especial de la tele añadimos a la ruta la accion
            else
            {
                ruta += accionCod;
            }

            // En caso de que se esté apagando el aire ocultaremos su etiqueta
            if (accionCod == "00" && elemento == "Aire")
            {
                TxtTemperatura.Visibility = Visibility.Hidden;
            }


            if (accionCod == "00" && elemento == "Temporizador")
            {
                if (temporizador != null && temporizador.IsEnabled)
                {
                    lblTextoPaco.Content = "entra";
                    temporizador.Stop();
                }
            }

            // Llamamos al sonido
            sonidos[ruta].Play();
            img.Source = new ImageSourceConverter().ConvertFromString(rutaFijaImg + ruta + ".png") as ImageSource;

        }
        public void IniciarTemporizador(int segundos)
        {
            duracionTotal = TimeSpan.FromSeconds(segundos);
            reloj = Stopwatch.StartNew();

            temporizador = new DispatcherTimer();

            // Cuanto menos intervalo tenga más precisión tiene.
            temporizador.Interval = TimeSpan.FromMilliseconds(200);

            // Asigno un evento de escucha (Temporizador_Tick) a un método (temporizador.Tick)
            temporizador.Tick += Temporizador_Tick;

            // Lanzo el hilo
            temporizador.Start();
        }

        private void Temporizador_Tick(object sender, EventArgs e)
        {

            // Calculo cuánto tiempo ha pasado 
            TimeSpan tiempoRestante = duracionTotal - reloj.Elapsed;

            // Calculamos la imagen segun el resto del tiempoRestante entre 12
            string numImg = tiempoRestante.Seconds % 12 < 10 ? "0" + tiempoRestante.Seconds % 12 : "" + tiempoRestante.Seconds % 12;

            // TimeSpan.Zero es un método más preciso que poner '0'
            if (tiempoRestante <= TimeSpan.Zero)
            {
                // Paramos el temporizador porque ya ha llegado a su fin
                temporizador.Stop();

                // Esta línea en realidad es redundante pero la mantengo para solucion de posibles fallos
                imgTemporizador.Source = new ImageSourceConverter().ConvertFromString(rutaFijaImg + "Temporizador" + "00" + ".png") as ImageSource;

                // Aqui se reproducira una alarma
                sonidos["Temporizador00"].Play();
                return;
            }

            // Cambiamos la imagen 
            imgTemporizador.Source = new ImageSourceConverter().ConvertFromString(rutaFijaImg + "Temporizador" + numImg + ".png") as ImageSource;

        }

    }

}