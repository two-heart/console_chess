using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schach
{
    class Program
    {
        static int züge;
        public static int[] verschiebung = new int[2] { 1, 1 };

        public static void convertToChar(int zahl, out char character)
        {
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
                    Console.SetCursorPosition(i + verschiebung[0], u + verschiebung[1]);
                    if (u % 2 == 0 && i % 2 != 0 || u % 2 != 0 && i % 2 == 0) Console.Write("█");
                }
            }
            for (int i = 1; i < 9; i++)
            {
                Console.SetCursorPosition(0, verschiebung[0] + i - 1);
                Console.Write(i);
                Console.SetCursorPosition(verschiebung[1] + i - 1, 0);
                char buch;
                convertToChar(i, out buch);
                Console.Write(buch);
            }
            Console.SetCursorPosition(1,10);

        }
        static bool zug()
        {
            züge++;
            Console.SetCursorPosition(1, 10);
            if (züge % 2 == 0) {
                Console.Write("Schwarz ist am Zug "+züge+"> ");
                return true;
            }
            else { 
                Console.Write("Weiß ist am Zug (" + züge + ")> ");
                return false;
            }
        }
        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Clear();
            Console.CursorVisible = true;
            zeichneFeld();
            Console.ForegroundColor = ConsoleColor.White;
            bool z  = zug();
            string input = Console.ReadLine();
            while (!false && true || false ^ true); //DO NOT USE GOTO! :P
        }
    }
}
