using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace SeaBattleConsole
{
    internal class Board
    {
        private int playerHP, enemyHP, shipsPlaced;
        private bool turn, planning, placement, waiting, host, connected, lobby, getHostIP;
        private IPAddress hostIP, clientIP;
        private Cell[,] playerField = new Cell[10, 10];
        private Cell[,] enemyField = new Cell[10, 10];

        private void drawCell(Cell cell)
        {
            int status = cell.GetStatus();

            switch (status)
            {
                case 0:
                    Console.Write("#"); // fog
                    break;

                case 1:
                    Console.Write("_"); // empty unbombed
                    break;

                case 2:
                    Console.Write("*"); // empty bombed
                    break;

                case 3:
                    Console.Write("O"); // ship unbombed
                    break;

                case 4:
                    Console.Write("@"); // ship bombed
                    break;
            }
        }

        private int validCheck(string input)
        {
            int length = input.Length;

            switch (length)
            {
                case 1:
                    if (planning)
                        if (input == "R" || input == "r")
                            placement = !placement;
                    return -1;

                case 2:
                    {
                        int column = input[0];
                        int row = input[1];

                        bool columnValid = false;
                        bool rowValid = false;

                        if (column > 64 && column < 75)
                        {
                            columnValid = true;
                            column -= 65;
                        }
                        if (column > 96 && column < 107)
                        {
                            columnValid = true;
                            column -= 97;
                        }
                        if (row > 47 && row < 58)
                        {
                            rowValid = true;
                            row -= 48;
                        }

                        if (columnValid && rowValid)
                            return row * 10 + column;
                        else
                            return -1;
                    }
                default:
                    return -1;
            }
        }

        private bool placeShip(int row, int column, int length)
        {
            int lengthleft = length + 1;

            if (placement) // vertical
            {
                for (int j = column - 1; j < column + 2; j++)
                {
                    if (j <= -1 || j >= 10) // out of border, no need to check there
                        continue;

                    for (int i = row - 1; i < row + length + 1; i++)
                    {
                        if (i <= -1 || i >= 10) // out of border, no need to check there
                            continue;

                        if (playerField[i, j].GetShip())
                            return false;

                        lengthleft--;
                    }

                    if (lengthleft > 0) // ship can't fit here because of borders
                        return false;
                }
            }
            else // horizontal
            {
                for (int i = row - 1; i < row + 2; i++)
                {
                    if (i <= -1 || i >= 10) // out of border, no need to check there
                        continue;

                    for (int j = column - 1; j < column + length + 1; j++)
                    {
                        if (j <= -1 || j >= 10) // out of border, no need to check there
                            continue;

                        if (playerField[i, j].GetShip())
                            return false;

                        lengthleft--;
                    }

                    if (lengthleft > 0) // ship can't fit here because of borders
                        return false;
                }
            }

            for (int i = 0; i < length; i++)
            {
                playerField[row, column].PlaceShip();

                if (placement) // vertical
                    row++;
                else
                    column++; // horizontal
            }

            shipsPlaced++;
            return true;
        }

        private void afterPlan()
        {
            Console.Write("All ships dispatched, waiting for enemy...");
            while (true)
            {
                // TODO get enemy field data
            }
        }

        private void connectionSetup()
        {
            if (lobby)
            {
                Console.WriteLine("0 - Host");
                Console.WriteLine("1 - Join\n");
                string input = Console.ReadLine();

                if (input.Length != 1)
                    return;

                lobby = false;

                switch (input[0])
                {
                    case '0':
                        host = true;
                        turn = true;
                        break;

                    case '1':
                        host = false;
                        turn = false;
                        break;

                    default:
                        lobby = true;
                        break;
                }

                return;
            }

            if (!connected)
            {
                while (true)
                {
                    if (host)
                    {
                        Console.WriteLine("No opponent connection, wait...");
                        // TODO write server logic
                    }
                    else
                    {
                        if (getHostIP)
                        {
                            Console.WriteLine("Enter host IP:");

                            IPAddress ip;
                            bool ValidateIP = false;
                            string input;
                            while (!ValidateIP)
                            {
                                input = Console.ReadLine();
                                ValidateIP = IPAddress.TryParse(input, out ip);

                                if (ValidateIP)
                                {
                                    Console.WriteLine("This is a valide ip address");
                                    hostIP = IPAddress.Parse(input);
                                }
                                else
                                    Console.WriteLine("This is not a valide ip address");
                            }

                            getHostIP = false;
                        }
                        else
                        {
                            Console.WriteLine("Connecting to host...");
                            // TODO write client reconnect logic
                        }
                    }
                }
            }
        }

        private void plan()
        {
            Console.Write("Enter position for ");

            int length = 0;

            if (shipsPlaced == 0)
                length = 4;
            if (shipsPlaced > 0 && shipsPlaced < 3)
                length = 3;
            if (shipsPlaced > 2 && shipsPlaced < 6)
                length = 2;
            if (shipsPlaced > 5)
                length = 1;
            Console.Write(length);

            Console.Write("-cell ship (f. e. B4 or d0).\n");
            Console.Write("Current placement mode: ");
            if (placement)
                Console.Write("||");
            else
                Console.Write("═");
            Console.Write(" . Enter R to change mode.\n");

            string input = Console.ReadLine();
            int cache = validCheck(input);
            if (cache == -1)
                return;

            placeShip(cache / 10, cache % 10, length);

            if (shipsPlaced == 10)
                waiting = true;
        }

        private void drawLogo()
        {
            string text = @"   _____ ______            ____       _______ _______ _      ______
  / ____|  ____|   /\     |  _ \   /\|__   __|__   __| |    |  ____|
 | (___ | |__     /  \    | |_) | /  \  | |     | |  | |    | |__
  \___ \|  __|   / /\ \   |  _ < / /\ \ | |     | |  | |    |  __|
  ____) | |____ / ____ \  | |_) / ____ \| |     | |  | |____| |____
 |_____/|______/_/    \_\ |____/_/    \_\_|     |_|  |______|______|";
            Console.Write(text);
            Console.Write("\n\n");
        }

        private void drawUI()
        {
            Console.Clear(); // refresh console, clear old frame

            drawLogo();

            Console.WriteLine($"Player HP: {playerHP} \t\tEnemy HP: {enemyHP}\n"); // HPs information

            Console.WriteLine("  ABCDEFGHIJ\t\t  ABCDEFGHIJ");

            for (int i = 0; i < 10; i++) // rows
            {
                Console.Write(i);
                Console.Write(" ");

                for (int j = 0; j < 10; j++) // player columns
                    drawCell(playerField[i, j]);

                Console.Write("\t\t");

                Console.Write(i);
                Console.Write(" ");

                for (int j = 0; j < 10; j++) // enemy columns
                    drawCell(enemyField[i, j]);

                Console.Write("\n"); // end of the line (row)
            }

            Console.Write("\n");

            if (planning && !waiting)
                plan();

            if (planning && waiting)
                afterPlan();
        }

        private bool getBombed(int rowColumn)
        {
            int row = rowColumn / 10;
            int column = rowColumn % 10;
            if (playerField[row, column].Bomb())
            {
                playerHP--;
                return true;
            }

            return false;
        }

        private bool bomb(int rowColumn)
        {
            int row = rowColumn / 10;
            int column = rowColumn % 10;

            if (enemyField[row, column].GetBombed()) // you can't bomb the same spot 2 times
                return false;

            if (enemyField[row, column].Bomb())
            {
                int rowUp = row--;
                int rowDown = row++;
                int columnLeft = column--;
                int columnRight = column++;
                bool destroyedShip = true;
                if (destroyedShip)
                    for (rowUp = rowUp; rowUp > -1; rowUp--) // looking up for more undamaged parts of the same ship
                    {
                        if (!enemyField[rowUp, column].GetShip()) // check if there is even a ship
                            break;
                        if (enemyField[rowUp, column].GetShip() && !enemyField[rowUp, column].GetBombed())
                        {
                            destroyedShip = false;
                            break;
                        }
                    }

                if (destroyedShip)
                    for (rowDown = rowDown; rowDown < 10; rowDown++) // looking down for more undamaged parts of the same ship
                    {
                        if (!enemyField[rowDown, column].GetShip()) // check if there is even a ship
                            break;
                        if (enemyField[rowDown, column].GetShip() && !enemyField[rowDown, column].GetBombed())
                        {
                            destroyedShip = false;
                            break;
                        }
                    }

                if (destroyedShip)
                    for (columnLeft = columnLeft; columnLeft > -1; columnLeft--) // looking left for more undamaged parts of the same ship
                    {
                        if (!enemyField[row, columnLeft].GetShip()) // check if there is even a ship
                            break;
                        if (enemyField[row, columnLeft].GetShip() && !enemyField[row, columnLeft].GetBombed())
                        {
                            destroyedShip = false;
                            break;
                        }
                    }

                if (destroyedShip)
                    for (columnRight = columnRight; columnRight > -1; columnRight++) // looking right for more undamaged parts of the same ship
                    {
                        if (!enemyField[row, columnRight].GetShip()) // check if there is even a ship
                            break;
                        if (enemyField[row, columnRight].GetShip() && !enemyField[row, columnRight].GetBombed())
                        {
                            destroyedShip = false;
                            break;
                        }
                    }

                if (destroyedShip)
                    destructionReveal(rowColumn);

                enemyHP--;

                return true;
            }

            return false;
        }

        private void bubbleReveal(int rowColumn)
        {
            int row = rowColumn / 10;
            int column = rowColumn % 10;

            for (int i = row - 1; i < row + 2; i++) // this loop will proc 4, 6 or 9 times, depends on border proximity
            {
                if (i <= -1 || i >= 10) // out of border
                    continue;

                for (int j = column - 1; j < column + 2; j++)
                {
                    if (j <= -1 || j >= 10) // out of border
                        continue;

                    enemyField[i, j].Reveal();
                }
            }
        }

        private void destructionReveal(int rowColumn)
        {
            int row = rowColumn / 10;
            int column = rowColumn % 10;

            int rowUp = row--;
            int rowDown = row++;
            int columnLeft = column--;
            int columnRight = column++;
            for (rowUp = rowUp; rowUp > -1; rowUp--) // looking up for more parts of this ship
            {
                if (!enemyField[rowUp, column].GetShip()) // check if there is even a ship
                    break;
                bubbleReveal(rowColumn);
            }

            for (rowDown = rowDown; rowDown < 10; rowDown++) // looking down for more parts of this ship
            {
                if (!enemyField[rowDown, column].GetShip()) // check if there is even a ship
                    break;
                bubbleReveal(rowColumn);
            }

            for (columnLeft = columnLeft; columnLeft > -1; columnLeft--) // looking left for more parts of this ship
            {
                if (!enemyField[row, columnLeft].GetShip()) // check if there is even a ship
                    break;
                bubbleReveal(rowColumn);
            }

            for (columnRight = columnRight; columnRight > -1; columnRight++) // looking right for more parts of this ship
            {
                if (!enemyField[row, columnRight].GetShip()) // check if there is even a ship
                    break;
                bubbleReveal(rowColumn);
            }
        }

        public Board()
        {
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    playerField[i, j] = new Cell(false);
                    enemyField[i, j] = new Cell(true);
                }
            }

            shipsPlaced = 0;
            playerHP = 20;
            enemyHP = playerHP;

            placement = false;
            waiting = false;
            connected = false;
            lobby = true;
            planning = true;
            getHostIP = true;
        }

        public void start()
        {
            while (true)
            {
                drawUI(); // gameplay loop
            }
        }
    }
}