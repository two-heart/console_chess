using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO;

/*
TODO:
KI Rochade
KI Austausch
Oberfläche
*/

namespace Schach
{
    //Den Font Teil haben wir NICHT programmiert! Der ist von der fantastischen Microsoft Seite oder so...
    #region Font
    [StructLayout(LayoutKind.Sequential, Pack = 1)] //Dieses struct ConsoleFont, die class ConsoleHelper und das structLayout dingens hab ich kopiert, ich hab es benutzt, um die Schriftart zu verändern, da es nur auf 8x8 richtig funktioniert
    public struct ConsoleFont
    {
        public uint Index;
        public short SizeX, SizeY;
    }
    public static class ConsoleHelper
    {
        [DllImport("kernel32")]
        public static extern bool SetConsoleIcon(IntPtr hIcon);

        [DllImport("kernel32")]
        private extern static bool SetConsoleFont(IntPtr hOutput, uint index);

        private enum StdHandle
        {
            OutputHandle = -11
        }

        [DllImport("kernel32")]
        private static extern IntPtr GetStdHandle(StdHandle index);

        public static bool SetConsoleFont(uint index)
        {
            return SetConsoleFont(GetStdHandle(StdHandle.OutputHandle), index);
        }

        [DllImport("kernel32")]
        private static extern bool GetConsoleFontInfo(IntPtr hOutput, [MarshalAs(UnmanagedType.Bool)]bool bMaximize,
            uint count, [MarshalAs(UnmanagedType.LPArray), Out] ConsoleFont[] fonts);

        [DllImport("kernel32")]
        private static extern uint GetNumberOfConsoleFonts();

        public static uint ConsoleFontsCount
        {
            get
            {
                return GetNumberOfConsoleFonts();
            }
        }

        public static ConsoleFont[] ConsoleFonts
        {
            get
            {
                ConsoleFont[] fonts = new ConsoleFont[GetNumberOfConsoleFonts()];
                if (fonts.Length > 0)
                    GetConsoleFontInfo(GetStdHandle(StdHandle.OutputHandle), false, (uint)fonts.Length, fonts);
                return fonts;
            }
        }

    }
    #endregion


    class Program
    {
        #region Variablen
        public static byte schriftgröße;
        public static bool[] rochadem; //Kurze Rochade weiß, lange Rochade weiß, kurze Rochade schwarz, lange Rochade schwarz
        public static int züge;
        public static bool weiß; //Ist weiß am Zug?
        public static int[] verschiebung; //Um wie viel ist das Spielfeld nach rechts / unten verschoben?
        public static int[] kingposw;
        public static int[] kingposb;
        public static Random rnd;
        /*
        Weiß:
        Bauern  -> 1
        Türme   -> 2
        Pferd   -> 3
        Läufer  -> 4
        Dame    -> 5
        König   -> 6

        Schwarz:
        Bauern  -> 7
        Türme   -> 8
        Pferd   -> 9
        Läufer  -> 10
        Dame    -> 11
        König   -> 12
        */
        public static byte[,] Feld;
        public static byte[,,,] specialfelder;
        //public static int[,,,] possis = new int[2, 1000000, 8, 8];
        public static char[] symbols;
        public static bool z;
        public static List<int> Figuren;
        public static bool spielende;
        public static int sizex, sizey;
        public static int[] Werte;
        public static int[,] differences;
        public static byte menupoint;
        public static string[] menupoints;

