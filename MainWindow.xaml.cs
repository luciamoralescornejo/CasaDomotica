using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CasaDomotica
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string rutaFija = "..\\..\\..\\imagenes\\";
        public bool escuchando = false;
        public string elemento = "";

        public Dictionary<string, Image> elementos = new Dictionary<string, Image>();
        public Dictionary<string, string> acciones = new Dictionary<string, string>();

        private SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine();
        public MainWindow()
        {
            InitializeComponent();
            ConfigDiccionarios();
            ConfigReconocedor();
        }
        public void ConfigDiccionarios()
        {
            // Diccionario de Elementos
            elementos.Add("Puerta", imgPuerta);
            elementos.Add("Luz", imgLuz);
            elementos.Add("Aire", imgAire);
            elementos.Add("Temporizador", imgTemporizador);
            elementos.Add("Television", imgTele);
            elementos.Add("Tiempo", imgTele);
            elementos.Add("Noticias", imgTele);
            elementos.Add("YouTube", imgTele);

            // Diccionario de acciones
            acciones.Add("enciende", "01");
            acciones.Add("pon", "01");
            acciones.Add("abre", "01");
            acciones.Add("cierra", "00");
            acciones.Add("quita", "00");
            acciones.Add("apaga", "00");
        }

        private void ConfigReconocedor()
        {
            // Grammar para la llamada Paco
            GrammarBuilder gbPaco = new GrammarBuilder();
            gbPaco.Append("Paco");

            Grammar grammarPaco = new Grammar(gbPaco);

            // Grammar para los comandos
            Choices comandos = new Choices();
            comandos.Add(acciones.Keys.ToArray<string>());
            Choices elementos = new Choices();
            elementos.Add(this.elementos.Keys.ToArray());

            GrammarBuilder gbComandos = new GrammarBuilder();
            gbComandos.Append(comandos);
            gbComandos.Append(elementos);

            Grammar grammarComandos = new Grammar(gbComandos);

            // Grammar de numeros
            Choices numeros = new Choices();
            numeros.Add(new string[]
            {
                "1","2","3","4","5","6","7","8","9","10",
                "11","12","13","14","15","16","17","18","19","20",
                "21","22","23","24","25","26","27","28","29","30",
                "31","32","33","34","35","36","37","38","39","40",
                "41","42","43","44","45","46","47","48","49","50",
                "51","52","53","54","55","56","57","58","59","60",

            });
            GrammarBuilder gbNumeros = new GrammarBuilder();
            gbNumeros.Append(numeros);
            Grammar grammarNumeros = new Grammar(gbNumeros);

            // Añadimos las dos Grammar y deshabilitamos la de comandos
            recognizer.LoadGrammar(grammarComandos);
            recognizer.LoadGrammar(grammarPaco);
            recognizer.LoadGrammar(grammarNumeros);
            grammarNumeros.Enabled = false;
            grammarComandos.Enabled = false;
            recognizer.EndSilenceTimeout= TimeSpan.FromSeconds(0.1);
            recognizer.SetInputToDefaultAudioDevice();
            // Logica de escucha
            string frase, accion, numero;
            escuchando = false;
            recognizer.SpeechRecognized += (s, e) =>
            {

                if (!escuchando && e.Result.Grammar == grammarPaco &&
                e.Result.Confidence > 0.50)
                {
                    escuchando = true;
                    frase = e.Result.Text.ToString();
                    lblTextoPaco.Content = frase;

                    // Cambiamos de estado
                    imgPaco.Source = new ImageSourceConverter().ConvertFromString(rutaFija + "Paco01.png") as ImageSource;
                    grammarPaco.Enabled = false;
                    grammarComandos.Enabled = true;
                }
                else if (escuchando && e.Result.Grammar == grammarComandos &&
                e.Result.Confidence > 0.30)
                {
                    frase = e.Result.Text.ToString();
                    lblTexto.Content = frase;
                    string[] fraseSplit = frase.Split(' ');
                    accion = fraseSplit[0];
                    elemento = fraseSplit[1];
                    if ((elemento == "Aire" || elemento == "Temporizador") && accion=="pon")
                    {
                        EjecutarComando(accion);
                        grammarComandos.Enabled = false;
                        grammarNumeros.Enabled = true;
                    }
                    else
                    {
                        EjecutarComando(accion);
                        // Cambiamos de estado
                        escuchando = false;
                        grammarComandos.Enabled = false;
                        grammarPaco.Enabled = true;
                        imgPaco.Source = new ImageSourceConverter().ConvertFromString(rutaFija + "Paco00.png") as ImageSource;
                        
                    }

                }else if (escuchando && e.Result.Grammar== grammarNumeros)
                {
                    numero = e.Result.Text.ToString();
                    if (elemento=="Aire")
                    {
                        lblNumeros.Content = numero;

                    }
                    if (elemento=="Temporizador")
                    {
                        lblNumerosTemp.Content = numero;
                    }
                    
                    escuchando= false;
                    grammarNumeros.Enabled = false;
                    grammarPaco.Enabled = true;
                    imgPaco.Source = new ImageSourceConverter().ConvertFromString(rutaFija + "Paco00.png") as ImageSource;
                }
                else
                {
                    lblTexto.Content = "No entiendo";
                }
            };
            recognizer.RecognizeAsync(RecognizeMode.Multiple);
        }

        
        public void EjecutarComando(string accion)
        {

            Image img = elementos[elemento];
            string ruta = rutaFija + img.Name.Substring(3);
            if (img.Name == "imgTele" && elemento != "Television")
            {
                ruta += accion == "quita" ? "01" : elemento switch
                {
                    "Noticias" => "10",
                    "Tiempo" => "11",
                    "YouTube" => "12",
                    _ => "01",
                };
            }
            else
            {
                ruta += acciones[accion];
            }
            if (acciones[accion] == "00")
            {
                if (elemento=="Aire")
                {
                    lblNumeros.Content = "";
                }
                if (elemento=="Temporizador")
                {
                    lblNumerosTemp.Content = "";
                }
            }
            img.Source = new ImageSourceConverter().ConvertFromString(ruta + ".png") as ImageSource;

        }

    }
}