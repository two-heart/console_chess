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
        public static bool[] rochadem = new bool[4] { true, true, true, true }; //Kurze Rochade weiß, lange Rochade weiß, kurze Rochade schwarz, lange Rochade schwarz
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
            { ' ', 'B', 'T', 'S', 'L', 'D', 'K', 'B', 'T', 'S', 'L', 'D', 'K' };
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
            zug();
            do
            {
                bool z;
                Console.SetCursorPosition(1, 12); //Der Spieler macht seine Eingabe
                input = Console.ReadLine();
                if (verarbeite(input)) z = zug();   //Die wiederum verarbeitet wird
            } while (true == !false);
        }
        public static bool nichtdazwischen(int previous, int xv, int xn, int yv, int yn)
        {
            int dx = delta(xv, xn);
            int dy = delta(yv, yn);
            if (previous == 0 || dx == 0 && dy == 0) return false;
            if (previous == 5 || previous == 11)
            {
                if (dx == 0 || dy == 0) previous = 2;
                else if (dx == dy) previous = 3;
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

        public static int delta(int a, int b)
        {
            int c = a - b;
            if (c < 0) c = -c;
            return c;
        }


        public static bool allowed(int pre, int xv, int xn, int yv, int yn, bool schlagen) //Überprüfung ob Zug erlaubt ist
        {
            int dy = delta(yv, yn);
            int dx = delta(xv, xn);
            if (pre == 0) return false; //keine Figur
            if (xv == xn && yv == yn) return false;  //Zielfeld = Endfeld

            else if (pre == 1 && !schlagen)//Bauer
            {
                if (yn == yv - 1) return true;
                else if (yv == 6 && yn == yv - 2) return true;
                else return false;
            }
            else if(pre == 1 && schlagen)
            {
                if (yn == yv - 1 && dx == 1) return true;
                else return false;
            }
            else if (pre == 7 && !schlagen)
            {
                if (yn == yv + 1) return true;
                else if (yv == 1 && yn == yv + 2) return true;
                else return false;
            }
            else if (pre == 7 && schlagen)
            {
                if (yn == yv - 1 && dx == 1) return true;
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
            ushort rochade = 0; //weiß klein, weiß groß, schwarz klein, schwarz groß
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
                int previous; //Das ist die Figur, die bewegt wird
                previous = Feld[peins[1], peins[0]];

                if ((Feld[pzwei[1], pzwei[0]] == 0 && !schlagen || Feld[pzwei[1], pzwei[0]] != 0 && schlagen) && (weiß && previous < 7 || !weiß && previous >= 7)/*Ist auch die passende Farbe am Zug?*/ && allowed(previous, peins[0], pzwei[0], peins[1], pzwei[1], schlagen) /*Ist der Zug (auf einem leeren Feld) erlaubt*/ && nichtdazwischen(previous, peins[0], pzwei[0], peins[1], pzwei[1])/*Ist keine Figur dazwischen*/)
                {
                    rochadeaktualisieren(previous, peins[0]);
                    Feld[peins[1], peins[0]] = 0; //Die vorige Position wird gelöscht
                    zeichnesymbol(' ', peins[0], peins[1]);
                    Console.ForegroundColor = ConsoleColor.Black;
                    if (previous < 7) Console.ForegroundColor = ConsoleColor.White; //Und die neue in der passenden Farbe gezeichnet
                    Feld[pzwei[1], pzwei[0]] = previous;
                    zeichnesymbol(symbols[previous], pzwei[0], pzwei[1]);
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                else
                {
                    Error();
                    return false;
                }
            }
            else
            {
                if(!rochadem[rochade - 1] || rochade == 1 && (Feld[7, 5] != 0 || Feld[7, 6] != 0) || rochade == 3 && (Feld[0, 5] != 0 ||Feld[0, 6] != 0) || rochade == 2 && (Feld[7, 1] != 0 || Feld[7, 2] != 0 || Feld[7, 3] != 0) || rochade == 4 && (Feld[0, 1] != 0 || Feld[0, 2] != 0 || Feld[0, 3] != 0))
                {
                    Error();
                    return false;
                }
                else
                {
                    if (rochade == 1) //4,7 -> 7,7
                    {
                        Feld[7, 4] = 2;
                        Feld[7, 7] = 6;
                        Console.ForegroundColor = ConsoleColor.White;
                        zeichnesymbol('T', 4, 7);
                        zeichnesymbol('K', 7, 7);
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    else if (rochade == 2) //4,7 -> 0,7
                    {
                        Feld[7, 4] = 2;
                        Feld[7, 0] = 6;
                        Console.ForegroundColor = ConsoleColor.White;
                        zeichnesymbol('T', 4, 7);
                        zeichnesymbol('K', 0, 7);
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    else if (rochade == 3) //4,0 -> 7,0
                    {
                        Feld[0, 4] = 8;
                        Feld[0, 7] = 12;
                        Console.ForegroundColor = ConsoleColor.Black;
                        zeichnesymbol('T', 4, 0);
                        zeichnesymbol('K', 7, 0);
                    }
                    else if (rochade == 4) //4,0 -> 0,0
                    {
                        Feld[0, 4] = 8;
                        Feld[0, 0] = 12;
                        Console.ForegroundColor = ConsoleColor.Black;
                        zeichnesymbol('T', 4, 0);
                        zeichnesymbol('K', 0, 0);
                    }
                    rochadem[rochade - 1] = false;
                }
            }
            ereignisse(previous, peins[0], pzwei[0], peins[1], pzwei[1]);
            Console.SetCursorPosition(1, 12);
            Console.Write("                        "); //Und die Eingabe gelöscht
            return true;
        }
        
        static void ereignisse(int pre, int xv, int xn, int yv, int yn) //Besondere Ereignisse im Spiel
        {
            if (pre == 1 && xn == 1) //Bauer erreicht das Ende des Felds
            {
                Console.SetCursorPosition(1, 12); //Der Spieler wählt eine neue Figur
                char finput = Convert.ToChar(Console.ReadLine());
                for (int i = 1; i < 12; i++)
                {
                    if (symbols[i]  == finput)
                    { 
                        Feld[xn - 1, yn - 1] = i;
                        break;
                    }
                }
            }
            if (pre == 7 && xn == 8)//Das gleiche nochmal für schwarz
            {
                Console.SetCursorPosition(1, 12);
                char finput = Convert.ToChar(Console.ReadLine());
                for (int i = 1; i < 12; i++)
                {
                    if (symbols[i] == finput)
                    {
                        Feld[xn - 1, yn - 1] = i+6;
                        break;
                    }
                }
            }
            zeichneFeld();
        }

        public static void rochadeaktualisieren(int previous, int x)
        {
            if (previous == 6)
            {
                rochadem[0] = false;
                rochadem[1] = false;
            }
            else if(previous == 12)
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

        public static void Error()
        {
            Console.SetCursorPosition(1, 14);
            Console.Write("Error");
            Console.ReadKey();
            Console.SetCursorPosition(1, 14);
            Console.Write("          ");
            Console.SetCursorPosition(1, 12);
            Console.Write("                        ");
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
                Console.Write("Weiß ist am Zug " + züge + "> ");
                züge++;
                weiß = true;
                return false;
            }
        }
    }
}

