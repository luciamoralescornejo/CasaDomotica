using System;
using System.Collections.Generic;
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
        public MainWindow()
        {
            InitializeComponent();

            SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine();
            GrammarBuilder gb = new GrammarBuilder();
            gb.Append("Paco");
            Grammar grammar = new Grammar(gb);
            recognizer.LoadGrammar(grammar);
            recognizer.SetInputToDefaultAudioDevice();

            string frase;
            recognizer.SpeechRecognized += (s, e) =>
            {
                if (e.Result.Confidence > 0.60)
                {
                    frase = e.Result.Text.ToString();
                    lblTextoPaco.Content = frase;
                    Escuchar();
                    imgPaco.Source = new ImageSourceConverter().ConvertFromString(rutaFija + "PacoActivado.png") as ImageSource;
                }
            };

            recognizer.RecognizeAsync(RecognizeMode.Multiple);
        }

        public void Escuchar()
        {
            SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine();

            Choices comandos = new Choices();
            comandos.Add(new string[] { "enciende", "abre", "apaga", "cierra", "pon", "quita" });
            Choices elementos = new Choices();
            elementos.Add(new string[] { "puerta", "luz", "aire", "tele", "tiempo", "noticias", "youtube" });

            GrammarBuilder gb = new GrammarBuilder();
            gb.Append(comandos);
            gb.Append(elementos);
            Grammar grammar = new Grammar(gb);
            recognizer.LoadGrammar(grammar);
            recognizer.SetInputToDefaultAudioDevice();

            string frase;
            recognizer.SpeechRecognized += (s, e) =>
            {
                    frase = e.Result.Text.ToString();
                    lblTexto.Content = frase;
                    EjecutarComando(frase);
            };

            recognizer.RecognizeAsync(RecognizeMode.Single);

        }
        public void EjecutarComando(string frase)
        {
            string[] fraseSeparada = frase.Split(' ');
            switch (fraseSeparada[1])
            {
                case "puerta":
                    if (fraseSeparada[0] == "abre")
                    {
                        imgPuerta.Source = new ImageSourceConverter().ConvertFromString(rutaFija + "PuertaActivada.png") as ImageSource;
                    }
                    else
                    {
                        imgPuerta.Source = new ImageSourceConverter().ConvertFromString(rutaFija + "PuertaDesactivada.png") as ImageSource;
                    }
                    break;
                case "aire":
                    if (fraseSeparada[0] == "enciende" || fraseSeparada[0] == "pon")
                    {
                        imgAire.Source = new ImageSourceConverter().ConvertFromString(rutaFija + "AireActivado.png") as ImageSource;
                    }
                    else
                    {
                        imgAire.Source = new ImageSourceConverter().ConvertFromString(rutaFija + "AireDesactivado.png") as ImageSource;
                    }
                    break;
                case "luz":
                    if (fraseSeparada[0] == "enciende")
                    {
                        imgLuz.Source = new ImageSourceConverter().ConvertFromString(rutaFija + "LuzActivada.png") as ImageSource;
                    }
                    else
                    {
                        imgLuz.Source = new ImageSourceConverter().ConvertFromString(rutaFija + "LuzDesactivada.png") as ImageSource;
                    }
                    break;
                case "tele":
                    if (fraseSeparada[0] == "enciende" || fraseSeparada[0] == "pon")
                    {
                        imgTele.Source = new ImageSourceConverter().ConvertFromString(rutaFija + "TeleTiempo.png") as ImageSource;
                    }
                    else if (fraseSeparada[0] == "apaga" || fraseSeparada[0] == "quita")
                    {
                        imgTele.Source = new ImageSourceConverter().ConvertFromString(rutaFija + "TeleDesactivada.png") as ImageSource;
                    }
                    break;
                case "tiempo":
                    if (fraseSeparada[0] == "enciende" || fraseSeparada[0] == "pon")
                    {
                        imgTele.Source = new ImageSourceConverter().ConvertFromString(rutaFija + "TeleTiempo.png") as ImageSource;
                    }
                    else if (fraseSeparada[0] == "apaga" || fraseSeparada[0] == "quita")
                    {
                        imgTele.Source = new ImageSourceConverter().ConvertFromString(rutaFija + "TeleDesactivada.png") as ImageSource;
                    }
                    break;
                case "youtube":
                    if (fraseSeparada[0] == "enciende" || fraseSeparada[0] == "pon")
                    {
                        imgTele.Source = new ImageSourceConverter().ConvertFromString(rutaFija + "TeleYouTube.png") as ImageSource;
                    }
                    else if (fraseSeparada[0] == "apaga" || fraseSeparada[0] == "quita")
                    {
                        imgTele.Source = new ImageSourceConverter().ConvertFromString(rutaFija + "TeleDesactivada.png") as ImageSource;
                    }
                    break;
                case "Noticias":
                    if (fraseSeparada[0] == "enciende" || fraseSeparada[0] == "pon")
                    {
                        imgTele.Source = new ImageSourceConverter().ConvertFromString(rutaFija + "TeleNoticias.png") as ImageSource;
                    }
                    else if (fraseSeparada[0] == "apaga" || fraseSeparada[0] == "quita")
                    {
                        imgTele.Source = new ImageSourceConverter().ConvertFromString(rutaFija + "TeleDesactivada.png") as ImageSource;
                    }
                    break;

            }
            imgPaco.Source = new ImageSourceConverter().ConvertFromString(rutaFija + "PacoDesactivado.png") as ImageSource;
            lblTextoPaco.Content = "aa";
        }
        
    }
}