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
    public partial class Mastermind : Form
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

        public static readonly Dictionary<InternalColor, Color> InternalColorToColor = new Dictionary<InternalColor, Color>
        {
            { InternalColor.NullColor, Color.LightGray },
            { InternalColor.Red, Color.Red },
            { InternalColor.Green, Color.Green},
            { InternalColor.Blue, Color.Blue },
            { InternalColor.Yellow, Color.Yellow },
            { InternalColor.Orange, Color.Orange },
            { InternalColor.Purple, Color.Purple }
        };
        private System.Speech.Recognition.SpeechRecognitionEngine _recognizer =
           new SpeechRecognitionEngine();
        private SpeechSynthesizer synth = new SpeechSynthesizer();
        private Game game;
        private CurrentCombination currentCombination;
        private bool cheating;
        private bool isGameEnd;

        public Mastermind()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            synth.Speak("Bienvenido a Mastermind.");

            game = new Game();
            currentCombination = new CurrentCombination();
            cheating = false;
            isGameEnd = false;

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
        }

        void _recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            SemanticValue semantics = e.Result.Semantics;

            string rawText = e.Result.Text;
            Console.WriteLine(rawText);

            if ("Salir".Equals(rawText))
            {
                Application.Exit();
            }

            if (isGameEnd)
            {
                AskForPlayAgain(rawText, semantics);
            } else
            {
                Play(rawText, semantics);
            }

            Invalidate();
        }

        /// <summary>
        /// Ask For Play Again Stage:
        /// Use (Si/No) gramar to play again or close the game
        /// </summary>
        /// <param name="rawText"></param>
        /// <param name="semantics"></param>
        private void AskForPlayAgain(string rawText, SemanticValue semantics)
        {

            if (semantics.ContainsKey("int"))
            {
                var currentAwnser = (string)semantics["int"].Value;
                if ("Si".Equals(currentAwnser))
                {
                    game = new Game();
                    currentCombination = new CurrentCombination();
                    label1.Text = "";
                    label2.Visible = false;
                    isGameEnd = false;
                    this.BackColor = Color.Beige;
                }
                else if ("No".Equals(currentAwnser))
                {
                    Application.Exit();
                }
                // nothing
            }

        }

        /// <summary>
        /// Playing stage.
        /// </summary>
        /// <param name="rawText"></param>
        /// <param name="semantics"></param>
        private void Play(string rawText, SemanticValue semantics)
        {

            if ("Enviar".Equals(rawText))
            {
                label3.Visible = false;
                Error error = currentCombination.IsInvalid();
                if (!Error.NullError.Equals(error))
                {
                    synth.Speak("Combinación Inválida");
                    if (Error.LengthInvalid.Equals(error))
                    {
                        label3.Text = "Longitud inválida";
                    } else if (Error.ColorTwice.Equals(error))
                    {
                        label3.Text = "No puede tener colores repetidos.";
                    }
                    label3.Visible = true;
                }
                else if (currentCombination.IsComplete())
                {
                    game.AddProposeCombination(currentCombination.ToProposeCombination());
                    currentCombination = new CurrentCombination();

                    if (game.IsWinner())
                    {
                        synth.Speak("Has ganado");
                        label1.Text = "¡Has ganado!";
                        label2.Visible = true;
                        isGameEnd = true;
                        this.BackColor = Color.LightGreen;
                    }

                    if (game.IsLooser())
                    {
                        isGameEnd = true;
                        synth.Speak("Has perdido");
                        label1.Text = "¡Has perdido!";
                        label2.Visible = true;
                        this.BackColor = Color.LightPink;
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

        /// <summary>
        /// Mastermind Grammar:
        /// <list type="bullet">
        ///     <item>(Poner/Agregar) (bola/ficha) (C [Rojo, Azul, Verde, Amarillo, Naranja, Lila])</item>
        ///     <item>Borrar</item>
        ///     <item>Enviar</item>
        ///     <item>Hacer Trampas</item>
        ///     <item>Ocultar Trampas</item>
        ///     <item>(Sí/No) [It only works when playing stage end.]</item>
        ///     <item>Salir</item>
        /// </list>
        /// 
        /// </summary>
        /// <returns>a grammar</returns>
        private Grammar CreateMastermindGrammar()
        {
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
            GrammarBuilder borrar = new GrammarBuilder(new Choices(new string[] { "Borrar", "Quitar" }));
            GrammarBuilder hacerTrampas = new GrammarBuilder("Hacer trampas");
            GrammarBuilder ocultarTrampas = new GrammarBuilder("Ocultar trampas");
            GrammarBuilder salir = new GrammarBuilder("Salir");

            Choices siNoAnswer = new Choices(new string[] { "Si", "No" });
            GrammarBuilder choiceBuild = new GrammarBuilder(new SemanticResultKey("int", siNoAnswer));

            Grammar grammar = new Grammar(new Choices(ponerFicha, enviar, borrar, hacerTrampas, ocultarTrampas, salir, choiceBuild));
            grammar.Name = "(Poner/Agregar ficha/bola <color>) / (Borrar/Quitar) / (Enviar) / (Hacer Trampas) / (Ocultar Trampas) / (Sí/No) / (Salir)";

            return grammar;
        }

        /// <summary>
        /// Recalculate Canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pe"></param>
        private void Form1_Paint(object sender, PaintEventArgs pe)
        {
            if (!isGameEnd)
            {
                Graphics g = pe.Graphics;
                var nextY = 50;
                nextY = PrintProposesCombination(g, 50, nextY, 15);
                nextY = PrintCombination(g, currentCombination, 50, nextY, 15)[1];
                if (cheating)
                {
                    PrintCombination(g, game.getSecretCombination(), 50, nextY, 15);
                }
            }
        }

        /// <summary>
        /// Helper Method that print the combination in canvas
        /// </summary>
        /// <param name="g"></param>
        /// <param name="c"></param>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        private int[] PrintCombination(Graphics g, Combination c, int startX, int startY, int radius)
        {
            Pen myPen = new Pen(Color.Black);
            int currentX = startX;
            int currentY = startY;
            foreach (InternalColor color in c.getColors())
            {
                if(InternalColorToColor.TryGetValue(color, out Color fillColor))
                {
                    SolidBrush myBrush = new SolidBrush(fillColor);

                    g.FillCircle(myBrush, currentX, startY, radius);
                    g.DrawCircle(myPen, currentX, startY, radius);

                    currentX = currentX + 2 * radius + radius / 4;
                }
            }
            currentY = currentY + 2 * radius + radius / 4;

            return new int[] { currentX, currentY };
        }

        /// <summary>
        /// Helper Method that prints blacks, whites and holes next to combination.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="c"></param>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="radius"></param>
        private void PrintBlackAndWhites(Graphics g, Combination c, int startX, int startY, int radius)
        {
            int currentX = startX;
            Pen blackPen = new Pen(Color.Black);
            SolidBrush blackBrush = new SolidBrush(Color.Black);
            Pen whitePen = new Pen(Color.Black);
            SolidBrush whiteBrush = new SolidBrush(Color.White);
            SolidBrush grayBursh = new SolidBrush(Color.LightGray);
            int iBlacks = 0;
            int iWhites = 0;
            int iHoles = 0;
            int blacks = c.getBlacks(game.getSecretCombination());
            int whites = c.getWhites(game.getSecretCombination());
            while (iBlacks + iWhites + iHoles < Combination.LENGTH){
                while(iBlacks < blacks)
                {
                    g.FillCircle(blackBrush, currentX, startY, radius);
                    g.DrawCircle(whitePen, currentX, startY, radius);
                    currentX = currentX + 2 * radius + radius / 2;
                    iBlacks++;
                    iHoles++;
                }
                while (iWhites < whites)
                {
                    // print whites
                    g.FillCircle(whiteBrush, currentX, startY, radius);
                    g.DrawCircle(blackPen, currentX, startY, radius);
                    currentX = currentX + 2 * radius + radius / 2;
                    iWhites++;
                    iHoles++;
                }
                while (iHoles < Combination.LENGTH)
                {
                    // print holes
                    g.FillCircle(grayBursh, currentX, startY, radius);
                    g.DrawCircle(blackPen, currentX, startY, radius);
                    currentX = currentX + 2 * radius + radius / 2;
                    iHoles++;
                }
            }
        }

        /// <summary>
        /// Helper method that print Propose Combination Grid.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        private int PrintProposesCombination(Graphics g, int startX, int startY, int radius)
        {
            var currentY = startY;
            for (int i = 0; i < game.GetCurrentAttempt(); i++)
            {
                ProposeCombination curr = game.GetProposeCombinationForIndex(i);
                int currentX = PrintCombination(g, curr, startX, currentY, radius)[0];
                PrintBlackAndWhites(g, curr, currentX, currentY, radius / 4);
                currentY = currentY + 2 * radius + radius / 2;
            }
            for (int i = game.GetCurrentAttempt(); i < Game.ROUNDS; i++)
            {
                Combination emptyCombination = new CurrentCombination();
                int currentX = PrintCombination(g, emptyCombination, startX, currentY, radius)[0];
                PrintBlackAndWhites(g, emptyCombination, currentX, currentY, radius / 4);
                currentY = currentY + 2 * radius + radius / 2;
            }
            return currentY;
        }
    }

}