        public readonly static byte schriftgrößestart = 0;
        public readonly static bool[] rochademstart = new bool[4] { true, true, true, true }; //Kurze Rochade weiß, lange Rochade weiß, kurze Rochade schwarz, lange Rochade schwarz
        public readonly static int zügestart = 0;
        public readonly static bool weißstart; //Ist weiß am Zug?
        public readonly static int[] verschiebungstart = new int[2] { 2, 2 }; //Um wie viel ist das Spielfeld nach rechts / unten verschoben?
        public readonly static int[] kingposwstart = new int[2];
        public readonly static int[] kingposbstart = new int[2];
        public readonly static Random rndstart = new Random();
        public readonly static byte[,] Feldstart = new byte[8, 8] //Das Feld mit den passenden Nummern (s.o.)
        {
            {8 ,9 ,10,11,12,10,9 ,8 },
            {7 ,7 ,7 ,7 ,7 ,7 ,7 ,7},
            {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0},
            {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0},
            {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0},
            {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0},
            {1 ,1 ,1 ,1 ,1 ,1 ,1 ,1},
            {2 ,3 ,4 ,5 ,6 ,4 ,3 ,2},
        };
        public readonly static byte[,,,] specialfelderstart = new byte[4, 2, 8, 8] //Das Feld mit den passenden Nummern (s.o.)
        {
            {
                {
                    {8 ,9 ,10,11,12,10,9 ,8 },
                    {7 ,7 ,7 ,7 ,7 ,7 ,7 ,7 },
                    {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
                    {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
                    {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
                    {0 ,0 ,0 ,0 ,1 ,0 ,0 ,0 },
                    {1 ,1 ,1 ,1 ,0 ,1 ,1 ,1 },
                    {2 ,3 ,4 ,5 ,6 ,4 ,3 ,2 },
                },
                {
                    {8 ,9 ,10,11,12,10,9 ,8 },
                    {7 ,7 ,7 ,0 ,7 ,7 ,7 ,7 },
                    {0 ,0 ,0 ,7 ,0 ,0 ,0 ,0 },
                    {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
                    {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
                    {0 ,0 ,0 ,0 ,1 ,0 ,0 ,0 },
                    {1 ,1 ,1 ,1 ,0 ,1 ,1 ,1 },
                    {2 ,3 ,4 ,5 ,6 ,4 ,3 ,2 },
                }
            },
            {
                {
                    {8 ,9 ,10,11,12,10,9 ,8 },
                    {7 ,7 ,7 ,7 ,7 ,7 ,7 ,7 },
                    {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
                    {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
                    {0 ,0 ,0 ,0 ,1 ,0 ,0 ,0 },
                    {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
                    {1 ,1 ,1 ,1 ,0 ,1 ,1 ,1 },
                    {2 ,3 ,4 ,5 ,6 ,4 ,3 ,2 },
                },
                {
                    {8 ,9 ,10,11,12,10,9 ,8 },
                    {7 ,7 ,7 ,0 ,7 ,7 ,7 ,7 },
                    {0 ,0 ,0 ,7 ,0 ,0 ,0 ,0 },
                    {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
                    {0 ,0 ,0 ,0 ,1 ,0 ,0 ,0 },
                    {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
                    {1 ,1 ,1 ,1 ,0 ,1 ,1 ,1 },
                    {2 ,3 ,4 ,5 ,6 ,4 ,3 ,2 },
                }
            },
            {
                {
                    {8 ,9 ,10,11,12,10,9 ,8 },
                    {7 ,7 ,7 ,7 ,7 ,7 ,7 ,7 },
                    {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
                    {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
                    {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
                    {0 ,0 ,0 ,0 ,0 ,0 ,0 ,3 },
                    {1 ,1 ,1 ,1 ,1 ,1 ,1 ,1 },
                    {2 ,3 ,4 ,5 ,6 ,4 ,0 ,2 },
                },
                {
                    {8 ,9 ,10,11,12,10,9 ,8 },
                    {7 ,7 ,7 ,0 ,7 ,7 ,7 ,7 },
                    {0 ,0 ,0 ,7 ,0 ,0 ,0 ,0 },
                    {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
                    {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
                    {0 ,0 ,0 ,0 ,0 ,0 ,0 ,3 },
                    {1 ,1 ,1 ,1 ,1 ,1 ,1 ,1 },
                    {2 ,3 ,4 ,5 ,6 ,4 ,0 ,2 },
                }
            },
            {
                {
                    {8 ,9 ,10,11,12,10,9 ,8 },
                    {7 ,7 ,7 ,7 ,7 ,7 ,7 ,7 },
                    {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
                    {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
                    {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
                    {0 ,0 ,0 ,0 ,0 ,3 ,0 ,0 },
                    {1 ,1 ,1 ,1 ,1 ,1 ,1 ,1 },
                    {2 ,3 ,4 ,5 ,6 ,4 ,0 ,2 },
                },
                {
                    {8 ,9 ,10,11,12,10,9 ,8 },
                    {7 ,7 ,7 ,0 ,7 ,7 ,7 ,7 },
                    {0 ,0 ,0 ,7 ,0 ,0 ,0 ,0 },
                    {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
                    {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
                    {0 ,0 ,0 ,0 ,0 ,3 ,0 ,0 },
                    {1 ,1 ,1 ,1 ,1 ,1 ,1 ,1 },
                    {2 ,3 ,4 ,5 ,6 ,4 ,0 ,2 },
                }
            }
        };
        //public static int[,,,] possis = new int[2, 1000000, 8, 8];
        public readonly static char[] symbolsstart = new char[13] //Die entsprechenden Symbole für die Figuren
            { ' ', 'B', 'T', 'S', 'L', 'D', 'K', 'B', 'T', 'S', 'L', 'D', 'K' };
        public readonly static bool zstart = true;
        public readonly static List<int> Figurenstart = new List<int>();
        public readonly static bool spielendestart = false;
        public readonly static int sizexstart = Console.LargestWindowWidth, sizeystart = Console.LargestWindowHeight;
        public readonly static int[] Wertestart = new int[7] { 0, 1, 5, 3, 3, 20, 10000 };
        public readonly static int[,] differencesstart;
        public readonly static byte menupointstart = 0;
        public readonly static string[] menupointsstart = new string[3] { "Singleplayer", "Multiplayer", "Settings" };
        public readonly static ConsoleColor[] colors = (ConsoleColor[])ConsoleColor.GetValues(typeof(ConsoleColor));
        public static byte[,,] differencefelder = new byte[2, 8, 8];
        public static bool[,] enpassentpossible = new bool[2, 8];

        public static ConsoleColor background = ConsoleColor.White;
        #endregion

        #region voids
        public static void Main(string[] args)
        {
            start:
            preinit();
            menu();
            postinit();
            if (menupoint != 3)
            {
                Spiel();
                Console.ForegroundColor = background;
                Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.Black;
            }
            else
                Settings();
            goto start;
        }

        public static void Spiel()
        {
            do
            {
                zeichneSpieler();
                debugfiles();
                checkschachgedöns();
                kingposw = getKingpos(Feld, false);
                kingposb = getKingpos(Feld, true);
                Console.ForegroundColor = ConsoleColor.Black;
                zeichnegeschlageneFiguren();
                if (differences != null)
                    paintdifferences();
                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        differencefelder[0, y, x] = Feld[y, x];
                    }
                }
                if (!z)
                {
                    neueeingabe();
                }
                else
                {
                    if (menupoint == 1)
                    {
                        botzug();
                        z = zug();
                    }
                    else if (menupoint == 2) neueeingabe();
                    else break;
                }
                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        differencefelder[1, y, x] = Feld[y, x];
                    }
                }
                enpassent();
            } while (!won() && !spielende);
        }

        public static void enpassent()
        {
            bool isdifferent = false;
            for (int x = 0; x < 8 && !isdifferent; x++)
            {
                for (int y = 0; y < 8 && !isdifferent; y++)
                {
                    if(differencefelder[0,y,x] != differencefelder[1,y,x])
                    {
                        isdifferent = true;
                        break;
                    }
                }
            }
            for (int x = 0; x < 8; x++)
            {
                if (differencefelder[0, 1, x] == 7 && differencefelder[1, 1, x] == 0 && differencefelder[0, 3, x] == 0 && differencefelder[1, 3, x] == 7)
                    enpassentpossible[0, x] = true;
                else enpassentpossible[0, x] = false;
                if (differencefelder[0, 6, x] == 1 && differencefelder[1, 6, x] == 0 && differencefelder[0, 4, x] == 0 && differencefelder[1, 4, x] == 1)
                    enpassentpossible[1, x] = true;
                else enpassentpossible[1, x] = false;
            }
        }

        public static void Settings()
        {
            int y = initSettings();
            ConsoleKey input;
            do
            {
                Console.BackgroundColor = background;
                Console.ForegroundColor = ConsoleColor.Black;
                input = Console.ReadKey(true).Key;
                Console.SetCursorPosition(19, 10 + y);
                Console.Write(colors[y + 1]);
                if (input == ConsoleKey.UpArrow && y > 0) y--;
                else if (input == ConsoleKey.DownArrow && y < colors.Length - 2) y++;
                Console.SetCursorPosition(19, 10 + y);
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(colors[y + 1]);
                
            } while (input != ConsoleKey.Enter && input != ConsoleKey.Spacebar);
            background = colors[y + 1];
        }

        public static int initSettings()
        {
            Console.Clear();
            Console.SetCursorPosition(10, 10);
            Console.Write("Colour: ");
            int i = -1;
            int returner = 0;
            foreach(var color in colors)
            {
                if (i != -1)
                {
                    Console.SetCursorPosition(19, 10 + i);
                    if (color == Console.BackgroundColor)
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        returner = i;
                    }
                    Console.Write(color);
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = background;
                }
                i++;
            }
            return returner;
        }

        public static void menu()
        {
            initmenu();
            ConsoleKey input;
            byte y = 0;
            do
            {
                Console.BackgroundColor = background;
                Console.ForegroundColor = ConsoleColor.Black;
                input = Console.ReadKey(true).Key;
                Console.SetCursorPosition(10, 10 + y);
                Console.Write(menupoints[y]);
                if (input == ConsoleKey.DownArrow && y < menupoints.Length - 1) y++;
                else if (input == ConsoleKey.UpArrow && y > 0) y--;
                Console.SetCursorPosition(10, 10 + y);
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(menupoints[y]);
            } while(input != ConsoleKey.Enter && input != ConsoleKey.Spacebar);
            menupoint = Convert.ToByte(y + 1);
        }

        public static void initmenu()
        {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = background;
            Console.Clear();
            for (int i = 0; i < menupoints.Length; i++)
            {
                Console.SetCursorPosition(10, 10 + i);
                Console.Write(menupoints[i]);
            }
            Console.SetCursorPosition(10, 11 + menupoints.Length);
            Console.Write("BTW: If it's too small, click on the icon in the left top corner, \n          select properties -> Font -> Lucida Console -> 36!");
            Console.SetCursorPosition(10, 10);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(menupoints[0]);
        }

        public static void preinit()
        {
            schriftgröße = schriftgrößestart;
            rochadem = rochademstart;
            züge = zügestart;
            weiß = weißstart;
            verschiebung = verschiebungstart; 
            kingposw = kingposwstart;
            kingposb = kingposbstart;
            rnd = rndstart;
            Feld = Feldstart;
            specialfelder = specialfelderstart;
            symbols = symbolsstart;
            z = zstart;
            Figuren = Figurenstart;
            spielende = spielendestart;
            sizex = sizexstart;
            sizey = sizeystart;
            Werte = Wertestart;
            differences = differencesstart;
            menupoint = menupointstart;
            menupoints = menupointsstart;
            Figuren.Clear();

            var os = Environment.OSVersion;
            if (os.Version.Minor == 1) schriftgröße = 8;
            else schriftgröße = 10;
            ConsoleHelper.SetConsoleFont(schriftgröße);
            if (File.Exists("debug.txt")) File.Delete("debug.txt");
            sizex = Console.LargestWindowWidth;
            sizey = Console.LargestWindowHeight;
            Console.SetWindowSize(sizex, sizey);
            Console.SetBufferSize(sizex, sizey);
            Console.CursorVisible = false;
            Console.CursorSize = 1;
        }

        public static void postinit()
        {            
            Figurenliste();
            Console.BackgroundColor = background; //Die Standardfarben
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Clear();
            zeichneHowto();
            zeichneFeld();
            zeichneSpieler();
            Console.ForegroundColor = ConsoleColor.Black;
            z = zug();
            z = zug();
            kingposw = getKingpos(Feld, false);
            kingposb = getKingpos(Feld, true);
        }

        public static void debugfiles()
        {
            string[] Textalt;
            string[] aktuell = tostringarray(Feld);
            string[] Textneu;
            int pos = 0;

            if (File.Exists("debug.txt"))
            {
                Textalt = File.ReadAllLines("debug.txt");
                File.Delete("debug.txt");
                Textneu = new string[Textalt.Length + 10];
                while (pos < Textalt.Length && pos < Textneu.Length - 10)
                {
                    Textneu[pos] = Textalt[pos];
                    pos++;
                }
            }
            else Textneu = new string[10];
            for (int i = 0; i < 8; i++, pos++)
            {
                Textneu[pos] = aktuell[i];
            }
            pos++;
            Textneu[pos] = " ";
            pos++;
            File.WriteAllLines("debug.txt", Textneu);

            int a = Textneu.Length;
        }

        public static string[] tostringarray(byte[,] dasFeld)
        {
            string[] derstring = new string[dasFeld.GetLength(0)];
            for (int i = 0; i < dasFeld.GetLength(0); i++)
            {
                for (int u = 0; u < dasFeld.GetLength(1); u++)
                {
                    if (dasFeld[i, u] != 0) derstring[i] += symbols[dasFeld[i, u]];
                    else derstring[i] += "█▄";
                    if (dasFeld[i, u] < 7 && dasFeld[i, u] > 0) derstring[i] += "1";
                    else if (dasFeld[i, u] > 0) derstring[i] += "2";
                }
            }
            return derstring;
        }

        public static void checkschachgedöns()
        {
            byte possi = 0; //nix, schwarz Schach, weiß Schach, schwarz Schachmatt, weiß Schachmatt
            if (isschach(true, Feld)) possi = 1;
            if (isschach(false, Feld)) possi = 2;
            if (isschachmatt(true, Feld)) possi = 3;
            if (isschachmatt(false, Feld)) possi = 4;
            Console.SetCursorPosition(verschiebung[0], verschiebung[1] + 21);
            if (background != ConsoleColor.Red)
                Console.ForegroundColor = ConsoleColor.Red;
            else
                Console.ForegroundColor = ConsoleColor.DarkRed;
            switch (possi)
            {
                case 1:
                    Console.Write("Black is Check!");
                    break;
                case 2:
                    Console.Write("White is Check!");
                    break;
                case 3:
                    Console.Write("Black is Checkmate!");
                    spielende = true;
                    break;
                case 4:
                    Console.Write("White is Checkmate!");
                    spielende = true;
                    break;
                default:
                    Console.Write("                           ");
                    break;
            }
            Console.ForegroundColor = ConsoleColor.Black;
        }

        public static void zeichneHowto()
        {
            Console.SetCursorPosition(verschiebung[0], 16);
            int cw = Console.BufferWidth;
            string text = "CONSOLE CHESS: this game is basically quite simple to play. Just select the figure you want to move with the arrow keys and enter/space. Once you selected your figure, navigate to the destination field and press enter/space again. You can change the Font with W/S and cancel your Input with Esc/entf. Have fun! by Timo/Liam";
            string[] words;
            words = text.Split(' ');
            int l = 0;
            int lines = 16;
            for (int i = 0; i < words.Length; i++)
            {
                l += words[i].Length;
                if (l < 50)
                {
                    Console.Write(words[i] + " ");
                }
                else
                {
                    lines++;
                    Console.SetCursorPosition(verschiebung[0], lines);
                    l = 0;
                    i--;
                }
            }
        }

        public static bool isschach(bool schwarz, byte[,] dasFeld)
        {
            byte[,] alt = new byte[8, 8];
            byte[,] temp = new byte[8, 8];
            for (byte i = 0; i < 8; i++)
            {
                for (byte u = 0; u < 8; u++)
                {
                    temp[u, i] = dasFeld[u, i];
                    alt[u, i] = Feld[u, i];
                }
            }
            Feld = temp;
            bool alt2 = z;
            z = !schwarz;
            int x2, y2;
            bool isschach = false;
            for (byte x = 0; x < 8; x++)
            {
                for (byte y = 0; y < 8; y++)
                {
                    if (dasFeld[y, x] != 0 && (dasFeld[y, x] < 7 && schwarz || dasFeld[y, x] > 6 && !schwarz))
                    {
                        if (schwarz)
                        {
                            x2 = kingposb[0];
                            y2 = kingposb[1];
                        }
                        else
                        {
                            x2 = kingposw[0];
                            y2 = kingposw[1];
                        }
                        if ((dasFeld[y2, x2] == 6 && !schwarz || dasFeld[y2, x2] == 12 && schwarz) && allowed1(dasFeld[y, x], x, x2, y, y2, true) && nichtdazwischen(dasFeld[y, x], x, x2, y, y2))
                        {
                            isschach = true;
                            Feld = alt;
                            z = alt2;
                            return isschach;
                        }
                    }
                }
            }
            Feld = alt;
            z = alt2;
            return isschach;
        }

        public static bool isschachmatt(bool schwarz, byte[,] dasFeld)
        {
            byte[,] alt = new byte[8, 8];
            byte[,] temp = new byte[8, 8];
            for (byte i = 0; i < 8; i++)
            {
                for (byte u = 0; u < 8; u++)
                {
                    temp[u, i] = dasFeld[u, i];
                    alt[u, i] = Feld[u, i];
                }
            }
            Feld = temp;
            bool alt2 = z;
            z = schwarz;
            bool isschachmatt = true;
            if (!isschach(schwarz, dasFeld))
            {
                z = alt2;
                Feld = alt;
                return false; ;
            }
            z = schwarz;
            for (byte x = 0; x < 8; x++)
            {
                for (byte y = 0; y < 8; y++)
                {
                    if (schwarz && dasFeld[y, x] > 6 || !schwarz && dasFeld[y, x] < 7 && dasFeld[y, x] != 0)
                    {
                        for (byte x2 = 0; x2 < 8; x2++)
                        {
                            for (byte y2 = 0; y2 < 8; y2++)
                            {
                                if ((allowed1(dasFeld[y, x], x, x2, y, y2, false) || allowed1(dasFeld[y, x], x, x2, y, y2, true) && nichtdazwischen(dasFeld[y, x], x, x2, y, y2)))
                                {
                                    int[] altkingposb = new int[2], altkingposw = new int[2];
                                    for (byte i = 0; i < 2; i++)
                                    {
                                        altkingposb[i] = kingposb[i];
                                        altkingposw[i] = kingposw[i];
                                    }
                                    if (dasFeld[y, x] == 6)
                                    {
                                        kingposw[0] = x2;
                                        kingposw[1] = y2;
                                    }
                                    else if (dasFeld[y, x] == 12)
                                    {
                                        kingposb[0] = x2;
                                        kingposb[1] = y2;
                                    }
                                    temp[y, x] = 0;
                                    temp[y2, x2] = dasFeld[y, x];
                                    if (!isschach(schwarz, temp))
                                    {
                                        kingposb = altkingposb;
                                        kingposw = altkingposw;
                                        z = alt2;
                                        Feld = alt;
                                        return false;
                                    }
                                    kingposb = altkingposb;
                                    kingposw = altkingposw;
                                    for (byte i = 0; i < 8; i++)
                                    {
                                        for (byte u = 0; u < 8; u++)
                                        {
                                            temp[u, i] = dasFeld[u, i];
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Feld = alt;
            z = alt2;
            return isschachmatt;
        }

        public static void alteeingabe()
        {
            string input;
            Console.SetCursorPosition(verschiebung[1], verschiebung[0] + 11); //Der Spieler macht seine Eingabe
            input = Console.ReadLine();
            if (verarbeite(input)) z = zug();   //Die wiederum verarbeitet wird
        }

        public static void neueeingabe()
        {
            if (spielende) return;
            ConsoleKey cki = new ConsoleKey();
            byte posx = 3;
            byte posy = 3;
            byte startx = 0, starty = 0, endx = 0, endy = 0;
            bool fertig = false;
            bool ausgewählt = false;
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.SetCursorPosition(verschiebung[0] + posx, verschiebung[1] + posy);
            if (Feld[posy, posx] > 6) Console.ForegroundColor = ConsoleColor.Black;
            else Console.ForegroundColor = ConsoleColor.White;
            Console.Write(symbols[Feld[posy, posx]]);
            while (!fertig && !spielende)
            {
                cki = Console.ReadKey(true).Key;
                if (Feld[posy, posx] > 6) Console.ForegroundColor = ConsoleColor.Black;
                else Console.ForegroundColor = ConsoleColor.White;
                zeichnesymbol(symbols[Feld[posy, posx]], Convert.ToInt32(posx), Convert.ToInt32(posy));
                Console.BackgroundColor = ConsoleColor.Blue;
                if (cki == ConsoleKey.RightArrow && posx < 7) posx++;
                else if (cki == ConsoleKey.LeftArrow && posx > 0) posx--;
                else if (cki == ConsoleKey.UpArrow && posy > 0) posy--;
                else if (cki == ConsoleKey.DownArrow && posy < 7) posy++;
                else if (cki == ConsoleKey.W && schriftgröße < 10)
                {
                    schriftgröße++;
                    ConsoleHelper.SetConsoleFont(schriftgröße);
                }
                else if (cki == ConsoleKey.S && schriftgröße > 0)
                {
                    schriftgröße--;
                    ConsoleHelper.SetConsoleFont(schriftgröße);
                }
                else if (cki == ConsoleKey.Enter || cki == ConsoleKey.Spacebar)
                {
                    if (!ausgewählt && Feld[posy, posx] > 0 && (Feld[posy, posx] < 7 && !z || Feld[posy,posx] > 6 && z))
                    {
                        startx = posx;
                        starty = posy;
                        startx++;
                        starty++;
                        ausgewählt = true;

                        zeichnepossis(Convert.ToByte(startx - 1), Convert.ToByte(starty - 1));
                        if(differences != null)
                            paintdifferences();
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.SetCursorPosition(verschiebung[0] + posx, verschiebung[1] + posy);
                        if (Feld[posy, posx] > 6) Console.ForegroundColor = ConsoleColor.Black;
                        else Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(symbols[Feld[posy, posx]]);
                        Console.BackgroundColor = ConsoleColor.Blue;
                        getnewcursorposition(posx, posy, out posx, out posy);
                    }
                    else if (ausgewählt)
                    {
                        endx = posx;
                        endy = posy;
                        endx++;
                        endy++;
                        fertig = true;
                    }
                }
                else if (cki == ConsoleKey.Escape || cki == ConsoleKey.Delete)
                {
                    startx = 0;
                    starty = 0;
                    ausgewählt = false;
                    zeichneSpieler();
                }
                if (ausgewählt)
                    zeichnepossis(Convert.ToByte(startx - 1), Convert.ToByte(starty - 1));
                if (differences != null)
                    paintdifferences();
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.SetCursorPosition(verschiebung[0] + posx, verschiebung[1] + posy);
                if (Feld[posy, posx] > 6) Console.ForegroundColor = ConsoleColor.Black;
                else Console.ForegroundColor = ConsoleColor.White;
                Console.Write(symbols[Feld[posy, posx]]);
                if (ausgewählt)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.SetCursorPosition(verschiebung[0] + startx - 1, verschiebung[1] + starty - 1);
                    if (Feld[starty - 1, startx - 1] > 6) Console.ForegroundColor = ConsoleColor.Black;
                    else Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(symbols[Feld[starty - 1, startx - 1]]);
                    Console.BackgroundColor = ConsoleColor.Blue;
                }
            }
            Console.BackgroundColor = background;
            string input = Convert.ToString(convertToChar(startx)) + starty + " " + convertToChar(endx) + endy;
            if (Feld[endy - 1, endx - 1] != 0) input += "x";
            if ((Feld[starty - 1, startx - 1] == 6 || Feld[starty-1, startx-1] == 12) && (Feld[endy - 1, endx - 1] == 2 ||Feld[endy - 1, endx - 1] == 8))
            {
                input = "0-0";
                if (endx < 6) input += "-0";
            }
            if (verarbeite(input)) z = zug();
            else zeichneSpieler();
            if (differences != null)
                paintdifferences();
            while (Console.KeyAvailable) Console.ReadKey(true);
        }

        public static void zeichnepossis(byte posx, byte posy)
        {
            bool[,] feldmöglich = new bool[8, 8];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        bool schlagen = k == 0;
                        if (allowed(Feld[posy, posx], posx, i, posy, j, schlagen) || (enpassentpossible[0, i] && j == 2 && !z && Feld[posy, posx] == 1 && (i == posx + 1 || i == posx - 1) && posy == 3) || (enpassentpossible[1, i] && j == 5 && z && Feld[posy, posx] == 7 && (i == posx + 1 || i == posx - 1) && posy == 4))
                        {
                            feldmöglich[j, i] = true;
                        }
                        if (rochadepossible(1) && Feld[posy, posx] == 6 && !z)
                            feldmöglich[7, 7] = true;
                        if (rochadepossible(2) && Feld[posy, posx] == 6 && !z)
                            feldmöglich[7, 0] = true;
                        if (rochadepossible(3) && Feld[posy, posx] == 12 && z)
                            feldmöglich[0, 7] = true;
                        if (rochadepossible(4) && Feld[posy, posx] == 12 && z)
                            feldmöglich[0, 0] = true;
                    }
                }
            }
            Console.BackgroundColor = ConsoleColor.Magenta;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (feldmöglich[y, x])
                    {
                        Console.SetCursorPosition(verschiebung[0] + x, verschiebung[1] + y);
                        if (Feld[y, x] > 6) Console.ForegroundColor = ConsoleColor.Black;
                        else Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(symbols[Feld[y, x]]);
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                }
            }
        }

        public static void getnewcursorposition(byte x, byte y, out byte x2, out byte y2)
        {
            if (Feld[y, x] == 1 && y > 0) y--;
            else if (Feld[y, x] == 2 || Feld[y, x] == 5 || Feld[y, x] == 6)
            {
                if (x > 0) x--;
                else if (y > 0) y--;
                else x++;
            }
            else if (Feld[y, x] == 3)
            {
                if (x > 0) x--;
                else x++;
                if (y - 1 > 0) y -= 2;
                else y += 2;
            }
            else if (Feld[y, x] == 4)
            {
                if (y > 0) y--;
                else y++;
                if (x > 0) x--;
                else x++;
            }
            else if (Feld[y, x] == 7 && y < 7) y++;
            else if (Feld[y, x] == 8 || Feld[y, x] == 11 || Feld[y, x] == 12)
            {
                if (x > 0) x--;
                else if (y > 0) y--;
                else x++;
            }
            else if (Feld[y, x] == 9)
            {
                if (x > 0) x--;
                else x++;
                if (y - 1 > 0) y -= 2;
                else y += 2;
            }
            else if (Feld[y, x] == 10)
            {
                if (y > 0) y--;
                else y++;
                if (x > 0) x--;
                else x++;
            }
            x2 = x;
            y2 = y;
        }

        public static void Figurenliste()
        {
            for (byte x = 0; x < 8; x++)
            {
                for (byte y = 0; y < 8; y++)
                {
                    if (Feld[y, x] != 0) Figuren.Add(Feld[y, x]);
                }
            }
        }

        public static void zeichnegeschlageneFiguren()
        {
            int[] geschlageneFiguren = getgeschlageneFiguren();
            int poss = 0, posw = 0;
            for (byte i = 0; i < geschlageneFiguren.Length; i++)
            {
                if (geschlageneFiguren[i] == 0) break;
                else if (geschlageneFiguren[i] < 7)
                {
                    Console.SetCursorPosition(9 + posw + verschiebung[0], 7 + verschiebung[1]);
                    posw += 2;
                }
                else if (geschlageneFiguren[i] > 6)
                {
                    Console.SetCursorPosition(9 + poss + verschiebung[0], verschiebung[1]);
                    poss += 2;
                }
                else break;
                Console.Write(symbols[geschlageneFiguren[i]]);
            }
        }

        public static int[] getgeschlageneFiguren()
        {
            int[] geschlageneFiguren;
            List<int> fehlendeFiguren = Figuren.ToArray().ToList();
            for (byte x = 0; x < 8; x++)
            {
                for (byte y = 0; y < 8; y++)
                {
                    if (fehlendeFiguren.Contains(Feld[y, x]))
                        fehlendeFiguren.Remove(Feld[y, x]);
                }
            }
            geschlageneFiguren = fehlendeFiguren.ToArray();
            return geschlageneFiguren;
        }

        public static bool won()//checks if so
        {
            byte könige = 0;
            for (byte i = 0; i < 8; i++)
            {
                for (byte j = 0; j < 8; j++)
                {
                    if (Feld[i, j] == 6 || Feld[i, j] == 12)
                        könige++;
                }
            }
            if (könige == 2) return false;
            gewonnen();
            return true;
        }

        public static List<byte> tolist(byte[,] array)//konvertiert ein Feld Array in eine entsprechende liste
        {
            List<byte> Liste = new List<byte>();
            for (byte u = 0; u < array.GetLength(1); u++)
            {
                for (byte i = 0; i < array.GetLength(0); i++)
                {
                    Liste.Add(array[i, u]);
                }
            }
            return Liste;
        }

        public static void botzug()
        {
            Console.SetCursorPosition(20, 5);
            Console.Write("Thinking...");
            byte[,] temp = new byte[8, 8]; //Das zu untersuchende Feld
            byte[,] tempa = new byte[8, 8]; //Der erste Zug (Wird auch untersucht)
            long bew = 0; //Der Zug
            byte[,,] eins;
            byte[,,,] zwei, drei;
            possibilities(out eins, out zwei, out drei); //Die Möglichkeiten(Nach dem dritten Zug)
            long[,] besterzugqwelcherzug = new long[3, 10]; //Der beste Zug
            int pos = 1;


            for (int i = 0; i < eins.GetLength(0); i++)
            {
                for (byte a = 0; a < 8; a++)
                {
                    for (byte s = 0; s < 8; s++)
                    {
                        temp[s, a] = eins[i, s, a]; //Das zu untersuchende Feld wird definiert
                    }
                }
                if (!feldnichtleer(temp)) break;
                bew = bewerte(temp, 1); //Wenn alles erlaubt ist, wird bew damit definiert
                z = true;
                if (checkWon(temp)) bew = 1000000000;
                if (bew > besterzugqwelcherzug[0, 0] || besterzugqwelcherzug[2, 0] <= 0 && bew > 0 || besterzugqwelcherzug[2, 0] > 3 || besterzugqwelcherzug[2, 0] == 0 || besterzugqwelcherzug[2, 0] == 0)
                {
                    besterzugqwelcherzug[0, 0] = bew; //Und gegebenenfalls wird der bestezug aktualisiert
                    besterzugqwelcherzug[1, 0] = i; //Und i in q gespeichert
                    besterzugqwelcherzug[2, 0] = 1;
                    pos = 1;
                }
                else if (bew == besterzugqwelcherzug[0, 0] && pos < 9)
                {
                    besterzugqwelcherzug[0, pos] = bew; //Und gegebenenfalls wird der bestezug aktualisiert
                    besterzugqwelcherzug[1, pos] = i; //Und i in q gespeichert
                    besterzugqwelcherzug[2, pos] = 1;
                    pos++;
                }
            }

            for (int i = 0; i < zwei.GetLength(1); i++)
            {
                for (byte a = 0; a < 8; a++)
                {
                    for (byte s = 0; s < 8; s++)
                    {
                        temp[s, a] = zwei[0, i, s, a]; //Das zu untersuchende Feld wird definiert
                    }
                }
                for (byte a = 0; a < 8; a++)
                {
                    for (byte s = 0; s < 8; s++)
                    {
                        tempa[s, a] = zwei[1, i, s, a]; //Das zu untersuchende Feld wird definiert
                    }
                }
                if (!feldnichtleer(temp)) break;
                if (!feldnichtleer(tempa)) break;
                bew = (bewerte(temp, 2) + bewerte(tempa, 1)) / 2 - 10; //Wenn alles erlaubt ist, wird bew damit definiert
                z = true;
                if (checkWon(tempa)) bew = 1000000000;
                if (bew > besterzugqwelcherzug[0, 0] || besterzugqwelcherzug[2, 0] <= 0 && bew > 0 || besterzugqwelcherzug[2, 0] > 3 || besterzugqwelcherzug[2, 0] == 0)
                {
                    besterzugqwelcherzug[0, 0] = bew; //Und gegebenenfalls wird der bestezug aktualisiert
                    besterzugqwelcherzug[1, 0] = i; //Und i in q gespeichert
                    besterzugqwelcherzug[2, 0] = 2;
                    pos = 1;
                }
                else if (bew == besterzugqwelcherzug[0, 0] && pos < 9)
                {
                    besterzugqwelcherzug[0, pos] = bew; //Und gegebenenfalls wird der bestezug aktualisiert
                    besterzugqwelcherzug[1, pos] = i; //Und i in q gespeichert
                    besterzugqwelcherzug[2, pos] = 2;
                    pos++;
                }
            }

            for (int i = 0; i < drei.GetLength(1); i++)
            {
                for (byte a = 0; a < 8; a++)
                {
                    for (byte s = 0; s < 8; s++)
                    {
                        temp[s, a] = drei[0, i, s, a]; //Das zu untersuchende Feld wird definiert
                    }
                }
                for (byte a = 0; a < 8; a++)
                {
                    for (byte s = 0; s < 8; s++)
                    {
                        tempa[s, a] = drei[1, i, s, a]; //Das zu untersuchende Feld wird definiert
                    }
                }
                if (!feldnichtleer(temp)) break;
                if (!feldnichtleer(tempa)) break;
                bew = (bewerte(temp, 3) + bewerte(tempa, 1)) / 2 - 100; //Wenn alles erlaubt ist, wird bew damit definiert
                z = true;
                if (checkWon(tempa)) bew = 1000000000;
                if (bew > besterzugqwelcherzug[0, 0] || besterzugqwelcherzug[2, 0] <= 0 && bew > 0 || besterzugqwelcherzug[2, 0] > 3 || besterzugqwelcherzug[2, 0] == 0)
                {
                    besterzugqwelcherzug[0, 0] = bew; //Und gegebenenfalls wird der bestezug aktualisiert
                    besterzugqwelcherzug[1, 0] = i; //Und i in q gespeichert
                    besterzugqwelcherzug[2, 0] = 1;
                    pos = 1;
                }
                else if (bew == besterzugqwelcherzug[0, 0] && pos < 9)
                {
                    besterzugqwelcherzug[0, pos] = bew; //Und gegebenenfalls wird der bestezug aktualisiert
                    besterzugqwelcherzug[1, pos] = i; //Und i in q gespeichert
                    besterzugqwelcherzug[2, pos] = 3;
                    pos++;
                }
            }
            int r = rnd.Next(0, pos-1);
            byte[,] next = new byte[8, 8];
            bool catchederror = false;
            errorcatcher:
            for (byte x = 0; x < 8; x++)
            {
                for (byte y = 0; y < 8; y++)
                {
                    if (besterzugqwelcherzug[2, r] == 1)
                    {
                        next[y, x] = eins[besterzugqwelcherzug[1, r], y, x]; //Wenn alle Möglichkeiten überprüft wurden, wird das Feld mit dem besten Zug aktualisiert
                    }
                    else if (besterzugqwelcherzug[2, r] == 2)
                    {
                        next[y, x] = zwei[1, besterzugqwelcherzug[1, r], y, x];
                    }
                    else if (besterzugqwelcherzug[2, r] == 3)
                    {
                        next[y, x] = drei[1, besterzugqwelcherzug[1, r], y, x];
                    }
                    else if (!catchederror)
                    {
                        r = 0;
                        catchederror = true;
                        goto errorcatcher;
                    }
                    else
                    {
                        Error();
                    }
                }
            }

            bool schäferzug = true;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (Feld[y, x] != specialfelder[0, 0, y, x])
                        schäferzug = false;
                }
            }
            bool schäferzug2 = true;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (Feld[y, x] != specialfelder[1, 0, y, x])
                        schäferzug2 = false;
                }
            }
            bool schäferzug3 = true;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (Feld[y, x] != specialfelder[2, 0, y, x])
                        schäferzug3 = false;
                }
            }
            bool schäferzug4 = true;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (Feld[y, x] != specialfelder[3, 0, y, x])
                        schäferzug4 = false;
                }
            }

            differences = getdifferences(Feld, next);
            for (byte x = 0; x < 8; x++)
            {
                for (byte y = 0; y < 8; y++)
                {
                    if (schäferzug)
                    {
                        next[y, x] = specialfelder[0, 1, y, x];
                        differences = getdifferences(Feld, next);
                    }
                    else if (schäferzug2)
                    {
                        next[y, x] = specialfelder[1, 1, y, x];
                        differences = getdifferences(Feld, next);
                    }
                    else if (schäferzug3)
                    {
                        next[y, x] = specialfelder[2, 1, y, x];
                        differences = getdifferences(Feld, next);
                    }
                    else if (schäferzug4)
                    {
                        next[y, x] = specialfelder[3, 1, y, x];
                        differences = getdifferences(Feld, next);
                    }
                    if (!feldnichtleer(next)) break;
                    Feld[y, x] = next[y, x];
                }
            };
            for (byte i = 0; i < 8; i++)
            {
                if (Feld[7, i] == 7)
                {
                    Feld[7, i] = 11;
                }
            }
            ;
            long b = bewerte(Feld, 1);
            zeichneSpieler(); //und in gezeichnet
            Console.BackgroundColor = ConsoleColor.Green;
            paintdifferences();
            Console.BackgroundColor = background;

            /*Console.ForegroundColor = ConsoleColor.Black; Console.SetCursorPosition(10, 0); Console.Write(bew.ToString());//Das ist nur zum Bugfixing
            Console.SetCursorPosition(20, 20); Console.Write(bewerte(Feld));
            Console.SetCursorPosition(40, 40);Console.Write(Gegnerpossis(Feld))
            for (int i = 0; i < 8; i++)
            {
                for (int u = 0; u < 8; u++)
                {
                    Console.SetCursorPosition(2 * i + 15, u);
                    if (besterzugqwelcherzug[2, r] == 1)
                        Console.Write(eins[besterzugqwelcherzug[1, r], u, i]);
                    else if (besterzugqwelcherzug[2, r] == 2)
                        Console.Write(zwei[0, besterzugqwelcherzug[1, r], u, i]);
                    else if (besterzugqwelcherzug[2, r] == 3)
                        Console.Write(drei[0, besterzugqwelcherzug[1, r], u, i]);
                    else Error();
                }
            }
            Console.Write(" " + besterzugqwelcherzug[2, r] + " " + besterzugqwelcherzug[1, r]);*/
            Console.SetCursorPosition(20, 5);
            Console.Write("           ");
        }

        public static void paintdifferences()
        {
            Console.BackgroundColor = ConsoleColor.Green;
            for (int i = 0; i < differences.GetLength(0) && differences[i, 0] != -1; i++)
            {
                Console.SetCursorPosition(differences[i, 0] + verschiebung[0], differences[i, 1] + verschiebung[1]);
                if (Feld[differences[i, 1], differences[i, 0]] > 6) Console.ForegroundColor = ConsoleColor.Black;
                else Console.ForegroundColor = ConsoleColor.White;
                Console.Write(symbols[Feld[differences[i, 1], differences[i, 0]]]);
            }
            Console.BackgroundColor = background;
            Console.ForegroundColor = ConsoleColor.Black;
        }

        public static bool feldnichtleer (byte[,] dasFeld)
        {
            List<byte> liste = tolist(dasFeld); //Und zur Liste konvertiert
            bool erlaubt = false;
            for (byte t = 1; t < liste.LastIndexOf(liste.Last()); t++)
            {
                if (liste.Contains(t)) erlaubt = true;//Überprüft nur die Felder, in denen nicht nur Nullen stehen
            }
            return erlaubt;
        }

        public static int[,] getdifferences(byte[,] Feld1, byte[,] Feld2)
        {
            try
            {
                int[,] differences = new int[4, 2] { { -1, -1 }, { -1, -1 }, { -1, -1 }, { -1, -1 } };
                int pos = 0;

                for (byte x = 0; x < 8; x++)
                {
                    for (byte y = 0; y < 8; y++)
                    {
                        if (Feld1[y, x] != Feld2[y, x] && pos < differences.GetLength(0))
                        {
                            differences[pos, 0] = x;
                            differences[pos, 1] = y;
                            pos++;
                        }
                    }
                }

                return differences;
            }
            catch
            {
                int[,] differences = new int[4, 2] { { -1, -1 }, { -1, -1 }, { -1, -1 }, { -1, -1 } };
                return differences;
            }
        }

        public static void testfeld(int[,,] possis) //nur zu Testzwecken - zeichnet alle Felder des Arrays
        {
            Console.Clear();
            int line = 0;
            for (int i = 0; i < 1000000; i++)
            {
                for (byte u = 0; u < 8; u++)
                {
                    for (byte z = 0; z < 8; z++)
                    {
                        int x = 0;
                        int y = 0;
                        x = i * 9 + u - line * (Console.BufferWidth - 1);
                        y = z + line * 9;
                        if (x > Console.BufferWidth)
                        {
                            x = 0;
                            line += 1;
                            y = z + line * 9;
                        }
                        if (y < Console.BufferHeight && x < Console.BufferWidth)
                        {
                            Console.SetCursorPosition(x, y);
                            char symbol = symbols[possis[i, z, u]];
                            if (symbol == ' ') symbol = '█';
                            Console.Write(symbol);
                        }
                    }
                }
                Console.ReadKey();
            }
            Console.ReadLine();
        }

        public static void possibilities(out byte[,,] eins, out byte[,,,] zwei, out byte[,,,] drei)
        {
            byte[,] now = new byte[8, 8];
            for (int i = 0; i < Feld.GetLength(0); i++)
            {
                for (int j = 0; j < Feld.GetLength(1); j++)
                {
                    now[i, j] = Feld[i, j];
                }
            }
            eins = new byte[150, 8, 8]; //Nach dem ersten Zug
            zwei = new byte[2, 20000, 8, 8]; //Nach dem zweiten Zug
            drei = new byte[2, 2000000, 8, 8]; //Nach dem dritten Zug
            byte[,,] temp = null; //Ein Temporäres Feld - nur zur Vereinfachung
            int pos = 0; //Die Position im derzeitigen Array
            for (byte x = 0; x < 8; x++)//erster zug
            {
                for (byte y = 0; y < 8; y++)
                {
                    if (Feld[y, x] > 6)
                    {
                        temp = getpossisofthis(Feld[y, x], x, y); //Die derzeitige Möglichkeit
                        if (temp.GetLength(0) > 0)
                        {
                            for (int i = 0; i < temp.GetLength(0) && i + pos < eins.GetLength(0); i++)
                            {
                                for (byte u = 0; u < 8; u++)
                                {
                                    for (byte s = 0; s < 8; s++)
                                    {
                                        eins[i + pos, s, u] = temp[i, s, u]; //Und ab ins ARRAY
                                    }
                                }
                            }
                            pos += temp.GetLength(0); //Und die Position im Array wird um eins erhöht
                        }
                    }
                }
            }
            pos = 0;//zweiter Zug -> gleich wie bei eins
            for (int h = 0; h < eins.GetLength(0); h++)
            {
                for (byte i = 0; i < 8; i++)
                {
                    for (byte u = 0; u < 8; u++)
                    {
                        Feld[u, i] = eins[h, u, i];
                    }
                }
                for (byte x = 0; x < 8; x++)
                {
                    for (byte y = 0; y < 8; y++)
                    {
                        if (Feld[y, x] > 6)
                        {
                            temp = getpossisofthis(Feld[y, x], x, y);
                            if (temp.GetLength(0) > 0)
                            {
                                for (int i = 0; i < temp.GetLength(0) && i + pos < zwei.GetLength(1); i++)
                                {
                                    for (byte u = 0; u < 8; u++)
                                    {
                                        for (byte s = 0; s < 8; s++)
                                        {
                                            zwei[0, i + pos, s, u] = temp[i, s, u];
                                            for (byte b = 0; b < 8; b++)
                                            {
                                                for (byte n = 0; n < 8; n++)
                                                {
                                                    zwei[1, i + pos, n, b] = eins[h, n, b];
                                                }
                                            }
                                        }
                                    }
                                }
                                pos += temp.GetLength(0);
                            }
                        }
                    }
                }
            }
            pos = 0;//Dritter zug
            for (int h = 0; h < zwei.GetLength(0); h++)
            {
                for (byte i = 0; i < 8; i++)
                {
                    for (byte u = 0; u < 8; u++)
                    {
                        Feld[u, i] = zwei[0, h, u, i];
                    }
                }
                for (byte x = 0; x < 8; x++)
                {
                    for (byte y = 0; y < 8; y++)
                    {
                        if (Feld[y, x] > 6)
                        {
                            temp = getpossisofthis(Feld[y, x], x, y);
                            if (temp.GetLength(0) > 0)
                            {
                                for (int i = 0; i < temp.GetLength(0); i++)
                                {
                                    for (byte u = 0; u < 8; u++)
                                    {
                                        for (byte s = 0; s < 8; s++)
                                        {
                                            drei[0, i + pos, s, u] = temp[i, s, u];
                                            for (byte b = 0; b < 8; b++)
                                            {
                                                for (byte n = 0; n < 8; n++)
                                                {
                                                    drei[1, i + pos, n, b] = eins[h, n, b];
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            //if (pos < drei.GetLength(1) + temp.GetLength(0))
                            pos += temp.GetLength(0);
                            //else
                            //  break;
                        }
                    }
                }
            }
            for (int i = 0; i < now.GetLength(0); i++)
            {
                for (int j = 0; j < now.GetLength(1); j++)
                {
                    Feld[i, j] = now[i, j];
                }
            }
        }

        public static byte[,,] getpossisofthis(byte pre, int x, int y)
        {
            byte[,,] dat = new byte[100, 8, 8]; //DatPossis
            int pos = 0; //Wie immer die Position im Array
            for (byte i = 0; i < 8; i++)
            {
                for (byte u = 0; u < 8; u++)
                {
                    if ((allowed(pre, x, i, y, u, false) || allowed(pre, x, i, y, u, true))) //Ist diese Möglichkeit erlaubt
                    {
                        for (byte q = 0; q < 8; q++)
                        {
                            for (byte w = 0; w < 8; w++)
                            {
                                dat[pos, w, q] = Feld[w, q]; //Ich hoffe des untre hier ist selbsterklärend
                            }
                        }
                        dat[pos, u, i] = pre;
                        dat[pos, y, x] = 0;
                        pos++;
                    }
                }
            }
            byte[,,] temp = new byte[pos, 8, 8];
            for (int i = 0; i < pos; i++)
            {
                for (byte u = 0; u < 8; u++)
                {
                    for (byte s = 0; s < 8; s++)
                    {
                        temp[i, s, u] = dat[i, s, u];
                    }
                }
            }
            return temp;
        }

        public static bool nichtdazwischen(int previous, int xv, int xn, int yv, int yn)//Ist was dazwischen? Beinhaltet eben die ganzen Überprüfungen dafür
        {
            int dx = delta(xv, xn);
            int dy = delta(yv, yn);
            if (previous == 0 || dx == 0 && dy == 0) return false;
            if (previous == 5 || previous == 11)
            {
                if (dx == 0 || dy == 0) previous = 2;
                else if (dx == dy) previous = 4;
                else return false;
            }
            if (previous == 1 || previous == 7) previous = 2;
            if (previous == 2 || previous == 8)
            {
                if (dy == 0)
                    for (int i = 1; i < dx; i++)
                    {
                        if (xn > xv) { if (Feld[yv, xv + i] != 0) return false; }
                        else { if (Feld[yv, xv - i] != 0) return false; }
                    }
                else if (dx == 0)
                    for (int i = 1; i < dy; i++)
                    {
                        if (yn > yv) { if (Feld[yv + i, xv] != 0) return false; }
                        else { if (Feld[yv - i, xv] != 0) return false; }
                    }
            }
            else if (previous == 4 || previous == 10)
            {
                for (int i = 1; i < dy; i++)
                {
                    if (yn > yv)
                    {
                        if (xn > xv)
                        {
                            if (Feld[yv + i, xv + i] != 0) return false;
                        }
                        else
                        {
                            if (Feld[yv + i, xv - i] != 0) return false;
                        }
                    }
                    else
                    {
                        if (xn > xv)
                        {
                            if (Feld[yv - i, xv + i] != 0) return false;
                        }
                        else
                        {
                            if (Feld[yv - i, xv - i] != 0) return false;
                        }
                    }
                }
            }
            return true;
        }

        public static int delta(int a, int b) //Die DIFFERENZ
        {
            int c = a - b;
            if (c < 0) c = -c;
            return c;
        }


        public static bool allowed(int pre, int xv, int xn, int yv, int yn, bool schlagen) //Überprüfung ob Zug erlaubt ist
        {
            if (!allowed1(pre, xv, xn, yv, yn, schlagen) || !nichtdazwischen(pre, xv, xn, yv, yn) || !isschachpossible(pre, xv, xn, yv, yn)) return false;
            return true;
        }

        public static bool isschachpossible(int pre, int xv, int xn, int yv, int yn)
        {
            byte[,] temp = new byte[8, 8];
            for (int i = 0; i < 8; i++)
            {
                for (int u = 0; u < 8; u++)
                {
                    temp[u, i] = Feld[u, i];
                }
            }
            temp[yv, xv] = 0;
            temp[yn, xn] = Feld[yv, xv];
            int[] altkingpos = new int[2];
            if (!z)
            {
                altkingpos[0] = kingposw[0];
                altkingpos[1] = kingposw[1];
                if (Feld[yv, xv] == 6)
                {
                    kingposw[0] = xn;
                    kingposw[1] = yn;
                }
            }
            else
            {
                altkingpos[0] = kingposb[0];
                altkingpos[1] = kingposb[1];
                if (Feld[yv, xv] == 12)
                {
                    kingposb[0] = xn;
                    kingposb[1] = yn;
                }
            }

            if (isschach(z, temp))
            {
                if (z) kingposb = altkingpos;
                else kingposw = altkingpos;
                return false;
            }
            if (z) kingposb = altkingpos;
            else kingposw = altkingpos;
            return true;
        }

        public static bool allowed1(int pre, int xv, int xn, int yv, int yn, bool schlagen)
        {
            if (schlagen && Feld[yn, xn] == 0 || !schlagen && Feld[yn, xn] != 0) return false;
            if (!z && Feld[yn, xn] < 7 && Feld[yn, xn] > 0 || z && Feld[yn, xn] > 6) return false;
            int dy = delta(yv, yn);
            int dx = delta(xv, xn);
            if (pre == 0) return false; //keine Figur
            if (xv == xn && yv == yn) return false;  //Zielfeld = Endfeld

            if (pre == 1 && !schlagen)//Bauer
            {
                if (yn == yv - 1 && dx == 0) return true;
                else if (yv == 6 && yn == yv - 2 && dx == 0) return true;
                else return false;
            }
            else if (pre == 1 && schlagen)
            {
                if (yn == yv - 1 && dx == 1) return true;
                else return false;
            }
            else if (pre == 7 && !schlagen)
            {
                if (yn == yv + 1 && dx == 0) return true;
                else if (yv == 1 && yn == yv + 2 && dx == 0) return true;
                else return false;
            }
            else if (pre == 7 && schlagen)
            {
                if (yn == yv + 1 && dx == 1) return true;
                else return false;
            }
            else if (pre == 2 || pre == 8)//Turm
            {
                if (dx == 0 || dy == 0) return true;
                else return false;
            }
            else if (pre == 3 || pre == 9)//Springer
            {
                if (dx < 0) dx = -dx;
                if (dy < 0) dy = -dy;
                if (dx == 2 && dy == 1 || dy == 2 && dx == 1) return true;
                else return false;
            }
            else if (pre == 4 || pre == 10)//Läufer
            {
                if (dx == dy) return true;
                else return false;
            }
            else if (pre == 5 || pre == 11) //Dame
            {
                if (dx == dy || dx == 0 || dy == 0) return true;
                else return false;
            }
            else if (pre % 6 == 0)//König
            {
                if (dx <= 1 && dy <= 1) return true;
                else return false;
            }
            else return false;
        }

        public static bool verarbeite(string input) //Und zwar hier
        {
            try
            {
                byte rochade = 0; //weiß klein, weiß groß, schwarz klein, schwarz groß
                bool schlagen = false;
                if (input.ToUpper().Contains("X"))
                {
                    schlagen = true;
                    input.Remove(5, 1);
                }
                if (input == "0-0" && weiß)
                {
                    rochade = 1;
                }
                else if (input == "0-0-0" && weiß)
                {
                    rochade = 2;
                }
                else if (input == "0-0" && !weiß)
                {
                    rochade = 3;
                }
                else if (input == "0-0-0" && !weiß)
                {
                    rochade = 4;
                }
                if (rochade == 0)
                {
                    string[] splitted = input.Split(' '); //Die Eingabe sollte in der Form A1 B3 sein, wobei der erste Teil das Start- und der zweite Teil das Endfeld ist. Die Eingabe wird demenstsprechend nach Start- und Endfeld aufgesplitted1t
                    if (splitted.Length != 2) //Es sollten natürlich nur zwei Felder sein
                    {
                        Error();
                        return false;
                    }

                    char[] eins = splitted[0].ToCharArray();//Diese zwei Felder werden dann wieder zu x und y gesplittet
                    char[] zwei = splitted[1].ToCharArray();
                    int[] peins = new int[2] { convertToInt(eins[0]) - 1, Convert.ToInt32(eins[1] - '0') - 1 }; //Und der Buchstabe wird in eine equivalente Zahl umgewandelt
                    int[] pzwei = new int[2] { convertToInt(zwei[0]) - 1, Convert.ToInt32(zwei[1] - '0') - 1 };
                    byte previous; //Das ist die Figur, die bewegt wird
                    previous = Feld[peins[1], peins[0]];
                    byte enpassentnow = 0;
                    if (enpassentpossible[0, pzwei[0]] && pzwei[1] == 2 && !z && Feld[peins[1], peins[0]] == 1 && (pzwei[0] == peins[0] + 1 || pzwei[0] == peins[0] - 1) && peins[1] == 3)
                        enpassentnow = 1;
                    else if (enpassentpossible[1, pzwei[0]] && pzwei[1] == 5 && z && Feld[peins[1], peins[0]] == 7 && (pzwei[0] == peins[0] + 1 || pzwei[0] == peins[0] - 1) && peins[1] == 4)
                        enpassentnow = 2;


                    if ((Feld[pzwei[1], pzwei[0]] == 0 && !schlagen || Feld[pzwei[1], pzwei[0]] != 0 && schlagen) && (weiß && previous < 7 || !weiß && previous >= 7)/*Ist auch die passende Farbe am Zug?*/ && allowed(previous, peins[0], pzwei[0], peins[1], pzwei[1], schlagen))
                    {
                        rochadeaktualisieren(previous, peins[0]);
                        Feld[peins[1], peins[0]] = 0; //Die vorige Position wird gelöscht
                        Feld[pzwei[1], pzwei[0]] = previous;
                        zeichneSpieler();
                        ereignisse(previous, peins[0], pzwei[0], peins[1], pzwei[1]);
                    }
                    else if(enpassentnow > 0)
                    {
                        rochadeaktualisieren(previous, peins[0]);
                        Feld[peins[1], peins[0]] = 0;
                        Feld[pzwei[1], pzwei[0]] = previous;
                        if (enpassentnow == 1)
                            Feld[pzwei[1] + 1, pzwei[0]] = 0;
                        else
                            Feld[pzwei[1] - 1, pzwei[0]] = 0;
                        zeichneSpieler();
                        ereignisse(previous, peins[0], pzwei[0], peins[1], pzwei[1]);
                    }
                    else
                    {
                        Error();
                        return false;
                    }
                }
                else
                {
                    if (!rochadepossible(rochade))
                    {
                        Error();
                        return false;
                    }
                    else
                    {
                        if (rochade == 1) //4,7 -> 7,7
                        {
                            Feld[7, 7] = 0;
                            Feld[7, 6] = 6;
                            Feld[7, 5] = 2;
                            Feld[7, 4] = 0;
                            Console.ForegroundColor = ConsoleColor.White;
                            zeichneSpieler();
                            Console.ForegroundColor = ConsoleColor.Black;
                        }
                        else if (rochade == 2) //4,7 -> 0,7
                        {
                            Feld[7, 0] = 0;
                            Feld[7, 2] = 6;
                            Feld[7, 3] = 2;
                            Feld[7, 4] = 0;
                            Console.ForegroundColor = ConsoleColor.White;
                            zeichneSpieler();
                            Console.ForegroundColor = ConsoleColor.Black;
                        }
                        else if (rochade == 3) //4,0 -> 7,0
                        {
                            Feld[0, 7] = 0;
                            Feld[0, 6] = 12;
                            Feld[0, 5] = 8;
                            Feld[0, 4] = 0;
                            Console.ForegroundColor = ConsoleColor.Black;
                            zeichneSpieler();
                        }
                        else if (rochade == 4) //4,0 -> 0,0
                        {
                            Feld[0, 0] = 0;
                            Feld[0, 2] = 12;
                            Feld[0, 3] = 8;
                            Feld[0, 4] = 0;
                            Console.ForegroundColor = ConsoleColor.Black;
                            zeichneSpieler();
                        }
                        rochadem[rochade - 1] = false;
                    }
                }
                if (differences != null)
                    paintdifferences();
                Console.SetCursorPosition(verschiebung[1], verschiebung[0] + 11);
                Console.Write("                        "); //Und die Eingabe gelöscht
                return true;
            }
            catch
            {
                Error();
                return false;
            }
        }

        public static bool rochadepossible(int rochade)
        {
            if (!rochadem[rochade - 1] || rochade == 1 && (Feld[7, 5] != 0 || Feld[7, 6] != 0) || rochade == 3 && (Feld[0, 5] != 0 || Feld[0, 6] != 0) || rochade == 2 && (Feld[7, 1] != 0 || Feld[7, 2] != 0 || Feld[7, 3] != 0) || rochade == 4 && (Feld[0, 1] != 0 || Feld[0, 2] != 0 || Feld[0, 3] != 0))
                return false;
            else return true;
        }

        public static void gewonnen()
        {
            Console.Clear();
            bool schwarz = false;
            for (byte i = 0; i < 8; i++)
            {
                for (byte j = 0; j < 8; j++)
                {
                    if (Feld[i, j] == 12)
                    {
                        schwarz = true;
                    }
                }
            }
            Console.WriteLine("Checkmate!");
            if (schwarz) Console.Write("Black ");
            else Console.Write("White ");
            Console.Write("wins");
            Console.ReadKey();
        }
        public static void ereignisse(int pre, int xv, int xn, int yv, int yn) //Besondere Ereignisse im Spiel
        {
            if (pre == 1 && yn == 0) //Bauer erreicht das Ende des Felds
            {
                a:
                Console.SetCursorPosition(verschiebung[1], verschiebung[0] + 13); //Der Spieler wählt eine neue Figur
                Console.Write("New Figure: ");
                char[] a = Console.ReadLine().ToString().ToUpper().ToCharArray();
                bool ersetzt = false;
                string check;
                check = Convert.ToString(a[0]);
                if (check.Contains("K")) goto king;
                char finput = a[0];
                for (byte i = 1; i < 6; i++)
                {
                    if (symbols[i] == finput)
                    {
                        ersetzt = true;
                        Figuren.Remove(Feld[yn, xn]);
                        Figuren.Add(i);
                        Feld[yn, xn] = i;
                        zeichneSpieler();
                        Console.ForegroundColor = ConsoleColor.Black;
                        break;
                    }
                }
                king:
                Console.SetCursorPosition(verschiebung[1], verschiebung[0] + 13);
                Console.Write("                                ");
                if (!ersetzt)
                {
                    Error();
                    goto a;//ugly code needs to be fought with ugy code - goto hell! -> This was LIAM
                }
            }
            if (pre == 7 && yn == 7)//Das gleiche nochmal für schwarz
            {
                a:
                Console.SetCursorPosition(verschiebung[1], verschiebung[0] + 13);
                Console.Write("New Figure: ");
                bool ersetzt = false;
                char[] a = Console.ReadLine().ToString().ToUpper().ToCharArray();
                string check;
                check = Convert.ToString(a[0]);
                if (check.Contains("K")) goto king;
                char finput = a[0];
                for (byte i = 1; i < 6; i++)
                {
                    if (symbols[i] == finput)
                    {
                        ersetzt = true;
                        Figuren.Remove(Feld[yn, xn]);
                        Figuren.Add(i + 6);
                        Feld[yn, xn] = Convert.ToByte(i + 6);
                        zeichneSpieler();
                        Console.ForegroundColor = ConsoleColor.Black;
                        break;
                    }
                }
                Console.SetCursorPosition(verschiebung[1], verschiebung[0] + 13);
                Console.Write("                                ");
                king:
                if (!ersetzt)
                {
                    Error();
                    goto a;//ugly code needs to be fought with ugy code - goto hell!
                }
            }
        }

        public static void rochadeaktualisieren(int previous, int x) //Es wird aktualisiert, ob der Player seine Rochade versaut hat
        {
            if (previous == 6)
            {
                rochadem[0] = false;
                rochadem[1] = false;
            }
            else if (previous == 12)
            {
                rochadem[2] = false;
                rochadem[3] = false;
            }
            else if (previous == 2)
            {
                if (x == 0)
                {
                    rochadem[0] = false;
                }
                else if (x == 7)
                {
                    rochadem[1] = false;
                }
            }
            else if (previous == 8)
            {
                if (x == 0)
                {
                    rochadem[2] = false;
                }
                else if (x == 7)
                {
                    rochadem[3] = false;
                }
            }
        }

        public static void Error() //Oh nein
        {
            zeichneSpieler();
            Console.ForegroundColor = ConsoleColor.Black;
            Console.SetCursorPosition(verschiebung[1], verschiebung[0] + 13);
            Console.Write("Error");
            Console.ReadKey();
            Console.SetCursorPosition(verschiebung[1], verschiebung[0] + 11);
            Console.Write("          ");
            Console.SetCursorPosition(verschiebung[1], verschiebung[0] + 13);
            Console.Write("                        ");
            checkschachgedöns();
        }
        public static void zeichnesymbol(char Symbol, int x, int y)
        {
            Console.SetCursorPosition(x + verschiebung[0], y + verschiebung[1]);
            if (y % 2 == 0 && x % 2 != 0 || y % 2 != 0 && x % 2 == 0) Console.BackgroundColor = ConsoleColor.DarkGray; //Die passende Hintergrundfarbe
            else Console.BackgroundColor = ConsoleColor.Gray;
            Console.Write(Symbol); //und das passende Symbol
            Console.BackgroundColor = background;
        }

        public static int convertToInt(char character)
        { //Jedem Buchstaben wird eine Zahl zugeordnet
            character = char.ToUpper(character);
            if (character == 'A') return 1;
            else if (character == 'B') return 2;
            else if (character == 'C') return 3;
            else if (character == 'D') return 4;
            else if (character == 'E') return 5;
            else if (character == 'F') return 6;
            else if (character == 'G') return 7;
            else return 8;
        }

        public static char convertToChar(int zahl)
        { //Jeder Zahl wird ein Buchstabe zugeordnet
            char character;
            char[] characters = new char[9] { ' ', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' };
            character = characters[zahl];
            return character;
        }

        public static void zeichneFeld()
        {
            for (byte i = 0; i < 8; i++)
            {
                for (byte u = 0; u < 8; u++)
                {
                    zeichnesymbol(' ', i, u); //Es werden einfach Leerzeichen geschrieben, die von dem zeichnesymbol void automatisch mit der richtigen Hintergrundfarbe ausgestattet werden
                }
            }
            for (byte i = 1; i < 9; i++)
            {
                Console.SetCursorPosition(verschiebung[1] - 1, verschiebung[0] + i - 1);
                Console.Write(i); //Die Zahlen am Spielfeldrand
                Console.SetCursorPosition(verschiebung[1] + i - 1, verschiebung[0] - 1);
                char buch = convertToChar(i);
                Console.Write(buch); //Die Buchstaben am Spielfeldrand
                Console.SetCursorPosition(verschiebung[1] + 8, verschiebung[0] + i - 1);//Zahl links
                Console.Write(i);
                Console.SetCursorPosition(verschiebung[1] - 1 + i, verschiebung[0] + 8);//Buchstaben unten
                Console.Write(buch);
            }
        }

        public static void zeichneSpieler()
        {
            for (byte i = 0; i < 8; i++)
            {
                for (byte u = 0; u < 8; u++)
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                    if (Feld[u, i] < 7) Console.ForegroundColor = ConsoleColor.White; //Die passende Farbe
                    zeichnesymbol(symbols[Feld[u, i]], i, u); //Alle Spieler vom Feld werden gezeichnet
                    Console.BackgroundColor = background;
                }
            }
        }

        public static bool zug()//Das war nicht ich :D
        {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.SetCursorPosition(verschiebung[1], verschiebung[0] + 9);
            if (züge % 2 == 0)
            {
                Console.Write("It's Black's " + züge/2 + ". turn       ");
                züge++;
                weiß = false;
                return true;
            }
            else
            {
                Console.Write("It's White's " + ((int)züge/(int)2 + (int)1).ToString() + ". turn           ");
                züge++;
                weiß = true;
                return false;
            }
        }


        public static long bewerte(byte[,] dasFeld, byte welcherzug)
        {
            long Bewertung = 0;
            if (Feld[3, 7] == 5 && Feld[1, 6] == 7 && dasFeld[2, 6] == 7)
                Bewertung += 10000;
            //if (checkWon(dasFeld)) Bewertung += 1000 / (welcherzug * welcherzug);//gewonnen
            //Bewertung += myScore(dasFeld); //Der Score (Bauern -> 1,...)
            Bewertung -= enScore(dasFeld, welcherzug) * 15; //Auch für den Gegner
            //Bewertung += Safety(dasFeld); //Wie sicher ist der König?
            Bewertung += Bauern(dasFeld, true); //Wie weit sind die Bauern?
            Bewertung -= Bauern(dasFeld, false); //Wie weit sind die Bauern?
            try
            {
                Bewertung -= Gegnerpossis(dasFeld, welcherzug); //Was für Möglichkeiten hat der Gegner dann?
            }
            catch {
                long a = Gegnerpossis(dasFeld, welcherzug);
            };
            Bewertung += Deckung(dasFeld) / 30;
            Bewertung -= kingposb[1];
            if (getKingpos(dasFeld, true)[0] != kingposb[0] || getKingpos(dasFeld, true)[1] != kingposb[1])
                Bewertung -= 100;
            if (isschachmatt(false, dasFeld))
            {
                if (welcherzug == 1)
                    Bewertung += 100000;
                else if (welcherzug == 2)
                    Bewertung += 1000;
                else
                    Bewertung += 100;
            }
            return Bewertung;
        }

        public static int Deckung(byte[,] dasFeld)
        {
            int Wert = 0;
            for (byte x2 = 0; x2 < 8; x2++)
            {
                for (byte y2 = 0; y2 < 8; y2++)
                {
                    if (Feld[y2, x2] > 6)
                    {
                        if (playergedeckt(dasFeld, x2, y2))
                        {
                            Wert += dasFeld[y2, x2];
                            if (ingefahr(x2, y2))
                                Wert += dasFeld[y2, x2] * 5;
                        }
                    }
                }
            }
            return Wert;
        }

        public static bool playergedeckt(byte[,] dasFeld, byte x2, byte y2)
        {
            bool gedeckt = false;
            byte pseudo = 0;
            byte[,] alt = new byte[8, 8];
            for (int i = 0; i < 8; i++)
            {
                for (int u = 0; u < 8; u++)
                {
                    alt[u, i] = Feld[u, i];
                }
            }
            Feld = dasFeld;
            z = false;

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (Feld[y, x] > 6)
                    {

                        if (Feld[y2, x2] < 12)
                        {
                            pseudo = Convert.ToByte(Feld[y, x] - 6);
                            if (allowed(pseudo, x, x2, y, y2, true))
                            {
                                gedeckt = true;
                                Feld = alt;
                                z = true;
                                return gedeckt;
                            }
                        }
                    }
                }
            }
            Feld = alt;
            z = true;
            return gedeckt;
        }

        public static bool ingefahr(int x2, int y2)
        {
            bool ingefahr = false;
            for (int x1 = 0; x1 < 8; x1++)
            {
                for (int y1 = 0; y1 < 8; y1++)
                {
                    if (allowed(Feld[y1, x1], x1, x2, y1, y2, true))
                    {
                        ingefahr = true;
                        return ingefahr;
                    }
                }
            }
            return ingefahr;
        }

        public static long Gegnerpossis(byte[,] dasFeld, byte welcherzug)
        {
            byte[,] now = new byte[8, 8];
            for (int i = 0; i < Feld.GetLength(0); i++)
            {
                for (int j = 0; j < Feld.GetLength(1); j++)
                {
                    now[i, j] = Feld[i, j];
                }
            }
            Feld = dasFeld;
            long Wert = 0;
            for (byte x = 0; x < 8; x++)
            {
                for (byte y = 0; y < 8; y++)
                {
                    if (Feld[y, x] < 7 && Feld[y, x] > 0)
                    {
                        for (byte x2 = 0; x2 < 8; x2++)
                        {
                            for (byte y2 = 0; y2 < 8; y2++)
                            {
                                z = false;
                                if (allowed(Feld[y, x], x, x2, y, y2, true))
                                {
                                    long add = 0;
                                    if (dasFeld[y2, x2] > 6)
                                        add += (long)Werte[dasFeld[y2, x2] - 6];
                                    //if (dasFeld[y2, x2] == 12 && welcherzug == 1) add += 1000000;
                                    add = add * 500;
                                    if (playergedeckt(dasFeld, x2, y2))
                                            add -= (long)Werte[Feld[y, x]] * 500;
                                    if (add < 0) add = 0;
                                    Wert += add;
                                    z = false;
                                }
                                if ((allowed(Feld[y, x], x, x2, y, y2, false) || allowed(Feld[y, x], x, x2, y, y2, true)))
                                {
                                    byte[,] temp = new byte[8, 8];
                                    for (int x3 = 0; x3 < 8; x3++)
                                    {
                                        for (int y3 = 0; y3 < 8; y3++)
                                        {
                                            temp[y3, x3] = Feld[y3, x3];
                                        }
                                    }
                                    temp[y, x] = 0;
                                    temp[y2, x2] = Feld[y, x];
                                    if (isschach(true, temp))
                                        Wert += 3;
                                    if (isschachmatt(true, temp) && welcherzug == 1)
                                    {
                                        Wert += 1000000;
                                        z = true;
                                        Feld = now;
                                        return Wert;
                                    }
                                    else if (isschachmatt(true, temp))
                                    {
                                        Wert += 1000;
                                        z = true;
                                        Feld = now;
                                        return Wert;
                                    }
                                }
                                z = true;
                            }
                        }
                    }
                }
            }
            Feld = now;
            return Wert;
        }

        public static bool checkWon(byte[,] dasFeld)
        {
            for (byte x = 0; x < 8; x++)
            {
                for (byte y = 0; y < 8; y++)
                {
                    if (!z && dasFeld[y, x] == 12 || z && dasFeld[y, x] == 6) return false;
                }
            }
            return true;
        }

        public static int myScore(byte[,] dasFeld)
        {
            int Score = 0;
            int figur = 0;
            for (byte x = 0; x < 8; x++)
            {
                for (byte y = 0; y < 8; y++)
                {
                    if (Feld[y, x] != 0)
                    {
                        figur = Feld[y, x];
                        if (Feld[y, x] > 6)
                        {
                            figur -= 7;
                            Score += Werte[figur - 6];
                        }
                    }
                }
            }
            return Score;
        }

        public static int enScore(byte[,] dasFeld, byte welcherzug)
        {
            int Score = 0;
            int figur = 0;
            for (byte x = 0; x < 8; x++)
            {
                for (byte y = 0; y < 8; y++)
                {
                    if (dasFeld[y, x] != 0 && dasFeld[y,x] < 6)
                    {
                        figur = dasFeld[y, x];
                            Score += Werte[figur];
                    }
                }
            }
            //if (welcherzug != 1 && Score < Werte[6]) Score += Werte[6] -5;
            //Score += ((welcherzug - 1) / 4) * Score;
            return Score;
        }

        public static int Safety(byte[,] dasFeld)
        {
            int[] kingpos = getKingpos(dasFeld, true);
            int safety = 0;
            if (!kingpos.Contains(10))
            {
                for (byte i = 0; i < 3; i++)
                {
                    try
                    {
                        if (Feld[kingpos[1] - 1, kingpos[0] - 1 + i] > 6) safety++;
                        if (Feld[kingpos[1] + 1, kingpos[0] - 1 + i] > 6) safety++;
                    }
                    catch { }
                }
                try
                {
                    if (Feld[kingpos[1], kingpos[0] - 1] > 6) safety++;
                    if (Feld[kingpos[1], kingpos[0] + 1] > 6) safety++;
                }
                catch { }
            }
            return safety;
        }

        public static int[] getKingpos(byte[,] dasFeld, bool schwarz)
        {
            int[] pos = new int[2] { 10, 10 };
            for (byte x = 0; x < 8; x++)
            {
                for (byte y = 0; y < 8; y++)
                {
                    if (schwarz && dasFeld[y, x] == 12 || !schwarz && dasFeld[y, x] == 6)
                    {
                        pos[0] = x;
                        pos[1] = y;
                    }
                }
            }
            return pos;
        }

        public static int Bauern(byte[,] dasFeld, bool schwarz)
        {
            int[,] bauernpos = getBauernpos(dasFeld, schwarz);
            int bauernscore = 0;
            for (byte i = 0; i < 8; i++)
            {
                if (bauernpos[i, 0] != 10)
                {
                    bauernscore += bauernpos[i, 1] * bauernpos[i, 1];
                }
                else break;
            }
            return bauernscore;
        }

        public static int[,] getBauernpos(byte[,] dasFeld, bool schwarz) //Die Positionen der Bauern
        {
            int[,] pos = new int[8, 2]
            {
                { 10, 10 },
                { 10, 10 },
                { 10, 10 },
                { 10, 10 },
                { 10, 10 },
                { 10, 10 },
                { 10, 10 },
                { 10, 10 },
            };
            int apos = 0;
            for (byte x = 0; x < 8; x++)
            {
                for (byte y = 0; y < 8; y++)
                {
                    if (schwarz && dasFeld[y, x] == 7 || !schwarz && dasFeld[y, x] == 1)
                    {
                        pos[apos, 0] = x;
                        pos[apos, 1] = y;
                        apos++;
                    }
                }
            }
            return pos;
        }
        #endregion
    }
}
