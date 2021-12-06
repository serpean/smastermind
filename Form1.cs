using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using SMastermind.models;
using SMastermind.utils;
using Color = System.Drawing.Color;
using InternalColor = SMastermind.models.Color;

namespace SMastermind
{
    public partial class Form1 : Form
    {
        public static readonly Dictionary<string, InternalColor> ColorMapper = new Dictionary<string, InternalColor>
        {
            { "ffff0000", InternalColor.Red },
            { "ff008000", InternalColor.Green },
            { "ff0000ff", InternalColor.Blue },
            { "ffffff00", InternalColor.Yellow },
            { "ffffa500", InternalColor.Orange },
            { "ff800080", InternalColor.Purple }
        };
        private System.Speech.Recognition.SpeechRecognitionEngine _recognizer =
           new SpeechRecognitionEngine();
        private SpeechSynthesizer synth = new SpeechSynthesizer();


        private Game game;
        private CurrentCombination currentCombination;
        private bool cheating;


        public Form1()
        {
            InitializeComponent();
        }


        /* TODO: 
         * 
         * - semántica
         * */

        private void Form1_Load(object sender, EventArgs e)
        {
            synth.Speak("Bienvenido al diseño de interfaces avanzadas. Inicializando la Aplicación");

            game = new Game();
            currentCombination = new CurrentCombination();
            cheating = false;

            Grammar grammar = CreateMastermindGrammar();
            _recognizer.SetInputToDefaultAudioDevice();
            _recognizer.UnloadAllGrammars();
            // Nivel de confianza del reconocimiento 70%
            _recognizer.UpdateRecognizerSetting("CFGConfidenceRejectionThreshold", 50);
            grammar.Enabled = true;
            _recognizer.LoadGrammar(grammar);
            _recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(_recognizer_SpeechRecognized);
            //reconocimiento asíncrono y múltiples veces
            _recognizer.RecognizeAsync(RecognizeMode.Multiple);
            synth.Speak("Aplicación preparada para reconocer su voz");
        }



        void _recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            //obtenemos un diccionario con los elementos semánticos
            SemanticValue semantics = e.Result.Semantics;

            string rawText = e.Result.Text;
            Console.WriteLine(rawText);

            if ("Salir".Equals(rawText))
            {
                Application.Exit();
            }

            if (game.IsWinner() || game.IsLooser()) // END
            {
                if (semantics.ContainsKey("int"))
                {
                    var currentAwnser = (string) semantics["int"].Value;
                    if ("Si".Equals(currentAwnser))
                    {
                        game = new Game();
                        currentCombination = new CurrentCombination();
                    } else if("No".Equals(currentAwnser))
                    {
                        Application.Exit();
                    }
                    // nothing
                }

            } else // PLAYING
            {
                Play(rawText, semantics) ;
            }

            this.label1.Text = currentCombination.ToString();




            List<string> label2Result = new List<string>();
            for(int i = 0; i < game.GetCurrentAttempt(); i++)
            {
                var curr = game.GetProposeCombinationForIndex(i);
                label2Result.Add(curr.ToString() + " blacks (" + curr.getBlacks(game.getSecretCombination()) +
                        ") whites (" + curr.getWhites(game.getSecretCombination()) + ")");

            }
            this.label2.Text = String.Join("\n", label2Result);
            this.label3.Text = cheating ? game.getSecretCombination().ToString() : "";


            Update();
        }

        private void Play(string rawText, SemanticValue semantics)
        {
            if ("Enviar".Equals(rawText))
            {
                if (currentCombination.IsComplete())
                {
                    game.AddProposeCombination(currentCombination.ToProposeCombination());
                    currentCombination = new CurrentCombination();
                    
                    if (game.IsWinner())
                    {
                        synth.Speak("Has ganado");
                    }

                    if (game.IsLooser())
                    {
                        synth.Speak("Has perdido");
                    }
                }
            }

            if ("Borrar".Equals(rawText))
            {
                _ = currentCombination.RemoveColor();
            }

            if ("Hacer trampas".Equals(rawText))
            {
                cheating = true;
            }

            if ("Ocultar trampas".Equals(rawText))
            {
                cheating = false;
            }
            
            if (semantics.ContainsKey("rgb"))
            {
                var currenctColor = Color.FromArgb((int)semantics["rgb"].Value);
                if (ColorMapper.TryGetValue(currenctColor.Name, out InternalColor currentInternalColor))
                    currentCombination.AddColor(currentInternalColor);
            }
        }

        private Grammar CreateMastermindGrammar()
        {

            // (Poner/Agregar) (bola/ficha) (C)
            // (Borrar)
            // (Enivar)
            // (Hacer trampas)
            // (Ocultar trampas)
            // (Sí/No)

            //synth.Speak("Creando ahora la gramática");
            Choices colorChoice = new Choices();

            colorChoice.Add(new SemanticResultValue("Rojo", Color.FromName("Red").ToArgb()).ToGrammarBuilder());
            colorChoice.Add(new SemanticResultValue("Azul", Color.FromName("Blue").ToArgb()).ToGrammarBuilder());
            colorChoice.Add(new SemanticResultValue("Verde", Color.FromName("Green").ToArgb()).ToGrammarBuilder());
            colorChoice.Add(new SemanticResultValue("Amarillo", Color.FromName("Yellow").ToArgb()).ToGrammarBuilder());
            colorChoice.Add(new SemanticResultValue("Naranja", Color.FromName("Orange").ToArgb()).ToGrammarBuilder());
            colorChoice.Add(new SemanticResultValue("Lila", Color.FromName("Purple").ToArgb()).ToGrammarBuilder());

            GrammarBuilder colores = new GrammarBuilder(new SemanticResultKey("rgb", colorChoice));

            GrammarBuilder ponerFicha = new GrammarBuilder();
            ponerFicha.Append("Poner ficha");
            ponerFicha.Append(colores);

            GrammarBuilder enviar = new GrammarBuilder("Enviar");
            GrammarBuilder borrar = new GrammarBuilder(new Choices(new string[] { "Borrar", "Quitar"}));
            GrammarBuilder hacerTrampas = new GrammarBuilder("Hacer trampas");
            GrammarBuilder ocultarTrampas = new GrammarBuilder("Ocultar trampas");
            GrammarBuilder salir = new GrammarBuilder("Salir");

            Choices siNoAnswer = new Choices(new string[] { "Si", "No" });
            GrammarBuilder choiceBuild = new GrammarBuilder(new SemanticResultKey("int", siNoAnswer));

            Grammar grammar = new Grammar(new Choices(ponerFicha, enviar, borrar, hacerTrampas, ocultarTrampas, salir, choiceBuild));
            grammar.Name = "(Poner/Agregar ficha/bola <color>) / (Borrar/Quitar) / (Enviar) / (Hacer Trampas) / (Ocultar Trampas) / (Sí/No)'";

            return grammar;
        }

    
        private void Form1_Paint(object sender, PaintEventArgs pe)
        {
            // Declares the Graphics object and sets it to the Graphics object  
            // supplied in the PaintEventArgs.  
            Graphics g = pe.Graphics;
            // Insert code to paint the form here.
            foreach (InternalColor color in currentCombination.getColors())
            {
                Pen myPen = new Pen(Color.Red);
                SolidBrush myBrush = new SolidBrush(Color.Red);

                g.FillCircle(myBrush, 50, 50, 20);
                g.DrawCircle(myPen, 50, 50, 20);
            }

        }
    }

}