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
        public static int[] verschiebung = new int[2] { 2, 2 }; //Um wie viel ist das Spielfeld nach rechts / unten verschoben?
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
        public static byte[,] Feld = new byte[8, 8] //Das Feld mit den passenden Nummern (s.o.)
        {
            {8 ,9 ,10,11,12,10,9 ,8 },
            {7 ,7 ,7 ,0 ,7 ,7 ,7 ,7 },
            {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
            {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
            {5 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
            {0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
            {1 ,1 ,1 ,1 ,1 ,1 ,1 ,1 },
            {2 ,3 ,4 ,5 ,6 ,4 ,3 ,2 },
        };
        //public static int[,,,] possis = new int[2, 1000000, 8, 8];
        public static char[] symbols = new char[13] //Die entsprechenden Symbole für die Figuren
            { ' ', 'B', 'T', 'S', 'L', 'D', 'K', 'B', 'T', 'S', 'L', 'D', 'K' };
        public static bool z;
        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.White; //Die Standardfarben
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Clear();
            Console.CursorVisible = true;
            zeichneFeld();
            zeichneSpieler();
            Console.ForegroundColor = ConsoleColor.Black;
            string input;
            z = zug();
            do
            {
                Console.ForegroundColor = ConsoleColor.Black;
                if (!z)
                {
                    Console.SetCursorPosition(verschiebung[1], verschiebung[0] + 11); //Der Spieler macht seine Eingabe
                    input = Console.ReadLine();
                    if (verarbeite(input)) z = zug();   //Die wiederum verarbeitet wird
                }
                else
                {
                    botzug();
                    z = zug();
                }
            } while (!won());
        }
        static bool won()//checks if so
        {
            byte könige = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
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
            for (int u = 0; u < array.GetLength(1); u++)
            {
                for (int i = 0; i < array.GetLength(0); i++)
                {
                    Liste.Add(array[i, u]);
                }
            }
            return Liste;
        }

        static void botzug()
        {
            byte[,] temp = new byte[8, 8]; //Das zu untersuchende Feld
            byte[,] tempa = new byte[8, 8]; //Der erste Zug (Wird auch untersucht)
            int bew = 0; //Der Zug
            byte[,,] eins;
            byte[,,,] zwei, drei;
            possibilities(out eins, out zwei, out drei); //Die Möglichkeiten(Nach dem dritten Zug)
            int besterzug = 0; //Der beste Zug
            int q = 0; //Nur da, um i zu speichern
            int welcherzug = 0;


            for (int i = 0; i < eins.GetLength(1); i++)
            {
                for (int a = 0; a < 8; a++)
                {
                    for (int s = 0; s < 8; s++)
                    {
                        temp[s, a] = eins[i, s, a]; //Das zu untersuchende Feld wird definiert
                    }
                }
                List<byte> liste = tolist(temp); //Und zur Liste konvertiert
                bool erlaubt = false;
                for (byte t = 1; t < liste.LastIndexOf(liste.Last()); t++)
                {
                    if (liste.Contains(t)) erlaubt = true;//Überprüft nur die Felder, in denen nicht nur Nullen stehen
                }
                if (!erlaubt) break;
                bew = bewerte(temp); //Wenn alles erlaubt ist, wird bew damit definiert
                if (bew > besterzug || welcherzug <= 0 || welcherzug > 3 || besterzug == 0)
                {
                    besterzug = bew; //Und gegebenenfalls wird der bestezug aktualisiert
                    q = i; //Und i in q gespeichert
                    welcherzug = 1;
                }
            }

            for (int i = 0; i < zwei.GetLength(1); i++)
            {
                for (int a = 0; a < 8; a++)
                {
                    for (int s = 0; s < 8; s++)
                    {
                        temp[s, a] = zwei[0, i, s, a]; //Das zu untersuchende Feld wird definiert
                    }
                }
                for (int a = 0; a < 8; a++)
                {
                    for (int s = 0; s < 8; s++)
                    {
                        tempa[s, a] = zwei[1, i, s, a]; //Das zu untersuchende Feld wird definiert
                    }
                }
                List<byte> liste = tolist(temp); //Und zur Liste konvertiert
                bool erlaubt = false;
                for (byte t = 1; t < liste.LastIndexOf(liste.Last()); t++)
                {
                    if (liste.Contains(t)) erlaubt = true;//Überprüft nur die Felder, in denen nicht nur Nullen stehen
                }
                if (!erlaubt) break;
                bew = (bewerte(temp) + bewerte(tempa)) / 2; //Wenn alles erlaubt ist, wird bew damit definiert
                if (bew > besterzug || besterzug == 0)
                {
                    besterzug = bew; //Und gegebenenfalls wird der bestezug aktualisiert
                    q = i; //Und i in q gespeichert
                    welcherzug = 2;
                }
            }

            for (int i = 0; i < drei.GetLength(1); i++)
            {
                for (int a = 0; a < 8; a++)
                {
                    for (int s = 0; s < 8; s++)
                    {
                        temp[s, a] = drei[0, i, s, a]; //Das zu untersuchende Feld wird definiert
                    }
                }
                for (int a = 0; a < 8; a++)
                {
                    for (int s = 0; s < 8; s++)
                    {
                        tempa[s, a] = drei[1, i, s, a]; //Das zu untersuchende Feld wird definiert
                    }
                }
                List<byte> liste = tolist(temp); //Und zur Liste konvertiert
                bool erlaubt = false;
                for (byte t = 1; t < liste.LastIndexOf(liste.Last()); t++)
                {
                    if (liste.Contains(t)) erlaubt = true;//Überprüft nur die Felder, in denen nicht nur Nullen stehen
                }
                if (!erlaubt) break;
                bew = (bewerte(temp) + bewerte(tempa)) / 2; //Wenn alles erlaubt ist, wird bew damit definiert
                if (bew > besterzug || besterzug == 0)
                {
                    besterzug = bew; //Und gegebenenfalls wird der bestezug aktualisiert
                    q = i; //Und i in q gespeichert
                    welcherzug = 3;
                }
            }
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (welcherzug == 1)
                    {
                        Feld[y, x] = eins[q, y, x]; //Wenn alle Möglichkeiten überprüft wurden, wird das Feld mit dem besten Zug aktualisiert
                    }
                    else if (welcherzug == 2)
                    {
                        Feld[y, x] = zwei[1, q, y, x];
                    }
                    else if (welcherzug == 3)
                    {
                        Feld[y, x] = drei[1, q, y, x];
                    }
                    else Error();
                }
            }
            Console.SetCursorPosition(20, 20); Console.Write(bewerte(Feld));
            zeichneSpieler(); //und in gezeichnet

            Console.ForegroundColor = ConsoleColor.Black; Console.SetCursorPosition(10, 0); Console.Write(bew.ToString());//Das ist nur zum Bugfixing
            for (int i = 0; i < 8; i++)
            {
                for (int u = 0; u < 8; u++)
                {
                    Console.SetCursorPosition(2 * i + 15, u);
                    if (welcherzug == 1)
                        Console.Write(eins[q, u, i]);
                    else if (welcherzug == 2)
                        Console.Write(zwei[0, q, u, i]);
                    else if (welcherzug == 3)
                        Console.Write(drei[0, q, u, i]);
                    else Error();
                }
            }
            Console.Write(" " + welcherzug + " " + q);
        }

        public static void testfeld(int[,,] possis) //nur zu Testzwecken - zeichnet alle Felder des Arrays
        {
            Console.Clear();
            int line = 0;
            for (int i = 0; i < 1000000; i++)
            {
                for (int u = 0; u < 8; u++)
                {
                    for (int z = 0; z < 8; z++)
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
            for (int x = 0; x < 8; x++)//erster zug
            {
                for (int y = 0; y < 8; y++)
                {
                    if (Feld[y, x] > 6)
                    {
                        temp = getpossisofthis(Feld[y, x], x, y); //Die derzeitige Möglichkeit
                        if (temp.GetLength(0) > 0)
                        {
                            for (int i = 0; i < temp.GetLength(0) && i + pos < eins.GetLength(0); i++)
                            {
                                for (int u = 0; u < 8; u++)
                                {
                                    for (int s = 0; s < 8; s++)
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
                for (int i = 0; i < 8; i++)
                {
                    for (int u = 0; u < 8; u++)
                    {
                        Feld[u, i] = eins[h, u, i];
                    }
                }
                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        if (Feld[y, x] > 6)
                        {
                            temp = getpossisofthis(Feld[y, x], x, y);
                            if (temp.GetLength(0) > 0)
                            {
                                for (int i = 0; i < temp.GetLength(0) && i + pos < zwei.GetLength(1); i++)
                                {
                                    for (int u = 0; u < 8; u++)
                                    {
                                        for (int s = 0; s < 8; s++)
                                        {
                                            zwei[0, i + pos, s, u] = temp[i, s, u];
                                            for (int b = 0; b < 8; b++)
                                            {
                                                for (int n = 0; n < 8; n++)
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
                for (int i = 0; i < 8; i++)
                {
                    for (int u = 0; u < 8; u++)
                    {
                        Feld[u, i] = zwei[0, h, u, i];
                    }
                }
                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        if (Feld[y, x] > 6)
                        {
                            temp = getpossisofthis(Feld[y, x], x, y);
                            if (temp.GetLength(0) > 0)
                            {
                                for (int i = 0; i < temp.GetLength(0); i++)
                                {
                                    for (int u = 0; u < 8; u++)
                                    {
                                        for (int s = 0; s < 8; s++)
                                        {
                                            drei[0, i + pos, s, u] = temp[i, s, u];
                                            for (int b = 0; b < 8; b++)
                                            {
                                                for (int n = 0; n < 8; n++)
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
            for (int i = 0; i < 8; i++)
            {
                for (int u = 0; u < 8; u++)
                {
                    if ((allowed(pre, x, i, y, u, false) || allowed(pre, x, i, y, u, true)) && nichtdazwischen(pre, x, i, y, u)) //Ist diese Möglichkeit erlaubt
                    {
                        for (int q = 0; q < 8; q++)
                        {
                            for (int w = 0; w < 8; w++)
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
                for (int u = 0; u < 8; u++)
                {
                    for (int s = 0; s < 8; s++)
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
                    if (!rochadem[rochade - 1] || rochade == 1 && (Feld[7, 5] != 0 || Feld[7, 6] != 0) || rochade == 3 && (Feld[0, 5] != 0 || Feld[0, 6] != 0) || rochade == 2 && (Feld[7, 1] != 0 || Feld[7, 2] != 0 || Feld[7, 3] != 0) || rochade == 4 && (Feld[0, 1] != 0 || Feld[0, 2] != 0 || Feld[0, 3] != 0))
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
        static void gewonnen()
        {
            Console.Clear();
            bool schwarz = false;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (Feld[i, j] == 12)
                    {
                        schwarz = true;
                    }
                }
            }
            Console.WriteLine("SCHACH MATT!");
            if (schwarz) Console.Write("Schwarz ");
            else Console.Write("Weiß ");
            Console.Write("gewinnt");
            Console.ReadKey();
        }
        static void ereignisse(int pre, int xv, int xn, int yv, int yn) //Besondere Ereignisse im Spiel
        {
            if (pre == 1 && yn == 0) //Bauer erreicht das Ende des Felds
            {
                a:
                Console.SetCursorPosition(verschiebung[1], verschiebung[0] + 13); //Der Spieler wählt eine neue Figur
                Console.Write("Neue Figur: ");
                char[] a = Console.ReadLine().ToString().ToUpper().ToCharArray();
                bool ersetzt = false;
                string check;
                check = Convert.ToString(a[0]);
                if (check.Contains("K")) goto king;
                char finput = a[0];
                for (byte i = 1; i < 12; i++)
                {
                    if (symbols[i] == finput)
                    {
                        ersetzt = true;
                        Feld[yn, xn] = i;
                        zeichneSpieler();
                        Console.ForegroundColor = ConsoleColor.Black;
                        break;
                    }
                }
                king:
                Console.SetCursorPosition(verschiebung[1], verschiebung[0] + 12);
                Console.Write("                                ");
                if (!ersetzt)
                {
                    Error();
                    goto a;//ugly code needs to be fought with ugy code - goto hell!
                }
            }
            if (pre == 7 && yn == 7)//Das gleiche nochmal für schwarz
            {
                a:
                Console.SetCursorPosition(verschiebung[1], verschiebung[0] + 12);
                Console.Write("Neue Figur: ");
                bool ersetzt = false;
                char[] a = Console.ReadLine().ToString().ToUpper().ToCharArray();
                string check;
                check = Convert.ToString(a[0]);
                if (check.Contains("K")) goto king;
                char finput = a[0];
                for (byte i = 1; i < 12; i++)
                {
                    if (symbols[i] == finput)
                    {
                        Feld[yn, xn] = Convert.ToByte(i + 6);
                        zeichneSpieler();
                        Console.ForegroundColor = ConsoleColor.Black;
                        break;
                    }
                }
                Console.SetCursorPosition(verschiebung[1], verschiebung[0] + 12);
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
            Console.ForegroundColor = ConsoleColor.Black;
            Console.SetCursorPosition(verschiebung[1], verschiebung[0] + 13);
            Console.Write("Error");
            Console.ReadKey();
            Console.SetCursorPosition(verschiebung[1], verschiebung[0] + 13);
            Console.Write("          ");
            Console.SetCursorPosition(verschiebung[1], verschiebung[0] + 13);
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

        public static char convertToChar(int zahl)
        { //Jeder Zahl wird ein Buchstabe zugeordnet
            char character;
            if (zahl == 1) character = 'A';
            else if (zahl == 2) character = 'B';
            else if (zahl == 3) character = 'C';
            else if (zahl == 4) character = 'D';
            else if (zahl == 5) character = 'E';
            else if (zahl == 6) character = 'F';
            else if (zahl == 7) character = 'G';
            else character = 'H';
            return character;
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
                Console.SetCursorPosition(verschiebung[1] - 1, verschiebung[0] + i - 1);
                Console.Write(i); //Die Zahlen am Spielfeldrand
                Console.SetCursorPosition(verschiebung[1] + i - 1, verschiebung[0] - 1);
                char buch = convertToChar(i);
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
            Console.ForegroundColor = ConsoleColor.Black;
            Console.SetCursorPosition(verschiebung[1], verschiebung[0] + 9);
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


        public static int bewerte(byte[,] dasFeld)
        {
            int Bewertung = 0;
            if (checkWon(dasFeld)) Bewertung += 1000;//gewonnen
            //Bewertung += myScore(dasFeld); //Der Score (Bauern -> 1,...)
            Bewertung -= enScore(dasFeld) * 3; //Auch für den Gegner
            Bewertung += Safety(dasFeld); //Wie sicher ist der König?
            Bewertung += Bauern(dasFeld) / 2; //Wie weit sind die Bauern?
            Bewertung -= Gegnerpossis(dasFeld) * 4; //Was für Möglichkeiten hat der Gegner dann?
            return Bewertung;
        }

        public static int Gegnerpossis(byte[,] dasFeld)
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
            int Wert = 0;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (Feld[y, x] < 7 && Feld[y, x] > 0)
                    {
                        for (int x2 = 0; x2 < 8; x2++)
                        {
                            for (int y2 = 0; y2 < 8; y2++)
                            {
                                z = false;
                                if (allowed(Feld[y, x], x, x2, y, y2, true) && nichtdazwischen(Feld[y, x], x, x2, y, y2))
                                {
                                    if (dasFeld[y2, x2] == 7) Wert++;
                                    if (dasFeld[y2, x2] == 8 || dasFeld[y, x] == 10) Wert += 4;
                                    if (dasFeld[y2, x2] == 9) Wert += 7;
                                    if (dasFeld[y2, x2] == 11) Wert += 15;
                                    if (dasFeld[y2, x2] == 12) Wert += 1000000;
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
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (!z && dasFeld[y, x] == 12 || z && dasFeld[y, x] == 6) return false;
                }
            }
            return true;
        }

        public static int myScore(byte[,] dasFeld)
        {
            int Score = 0;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (dasFeld[y, x] == 7) Score++;
                    else if (dasFeld[y, x] == 8 || dasFeld[y, x] == 10) Score += 4;
                    else if (dasFeld[y, x] == 9) Score += 7;
                    else if (dasFeld[y, x] == 11) Score += 15;
                }
            }
            return Score;
        }

        public static int enScore(byte[,] dasFeld)
        {
            int Score = 0;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (dasFeld[y, x] == 1) Score++;
                    else if (dasFeld[y, x] == 2 || dasFeld[y, x] == 4) Score += 4;
                    else if (dasFeld[y, x] == 3) Score += 7;
                    else if (dasFeld[y, x] == 5) Score += 15;
                }
            }
            return Score;
        }

        public static int Safety(byte[,] dasFeld)
        {
            int[] kingpos = getKingpos(dasFeld, true);
            int safety = 0;
            if (!kingpos.Contains(10))
            {
                for (int i = 0; i < 3; i++)
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
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
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

        public static int Bauern(byte[,] dasFeld)
        {
            int[,] bauernpos = getBauernpos(dasFeld, true);
            int bauernscore = 0;
            for (int i = 0; i < 8; i++)
            {
                if (bauernpos[i, 0] != 10)
                {
                    bauernscore += bauernpos[i, 1];
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
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
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
    }
}
