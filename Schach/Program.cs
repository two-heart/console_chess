using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schach
{
    class Program
    {

        public static int züge;
        public static bool weiß; //Ist weiß am Zug?
        public static int[] verschiebung = new int[2] { 1, 1 }; //Um wie viel ist das Spielfeld nach rechts / unten verschoben?
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
        public static int[,] Feld = new int[8, 8] //Das Feld mit den passenden Nummern (s.o.)
        {
            {8 ,9 ,10,11,12,10,9 ,8 },
            {7 ,7 ,7 ,7 ,7 ,7 ,7 ,7 },
            {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
            {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
            {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
            {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
            {1 ,1 ,1 ,1 ,1 ,1 ,1 ,1 },
            {2 ,3 ,4 ,5 ,6 ,4 ,3 ,2 },
        };
        public static char[] symbols = new char[13] //Die entsprechenden Symbole für die Figuren
            {' ','B','T','P','L','D','K','B','T','P','L','D','K'};
        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.White; //Die Standardfarben
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Clear();
            Console.CursorVisible = false;
            zeichneFeld();
            zeichneSpieler();
            Console.ForegroundColor = ConsoleColor.Black;
            string input;
            a: //Wichtig
                bool z = zug();
                Console.SetCursorPosition(1, 12); //Der Spieler macht seine Eingabe
                input = Console.ReadLine();
                verarbeite(input);  //Die wiederum verarbeitet wird
            goto a;
        }

        public static void verarbeite(string input) //Und zwar hier
        {
            string[] splitted = input.Split(' '); //Die Eingabe sollte in der Form A1 B3 sein, wobei der erste Teil das Start- und der zweite Teil das Endfeld ist. Die Eingabe wird demenstsprechend nach Start- und Endfeld aufgesplitted1t
            if (splitted.Length != 2) //Es sollten natürlich nur zwei Felder sein
            {
                Console.Clear();
                Console.Write("ERROR");
                Console.ReadKey();
                return;
            }

            char[] eins = splitted[0].ToCharArray();    //Diese zwei Felder werden dann wieder zu x und y gesplittet
            char[] zwei = splitted[1].ToCharArray();
            int[] peins = new int[2] { convertToInt(eins[0]) - 1, Convert.ToInt32(eins[1] - '0') - 1 }; //Und der Buchstabe wird in eine equivalente Zahl umgewandelt
            int[] pzwei = new int[2] { convertToInt(zwei[0]) - 1, Convert.ToInt32(zwei[1] - '0') - 1 };
            int previous; //Das ist die Figur, die bewegt wird
            previous = Feld[peins[1], peins[0]];
            if (Feld[pzwei[1], pzwei[0]] == 0 && (weiß && previous < 7 || !weiß && previous >= 7)/*Ist auch die passende Farbe am Zug?*/ && previous != 0 && (previous != 2 && previous != 8 || (pzwei[0] == peins[0] ||pzwei[1] == peins[1]))/*Die vorzeitige Turm-Regel, die noch in ein extra void verlegt werden muss*/)
            {
                Feld[peins[1], peins[0]] = 0; //Die vorige Position wird gelöscht
                zeichnesymbol(' ', peins[0], peins[1]);
                Console.ForegroundColor = ConsoleColor.Black;
                if (previous < 7) Console.ForegroundColor = ConsoleColor.White; //Und die neue in der passenden Farbe gezeichnet
                Feld[pzwei[1], pzwei[0]] = previous;
                zeichnesymbol(symbols[previous], pzwei[0], pzwei[1]);
                Console.ForegroundColor = ConsoleColor.Black;
            }
            Console.SetCursorPosition(1, 12);
            Console.Write("                        "); //Und die Eingabe gelöscht
        }

        public static void zeichnesymbol(char Symbol, int x, int y)
        {
            Console.SetCursorPosition(x + verschiebung[0], y + verschiebung[1]);
            if (y % 2 == 0 && x % 2 != 0 || y % 2 != 0 && x % 2 == 0) Console.BackgroundColor = ConsoleColor.DarkGray; //Die passende Hintergrundfarbe
            else Console.BackgroundColor = ConsoleColor.Gray;
            Console.Write(Symbol); //und das passende Symbol
            Console.BackgroundColor = ConsoleColor.White;
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

        public static void convertToChar(int zahl, out char character)
        { //Jeder Zahl wird ein Buchstabe zugeordnet
            if (zahl == 1) character = 'A';
            else if (zahl == 2) character = 'B';
            else if (zahl == 3) character = 'C';
            else if (zahl == 4) character = 'D';
            else if (zahl == 5) character = 'E';
            else if (zahl == 6) character = 'F';
            else if (zahl == 7) character = 'G';
            else character = 'H';
        }

        public static void zeichneFeld()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int u = 0; u < 8; u++)
                {
                    zeichnesymbol(' ', i, u); //Es werden einfach Leerzeichen geschrieben, die von dem zeichnesymbol void automatisch mit der richtigen Hintergrundfarbe ausgestattet werden
                }
            }
            for (int i = 1; i < 9; i++)
            {
                Console.SetCursorPosition(0, verschiebung[0] + i - 1);
                Console.Write(i); //Die Zahlen am Spielfeldrand
                Console.SetCursorPosition(verschiebung[1] + i - 1, 0);
                char buch;
                convertToChar(i, out buch);
                Console.Write(buch); //Die Buchstaben am Spielfeldrand
            }
        }

        public static void zeichneSpieler()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int u = 0; u < 8; u++)
                {
                    Console.OutputEncoding = Encoding.Unicode; //Das ist inzwische eigentlich unnötig
                    Console.ForegroundColor = ConsoleColor.Black;
                    if (Feld[u, i] < 7) Console.ForegroundColor = ConsoleColor.White; //Die passende Farbe
                    zeichnesymbol(symbols[Feld[u, i]], i, u); //Alle Spieler vom Feld werden gezeichnet
                    Console.BackgroundColor = ConsoleColor.White;
                }
            }
        }

        public static bool zug()//Das war nicht ich :D
        {
            Console.SetCursorPosition(1, 10);
            if (züge % 2 == 0)
            {
                Console.Write("Schwarz ist am Zug " + züge + "> ");
                züge++;
                weiß = false;
                return true;
            }
            else
            {
                Console.Write("Weiß ist am Zug (" + züge + ")> ");
                züge++;
                weiß = true;
                return false;
            }
        }
    }
}
