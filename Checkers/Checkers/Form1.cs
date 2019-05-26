using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace Checkers
{
    public partial class Form1 : Form
    {
        #region variables and such
        int size = 8,
            buttonSize,
            turn = 1,
            diffOffset = 1,
            difficulty = 2,
            undoDepth = 0;

        const int BOARD_DIMENSIONS = 600,
            SETTINGS_WIDTH = 150,
            MARGINS = 10,
            BUTTON_HEIGHT = 60,
            MAX_SIZE = 12,
            MIN_SIZE = 7,
            MAX_UNDO = 50;

        int[] pieceToMove = new int[2];

        int[,] numBoard;

        bool PvP = false;

        Color formBack = Color.FromArgb(20, 10, 2),
            panelBack = Color.FromArgb(20, 10, 2),
            whiteColor = Color.FromArgb(240, 210, 200),
            blackColor = Color.FromArgb(85, 45, 20),
            blueColor = Color.Navy,
            buttonColor = Color.FromArgb(15, 10, 10),
            buttonColorLight = Color.FromArgb(40, 5, 5),
            disabledColor = Color.DarkRed,
            selectedColor = Color.FromArgb(95, 50, 25);

        Button PvPButton = new Button(),
            PvCButton = new Button(),
            UndoButton = new Button();

        Button[] difficultyButtons = new Button[3];

        Button[,] board;

        NumericUpDown sizeSetter = new NumericUpDown();

        Label sizeText = new Label(),
            thinking = new Label();

        Image blackPiece = Properties.Resources.brown_piece,
            whitePiece = Properties.Resources.orange_piece,
            blackQueen = Properties.Resources.brown_queen,
            whiteQueen = Properties.Resources.orange_queen;

        Image[] images = new Image[5];

        Move computerMove;

        Move[] undos;

        #endregion

        #region my functions
        private void Delete_Board()
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    panel1.Controls.Remove(board[x, y]);
                    board[x, y].Dispose();
                }
            }
        }

        private void Enable_Undo()
        {
            UndoButton.Enabled = true;
            UndoButton.BackColor = Color.NavajoWhite;
        }

        private void Disable_Undo()
        {
            UndoButton.Enabled = false;
            UndoButton.BackColor = disabledColor;
        }

        private void Create_Board()
        {
            board = new Button[size, size];
            numBoard = new int[size, size];
            buttonSize = BOARD_DIMENSIONS / size;

            int idCounter = 0;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    board[x, y] = new Button();
                    board[x, y].Location = new Point(x * buttonSize, y * buttonSize);
                    board[x, y].Size = new Size(buttonSize, buttonSize);
                    board[x, y].Tag = idCounter.ToString();
                    board[x, y].Click += Single_Button_Click;
                    board[x, y].FlatStyle = FlatStyle.Flat;
                    if (y % 2 != x % 2)
                        board[x, y].BackColor = whiteColor;
                    else
                        board[x, y].BackColor = blackColor;

                    panel1.Controls.Add(board[x, y]);
                    idCounter++;
                }
            }
        }

        private void Place_Piece(Image image, int x, int y)
        {
            board[x, y].BackgroundImage = image;
            board[x, y].BackgroundImageLayout = ImageLayout.Stretch;
            numBoard[x, y] = Array.IndexOf(images, image);
        }

        private int[] Queen_Moves(int x, int y, int turn)
        {
            int[] moves = new int[0];
            int index = 0;

            // up left
            int ty = y, tx = x;
            while (tx > 0 && ty > 0 && numBoard[tx - 1, ty - 1] != turn && numBoard[tx - 1, ty - 1] != turn + 2)
            {
                tx--;
                ty--;
                // if the tile is taken by the opponent 
                if (numBoard[tx, ty] != 0)
                {
                    ty -= 1;
                    tx -= 1;
                    // checking for the tile at the other end of the opponent's piece
                    if (tx >= 0 && ty >= 0 && numBoard[tx, ty] == 0)
                    {
                        Array.Resize(ref moves, moves.Length + 2);
                        moves[index] = tx;
                        moves[index + 1] = ty;
                        index += 2;
                    }
                    break;
                }
              
                // if its empty
                Array.Resize(ref moves, moves.Length + 2);
                moves[index] = tx;
                moves[index + 1] = ty;
                index += 2;
            }

            // up right
            ty = y;
            tx = x;
            while (tx < size - 1 && ty > 0 && numBoard[tx + 1, ty - 1] != turn && numBoard[tx + 1, ty - 1] != turn + 2)
            {
                tx++;
                ty--;
                // if the tile is taken by the opponent 
                if (numBoard[tx, ty] != 0)
                {
                    ty -= 1;
                    tx += 1;
                    // checking for the tile at the other end of the opponent's piece
                    if (tx < size && ty >= 0 && numBoard[tx, ty] == 0)
                    {
                        Array.Resize(ref moves, moves.Length + 2);
                        moves[index] = tx;
                        moves[index + 1] = ty;
                        index += 2;
                    }
                    break;
                }
                
                // if its empty
                Array.Resize(ref moves, moves.Length + 2);
                moves[index] = tx;
                moves[index + 1] = ty;
                index += 2;
            }

            // down left
            ty = y;
            tx = x;
            while (tx > 0 && ty < size - 1 && numBoard[tx - 1, ty + 1] != turn && numBoard[tx - 1, ty + 1] != turn + 2)
            {
                tx--;
                ty++;
                // if the tile is taken by the opponent 
                if (numBoard[tx, ty] != 0)
                {
                    ty += 1;
                    tx -= 1;
                    // checking for the tile at the other end of the opponent's piece
                    if (tx >= 0 && ty < size && numBoard[tx, ty] == 0)
                    {
                        Array.Resize(ref moves, moves.Length + 2);
                        moves[index] = tx;
                        moves[index + 1] = ty;
                        index += 2;
                    }
                    break;
                }
              
                // if its empty
                Array.Resize(ref moves, moves.Length + 2);
                moves[index] = tx;
                moves[index + 1] = ty;
                index += 2;
            }

            // down right
            ty = y;
            tx = x;
            while (tx < size - 1 && ty < size - 1 && numBoard[tx + 1, ty + 1] != turn && numBoard[tx + 1, ty + 1] != turn + 2)
            {
                tx++;
                ty++;
                // if the tile is taken by the opponent 
                if (numBoard[tx, ty] != 0)
                {
                    ty += 1;
                    tx += 1;
                    // checking for the tile at the other end of the opponent's piece
                    if (tx < size && ty < size && numBoard[tx, ty] == 0)
                    {
                        Array.Resize(ref moves, moves.Length + 2);
                        moves[index] = tx;
                        moves[index + 1] = ty;
                        index += 2;
                    }
                    break;
                }
               
                // if its empty
                Array.Resize(ref moves, moves.Length + 2);
                moves[index] = tx;
                moves[index + 1] = ty;
                index += 2;
            }

            return moves;
        }

        private int[] Moves(int x, int y, string dir, int turn)
        {
            int[] moves = new int[0];
            int index = 0;

            if (dir == "down")
            {
                // down right
                if (x < size - 1 && y < size - 1 && numBoard[x + 1, y + 1] == 0)
                {
                    Array.Resize(ref moves, moves.Length + 2);
                    moves[index] = x + 1;
                    moves[index + 1] = y + 1;
                    index += 2;
                }
                else if (x < size - 1 && y < size - 1 && numBoard[x + 1, y + 1] != 0 && numBoard[x + 1, y + 1] != turn && numBoard[x + 1, y + 1] != turn + 2)
                {
                    if (x < size - 2 && y < size - 2 && numBoard[x + 2, y + 2] == 0)
                    {
                        Array.Resize(ref moves, moves.Length + 2);
                        moves[index] = x + 2;
                        moves[index + 1] = y + 2;
                        index += 2;
                    }
                }

                // down left
                if (x > 0 && y < size - 1 && numBoard[x - 1, y + 1] == 0)
                {
                    Array.Resize(ref moves, moves.Length + 2);
                    moves[index] = x - 1;
                    moves[index + 1] = y + 1;
                    index += 2;
                }
                else if (x > 0 && y < size - 1 && numBoard[x - 1, y + 1] != 0 && numBoard[x - 1, y + 1] != turn && numBoard[x - 1, y + 1] != turn + 2)
                {
                    if (x > 1 && y < size - 2 && numBoard[x - 2, y + 2] == 0)
                    {
                        Array.Resize(ref moves, moves.Length + 2);
                        moves[index] = x - 2;
                        moves[index + 1] = y + 2;
                        index += 2;
                    }
                }
            }

            else
            {
                // up right
                if (x < size - 1 && y > 0 && numBoard[x + 1, y - 1] == 0)
                {
                    Array.Resize(ref moves, moves.Length + 2);
                    moves[index] = x + 1;
                    moves[index + 1] = y - 1;
                    index += 2;
                }
                else if (x < size - 1 && y > 0 && numBoard[x + 1, y - 1] != 0 && numBoard[x + 1, y - 1] != turn && numBoard[x + 1, y - 1] != turn + 2)
                {
                    if (x < size - 2 && y > 1 && numBoard[x + 2, y - 2] == 0)
                    {
                        Array.Resize(ref moves, moves.Length + 2);
                        moves[index] = x + 2;
                        moves[index + 1] = y - 2;
                        index += 2;
                    }
                }

                // up left
                if (x > 0 && y > 0 && numBoard[x - 1, y - 1] == 0)
                {
                    Array.Resize(ref moves, moves.Length + 2);
                    moves[index] = x - 1;
                    moves[index + 1] = y - 1;
                    index += 2;
                }
                else if (x > 0 && y > 0 && numBoard[x - 1, y - 1] != 0 && numBoard[x - 1, y - 1] != turn && numBoard[x - 1, y - 1] != turn + 2)
                {
                    if (x > 1 && y > 1 && numBoard[x - 2, y - 2] == 0)
                    {
                        Array.Resize(ref moves, moves.Length + 2);
                        moves[index] = x - 2;
                        moves[index + 1] = y - 2;
                        index += 2;
                    }
                }
            }
            return moves;
        }

        private void Clean_Colors()
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (y % 2 != x % 2)
                        board[x, y].BackColor = whiteColor;
                    else
                        board[x, y].BackColor = blackColor;
                }
            }
        }

        private void Apply_Move(Move move)
        {
            numBoard[move.startX, move.startY] = 0;
            if (move.queen)
                numBoard[move.endX, move.endY] = move.type + 2;
            else
                numBoard[move.endX, move.endY] = move.type;
            if (move.eatenType != 0)
                numBoard[move.eatenX, move.eatenY] = 0;
        }

        private void Reverse_Move(Move move)
        {
            numBoard[move.startX, move.startY] = move.type;
            numBoard[move.endX, move.endY] = 0;
            if (move.eatenType != 0)
                numBoard[move.eatenX, move.eatenY] = move.eatenType;
        }

        private int[,] GetCloneBoard(int[,] numBoard)
        {
            int[,] newBoard = new int[size, size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    newBoard[x, y] = numBoard[x, y];
                }
            }

            return newBoard;
        }

        private int Score(int[,] board)
        {
            int score = 0;

            int winState = Is_Win();
            if (winState == 1)
                return -10000;
            else if (winState == 2)
                return 10000;
            else if (winState == 3)
                return 0;

            else
            {
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        if (board[x, y] == 1)
                        {
                            score -= 20;
                            if (y == size - 1)
                                score -= 25;
                            score -= size - 1 - y;
                        }
                        else if (board[x, y] == 2)
                        {
                            score += 20;
                            if (y == 0)
                                score += 25;
                            score += y;
                        }
                        else if (board[x, y] == 3)
                            score -= 200;
                        else if (board[x, y] == 4)
                            score += 200;
                    }
                }
            }

            return score;
        }

        private void Show_Me(int[,] numBoard, string title)
        {
            string showMe = "";
            showMe += title + "\n";
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    showMe += numBoard[x, y].ToString();
                }
                showMe += "\n";
            }
            MessageBox.Show(showMe);
        }

        private void Show_Me(int[,] numBoard, int[,] numBoardTwo)
        {
            string showMe = "";
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    showMe += numBoard[x, y].ToString();
                }
                showMe += "\n";
            }
            showMe += "\n" + "\n";

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    showMe += numBoardTwo[x, y].ToString();
                }
                showMe += "\n";
            }

            MessageBox.Show(showMe);
        }

        //private int[,] Comp_Turn(int depth, int side)
        //{
        //    int[,] wantedBoard = numBoard;
        //    int edgeScore = 0;
        //    bool first = true;

        //    int[] prev;
        //    int[] eatenPiece;
        //    int[] moves;
        //    bool eaten;
        //    for (int y = 0; y < size; y++)
        //    {
        //        for (int x = 0; x < size; x++)
        //        {
        //            #region collecting possible moves
        //            if (side == 2)
        //            {
        //                if (numBoard[x, y] == 2)
        //                    moves = Moves(x, y, "down", 2);
        //                else if (numBoard[x, y] == 4)
        //                    moves = Queen_Moves(x, y, 2);
        //                else
        //                    continue;
        //            }
        //            else
        //            {
        //                if (numBoard[x, y] == 1)
        //                    moves = Moves(x, y, "up", 1);
        //                else if (numBoard[x, y] == 3)
        //                    moves = Queen_Moves(x, y, 1);
        //                else
        //                    continue;
        //            }
        //            #endregion

        //            // going over all the moves
        //            for (int i = 0; i < moves.Length; i += 2)
        //            {
        //                #region moving the piece according to move
        //                prev = new int[] { numBoard[x, y], numBoard[moves[i], moves[i + 1]]};
        //                eatenPiece = new int[3];
        //                eaten = false;
        //                numBoard[moves[i], moves[i + 1]] = numBoard[x, y];
        //                //queen promotion
        //                if (numBoard[x, y] == 2 && moves[i + 1] == size - 1)
        //                {
        //                    numBoard[moves[i], moves[i + 1]] = 4;
        //                }
        //                else if (numBoard[x, y] == 1 && moves[i + 1] == 0)
        //                {
        //                    numBoard[moves[i], moves[i + 1]] = 3;
        //                }
        //                numBoard[x, y] = 0;
        //                #endregion

        //                #region removing eaten piece if there is one
        //                if (Math.Abs(x - moves[i]) >= 2)
        //                {
        //                    if (x - moves[i] == y - moves[i + 1])
        //                    {
        //                        for (int tx = Math.Min(x, moves[i]) + 1, ty = Math.Min(y, moves[i + 1]) + 1; tx < Math.Max(x, moves[i]); tx++, ty++)
        //                        {
        //                            if (numBoard[tx, ty] != 0)
        //                            {
        //                                eatenPiece[0] = tx;
        //                                eatenPiece[1] = ty;
        //                                eatenPiece[2] = numBoard[tx, ty];
        //                                eaten = true;
        //                                numBoard[tx, ty] = 0;
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        for (int tx = Math.Min(x, moves[i]) + 1, ty = Math.Max(y, moves[i + 1]) - 1; tx < Math.Max(x, moves[i]); tx++, ty--)
        //                        {
        //                            if (numBoard[tx, ty] != 0)
        //                            {
        //                                eatenPiece[0] = tx;
        //                                eatenPiece[1] = ty;
        //                                eatenPiece[2] = numBoard[tx, ty];
        //                                eaten = true;
        //                                numBoard[tx, ty] = 0;
        //                            }
        //                        }
        //                    }                                        
        //                }
        //                #endregion

        //                #region deciding the score for the board
        //                int score = 0;
        //                if (depth > 1)
        //                {
        //                    if (side == 1)
        //                    {
        //                        score = Score(Comp_Turn(depth - 1, 2));
        //                        Show_Me(numBoard, "DEPTH: " + depth.ToString() + "  SCORE: " + score.ToString());
        //                    }
        //                    else
        //                    {
        //                        score = Score(Comp_Turn(depth - 1, 1));
        //                        Show_Me(numBoard, "DEPTH: " + depth.ToString() + "  SCORE: " + score.ToString());
        //                    }
        //                }
        //                else
        //                {
        //                    score = Score(numBoard);
        //                    Show_Me(numBoard, "DEPTH: "  + depth.ToString() + "  SCORE:  " + score.ToString());
        //                }
        //                #endregion

        //                #region calculating and changing highscores
        //                if (side == 2)
        //                {
        //                    // change edge score if needed
        //                    if (score > edgeScore || first)
        //                    {
        //                        edgeScore = score;
        //                        wantedBoard = GetCloneBoard(numBoard);
        //                        first = false;
        //                    }
        //                }
        //                else
        //                {
        //                    // change edge score if needed
        //                    if (score < edgeScore || first)
        //                    {
        //                        edgeScore = score;
        //                        wantedBoard = GetCloneBoard(numBoard);
        //                        first = false;
        //                    }
        //                }
        //                #endregion

        //                #region restoring the board
        //                numBoard[x, y] = prev[0];
        //                numBoard[moves[i],moves[i + 1]] = prev[1];
        //                if (eaten)
        //                    numBoard[eatenPiece[0], eatenPiece[1]] = eatenPiece[2];
        //                #endregion
        //            }
        //        }
        //    }
        //    return wantedBoard;
        //}

        private Move Mini_Max()
        {
            return Max_Move(difficulty + diffOffset);
        }
        private Move Max_Move(int depth)
        {
            bool first = true;
            int highScore = 0, score;
            int[] moves = null;
            Move bestMove = null;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (numBoard[x, y] == 2)
                        moves = Moves(x, y, "down", 2);
                    else if (numBoard[x, y] == 4)
                        moves = Queen_Moves(x, y, 2);
                    else
                        continue;

                    for (int i = 0; i < moves.Length; i += 2)
                    {
                        #region making a move
                        Move move = new Move(x, y, moves[i], moves[i + 1], numBoard[x, y]);
                        #region eating stuff
                        if (Math.Abs(x - moves[i]) >= 2)
                        {
                            if (moves[i] - x == moves[i + 1] - y)
                            {
                                for (int tx = Math.Min(moves[i], x) + 1, ty = Math.Min(moves[i + 1], y) + 1; tx < Math.Max(moves[i], x); tx++, ty++)
                                {
                                    if (numBoard[tx, ty] != 0)
                                    {
                                        move.eatenX = tx;
                                        move.eatenY = ty;
                                        move.eatenType = numBoard[tx, ty];
                                    }
                                }
                            }
                            else
                            {
                                for (int tx = Math.Min(moves[i], x) + 1, ty = Math.Max(moves[i + 1], y) - 1; tx < Math.Max(moves[i], x); tx++, ty--)
                                {
                                    if (numBoard[tx, ty] != 0)
                                    {
                                        move.eatenX = tx;
                                        move.eatenY = ty;
                                        move.eatenType = numBoard[tx, ty];
                                    }
                                }
                            }
                        }
                        #endregion
                        #region queen promotion
                        if (numBoard[x, y] == 2 && moves[i + 1] == size - 1)
                        {
                            move.queen = true;
                        }
                        #endregion
                        move.type = numBoard[move.startX, move.startY];

                        Apply_Move(move);
                        #endregion

                        if (depth > 1)
                        {
                            Move counter = Min_Move(depth - 1);
                            if (counter != null)
                            {
                                Apply_Move(counter);
                                score = Score(numBoard);
                                Reverse_Move(counter);
                            }
                            else
                                score = Score(numBoard);
                        }
                        else
                            score = Score(numBoard);

                        if (score > highScore || first)
                        {
                            first = false;
                            highScore = score;
                            bestMove = move;
                        }
                        Reverse_Move(move);
                    }
                }
            }
            return bestMove;
        }
        private Move Min_Move(int depth)
        {
            bool first = true;
            int lowScore = 0, score;
            int[] moves = null;
            Move bestMove = null;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (numBoard[x, y] == 1)
                        moves = Moves(x, y, "up", 1);
                    else if (numBoard[x, y] == 3)
                        moves = Queen_Moves(x, y, 1);
                    else
                        continue;

                    for (int i = 0; i < moves.Length; i += 2)
                    {
                        #region making a move
                        Move move = new Move(x, y, moves[i], moves[i + 1], numBoard[x, y]);
                        #region eating stuff
                        if (Math.Abs(x - moves[i]) >= 2)
                        {
                            if (moves[i] - x == moves[i + 1] - y)
                            {
                                for (int tx = Math.Min(moves[i], x) + 1, ty = Math.Min(moves[i + 1], y) + 1; tx < Math.Max(moves[i], x); tx++, ty++)
                                {
                                    if (numBoard[tx, ty] != 0)
                                    {
                                        move.eatenX = tx;
                                        move.eatenY = ty;
                                        move.eatenType = numBoard[tx, ty];
                                    }
                                }
                            }
                            else
                            {
                                for (int tx = Math.Min(moves[i], x) + 1, ty = Math.Max(moves[i + 1], y) - 1; tx < Math.Max(moves[i], x); tx++, ty--)
                                {
                                    if (numBoard[tx, ty] != 0)
                                    {
                                        move.eatenX = tx;
                                        move.eatenY = ty;
                                        move.eatenType = numBoard[tx, ty];
                                    }
                                }
                            }
                        }
                        #endregion
                        #region queen promotion
                        if (numBoard[x, y] == 1 && moves[i + 1] == 0)
                        {
                            move.queen = true;
                        }
                        #endregion
                        move.type = numBoard[move.startX, move.startY];

                        Apply_Move(move);
                        #endregion

                        if (depth > 1)
                        {
                            Move counter = Max_Move(depth - 1);
                            if (counter != null)
                            {
                                Apply_Move(counter);
                                score = Score(numBoard);
                                Reverse_Move(counter);
                            }
                            else
                                score = Score(numBoard);
                        }
                        else
                            score = Score(numBoard);


                        if (score < lowScore || first)
                        {
                            first = false;
                            lowScore = score;
                            bestMove = move;
                        }
                        Reverse_Move(move);
                    }
                }
            }
            return bestMove;
        }

        private int Is_Win()
        {
            bool whiteOptions = false,
                blackOptions = false;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (!whiteOptions & numBoard[x, y] == 1)
                    {
                        if (Moves(x, y, "up", 1).Length > 0)
                        {
                            whiteOptions = true;
                        }
                    }
                    if (!blackOptions & numBoard[x, y] == 2)
                    {
                        if (Moves(x, y, "down", 2).Length > 0)
                        {
                            blackOptions = true;
                        }
                    }
                    if (!whiteOptions & numBoard[x, y] == 3)
                    {
                        if (Queen_Moves(x, y, 1).Length > 0)
                        {
                            whiteOptions = true;
                        }
                    }
                    if (!blackOptions & numBoard[x, y] == 4)
                    {
                        if (Queen_Moves(x, y, 2).Length > 0)
                        {
                            blackOptions = true;
                        }
                    }
                    
                }
            }
            if (blackOptions && !whiteOptions)
                return 2;
            else if (!blackOptions & whiteOptions)
                return 1;
            else if (!blackOptions && !whiteOptions)
                return 3;
            return 0;
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public Form1()
        {
            InitializeComponent();
            this.Size = new Size(BOARD_DIMENSIONS + SETTINGS_WIDTH + 14, BOARD_DIMENSIONS + 38);
            this.BackColor = formBack;

            panel1.Location = new Point(SETTINGS_WIDTH, 0);
            panel1.Size = new Size(BOARD_DIMENSIONS, BOARD_DIMENSIONS);

            PvPButton.Location = new Point(MARGINS, MARGINS);
            PvPButton.Size = new Size(SETTINGS_WIDTH - MARGINS * 2, BUTTON_HEIGHT);
            PvPButton.BackColor = buttonColor;
            PvPButton.ForeColor = Color.White;
            PvPButton.Text = "PvP";
            PvPButton.Font = new Font(PvPButton.Font, FontStyle.Bold);
            PvPButton.Click += PvPButton_Click;
            this.Controls.Add(PvPButton);

            PvCButton.Location = new Point(MARGINS, BUTTON_HEIGHT + MARGINS * 4);
            PvCButton.Size = new Size(SETTINGS_WIDTH - MARGINS * 2, BUTTON_HEIGHT);
            PvCButton.BackColor = buttonColor;
            PvCButton.ForeColor = Color.White;
            PvCButton.Text = "PvC";
            PvCButton.Font = new Font(PvPButton.Font, FontStyle.Bold);
            PvCButton.Click += PvCButton_Click; ;
            this.Controls.Add(PvCButton);

            for (int i = 0; i < difficultyButtons.Length; i++)
            {
                difficultyButtons[i] = new Button();
                difficultyButtons[i].Location = new Point(MARGINS + (SETTINGS_WIDTH - MARGINS * 2) / difficultyButtons.Length * i, BUTTON_HEIGHT * 2 + MARGINS * 4);
                difficultyButtons[i].Size = new Size((SETTINGS_WIDTH - MARGINS * 2) / difficultyButtons.Length, BUTTON_HEIGHT / 2);
                difficultyButtons[i].BackColor = buttonColorLight;
                difficultyButtons[i].ForeColor = Color.White;
                difficultyButtons[i].Click += DifficultyChanged;
                this.Controls.Add(difficultyButtons[i]);
            }
            difficultyButtons[0].Text = "Noob";
            difficultyButtons[1].Text = "Meh";
            difficultyButtons[2].Text = "Pro";
            difficultyButtons[difficulty].BackColor = selectedColor;
            difficultyButtons[difficulty].ForeColor = Color.Black;

            sizeSetter.Location = new Point(MARGINS + 2, this.Height - MARGINS - 58);
            sizeSetter.Size = new Size(SETTINGS_WIDTH - MARGINS * 2 - 4, 0);
            sizeSetter.BackColor = buttonColor;
            sizeSetter.ForeColor = Color.White;
            sizeSetter.Value = size;
            sizeSetter.Maximum = MAX_SIZE;
            sizeSetter.Minimum = MIN_SIZE;
            sizeSetter.ValueChanged += sizeSetter_ValueChanged;
            this.Controls.Add(sizeSetter);

            sizeText.Text = "Size:";
            sizeText.Location = new Point(SETTINGS_WIDTH / 2 - 15, this.Height - 83);
            sizeText.ForeColor = Color.White;
            this.Controls.Add(sizeText);

            UndoButton.Location = new Point(MARGINS * 3, this.Height - 125);
            UndoButton.Size = new Size(SETTINGS_WIDTH - MARGINS * 6, BUTTON_HEIGHT / 2);
            UndoButton.ForeColor = Color.Black;
            UndoButton.Text = "Undo";
            UndoButton.Font = new Font(PvPButton.Font, FontStyle.Bold);
            UndoButton.Click += UndoButton_Click;
            Disable_Undo();
            this.Controls.Add(UndoButton);

            thinking.Text = "Thinking...";
            thinking.Location = new Point(MARGINS * 2, this.Height / 2);
            thinking.Size = new Size(SETTINGS_WIDTH, 50);
            thinking.Font = new Font("Arial", 14, FontStyle.Bold);
            thinking.BackColor = BackColor;
            thinking.ForeColor = Color.White;
            images[0] = null;
            images[1] = whitePiece;
            images[2] = blackPiece;
            images[3] = whiteQueen;
            images[4] = blackQueen;

            undos = new Move[MAX_UNDO];

            Create_Board();
        }

        private void UndoButton_Click(object sender, EventArgs e)
        {
            if (undoDepth > 0)
            {
                undoDepth -= 1;
                Move move = undos[undoDepth];
                Place_Piece(images[move.type], move.startX, move.startY);
                Place_Piece(images[0], move.endX, move.endY);
                if (Math.Abs(move.startX - move.endX) >= 2)
                    Place_Piece(images[move.eatenType], move.eatenX, move.eatenY);
                Clean_Colors();
                if (turn == 2)
                    turn = 1;
                else
                    turn = 2;
                if (undoDepth == 0)
                {
                    Disable_Undo();
                }
            }
        }

        private void PvCButton_Click(object sender, EventArgs e)
        {
            Delete_Board();
            Create_Board();
            turn = 1;

            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (board[x, y].BackColor == whiteColor)
                        Place_Piece(blackPiece, x, y);
                }
            }
            for (int y = size - 3; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (board[x, y].BackColor == whiteColor)
                        Place_Piece(whitePiece, x, y);
                }
            }
            PvP = false;

            PvCButton.BackColor = selectedColor;
            PvCButton.ForeColor = Color.Black;
            PvPButton.BackColor = buttonColor;
            PvPButton.ForeColor = Color.White;

            Disable_Undo();
            undoDepth = 0;
        }

        private void DifficultyChanged(object sender, EventArgs e)
        {
            difficulty = Array.IndexOf(difficultyButtons, (Button)sender);
            for (int i = 0; i < difficultyButtons.Length; i++)
            {
                if (i == difficulty)
                {
                    difficultyButtons[i].BackColor = selectedColor;
                    difficultyButtons[i].ForeColor = Color.Black;
                }

                else
                {
                    difficultyButtons[i].BackColor = buttonColorLight;
                    difficultyButtons[i].ForeColor = Color.White;
                }
            }
        }

        private void sizeSetter_ValueChanged(object sender, EventArgs e)
        {
            Delete_Board();
            size = (int)sizeSetter.Value;
            Create_Board();
            undos = new Move[MAX_UNDO];
        }

        private void Single_Button_Click(object sender, EventArgs e)
        {
            #region recognizing thee pressed button
            int id = int.Parse(((Button)(sender)).Tag.ToString());
            int bx = id % size;
            int by = id / size;
            #endregion

            #region if the player's own piece was clicked
            if (numBoard[bx, by] == turn || numBoard[bx, by] == turn + 2)
            {
                pieceToMove[0] = bx;
                pieceToMove[1] = by;

                Clean_Colors();

                board[bx, by].BackColor = blueColor;

                int[] blues;

                // checking for move options
                if (turn == 1)
                {
                    if (numBoard[bx, by] == 1)
                        blues = Moves(bx, by, "up", 1);
                    else
                        blues = Queen_Moves(bx, by, 1);
                }
                else
                    if (numBoard[bx, by] == 2)
                    blues = Moves(bx, by, "down", 2);
                else
                    blues = Queen_Moves(bx, by, 2);

                // painting the options blue
                if (blues.Length > 0)
                {
                    for (int i = 0; i < blues.Length; i += 2)
                    {
                        board[blues[i], blues[i + 1]].BackColor = blueColor;
                    }
                }

            }
            #endregion

            #region if a blue target tile was clicked
            // if the tile clicked is empty and blue
            if (numBoard[bx, by] == 0 && board[bx, by].BackColor == blueColor)
            {

                #region saving move for undoing
                if (undoDepth == MAX_UNDO)
                {
                    for (int i = 0; i < MAX_UNDO - 1; i++)
                    {
                        undos[i] = undos[i + 1];
                    }
                    undoDepth -= 1;
                }

                undos[undoDepth] = new Move(
                    pieceToMove[0],
                    pieceToMove[1],
                    bx,
                    by,
                    numBoard[pieceToMove[0], pieceToMove[1]]);
                Enable_Undo();
                #endregion

                Place_Piece(images[numBoard[pieceToMove[0], pieceToMove[1]]], bx, by);
                Place_Piece(null, pieceToMove[0], pieceToMove[1]);

                #region eating shenanigans
                if (Math.Abs(bx - pieceToMove[0]) >= 2)
                {
                    if (bx - pieceToMove[0] == by - pieceToMove[1])
                    {
                        for (int tx = Math.Min(bx, pieceToMove[0]) + 1, ty = Math.Min(by, pieceToMove[1]) + 1; tx < Math.Max(bx, pieceToMove[0]); tx++, ty++)
                        {
                            if (numBoard[tx, ty] != 0)
                            {
                                undos[undoDepth].eatenType = numBoard[tx, ty];
                                undos[undoDepth].eatenX = tx;
                                undos[undoDepth].eatenY = ty;
                                Place_Piece(images[0], tx, ty);
                            }
                        }
                    }
                    else
                    {
                        for (int tx = Math.Min(bx, pieceToMove[0]) + 1, ty = Math.Max(by, pieceToMove[1]) - 1; tx < Math.Max(bx, pieceToMove[0]); tx++, ty--)
                        {
                            if (undoDepth - 1 < MAX_UNDO && numBoard[tx, ty] != 0)
                            {
                                undos[undoDepth].eatenType = numBoard[tx, ty];
                                undos[undoDepth].eatenX = tx;
                                undos[undoDepth].eatenY = ty;
                                Place_Piece(images[0], tx, ty);
                            }
                        }
                    }
                }
                #endregion

                #region queen promotion
                // white queen promotion if need be
                if (turn == 1 && by == 0 && numBoard[bx, by] == 1)
                {
                    Place_Piece(images[3], bx, by);
                }

                // black queen promotion if need be
                if (turn == 2 && by == size - 1 && numBoard[bx, by] == 2)
                {
                    Place_Piece(images[4], bx, by);
                }
                #endregion

                #region changing turn if PvP
                if (PvP || (!PvP & turn == 2))
                {
                    if (turn == 1)
                        turn = 2;
                    else
                        turn = 1;
                }
                #endregion

                #region doing computer move if PvC
                else if (!PvP & turn == 1)
                {
                    Clean_Colors();
                    panel1.Refresh();
                    //Thread.Sleep(250);

                    #region calculating and executing computer move
                    this.Controls.Add(thinking);
                    this.Refresh();
                    computerMove = Mini_Max();
                    this.Controls.Remove(thinking);

                    if (computerMove != null)
                    {
                        if (computerMove.queen)
                            Place_Piece(images[computerMove.type + 2], computerMove.endX, computerMove.endY);
                        else
                            Place_Piece(images[computerMove.type], computerMove.endX, computerMove.endY);
                        Place_Piece(images[0], computerMove.startX, computerMove.startY);
                        if (computerMove.eatenType != 0)
                            Place_Piece(images[0], computerMove.eatenX, computerMove.eatenY);
                    }
                    #endregion

                    #region saving for undo
                    undoDepth += 1;
                    if (undoDepth == MAX_UNDO)
                    {
                        for (int i = 0; i < MAX_UNDO - 1; i++)
                        {
                            undos[i] = undos[i + 1];
                        }
                        undoDepth -= 1;
                    }

                    undos[undoDepth] = computerMove;
                    #endregion
                }
                undoDepth += 1;
                Clean_Colors();
                #endregion

                #region winning
                if (!PvP)
                    turn = 2;

                int winState = Is_Win();
                if (winState == 1 && turn == 2)
                {
                    MessageBox.Show("WhItE wOn!");
                    Delete_Board();
                    size = (int)sizeSetter.Value;
                    Create_Board();
                    undos = new Move[MAX_UNDO];
                    undoDepth = 0;
                    Disable_Undo();
                }
                else if (winState == 2 && turn == 1)
                {
                    MessageBox.Show("BlAcK wOn!");
                    Delete_Board();
                    size = (int)sizeSetter.Value;
                    Create_Board();
                    undos = new Move[MAX_UNDO];
                    undoDepth = 0;
                    Disable_Undo();
                }
                else if (winState == 2 && !PvP)
                {
                    MessageBox.Show("BlAcK wOn!");
                    Delete_Board();
                    size = (int)sizeSetter.Value;
                    Create_Board();
                    undos = new Move[MAX_UNDO];
                    undoDepth = 0;
                    Disable_Undo();
                }
                else if (winState == 3)
                {
                    MessageBox.Show("ThE gAmE eNdEd In A dRaW!");
                    Delete_Board();
                    size = (int)sizeSetter.Value;
                    Create_Board();
                    undos = new Move[MAX_UNDO];
                    undoDepth = 0;
                    Disable_Undo();
                }
                if (!PvP)
                    turn = 1;
                #endregion
            }
            #endregion
        }

        private void PvPButton_Click(object sender, EventArgs e)
        {
            Delete_Board();
            Create_Board();
            turn = 1;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Place_Piece(null, x, y);
                }
            }

            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (board[x, y].BackColor == whiteColor)
                        Place_Piece(blackPiece, x, y);
                }
            }
            for (int y = size - 3; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (board[x, y].BackColor == whiteColor)
                        Place_Piece(whitePiece, x, y);
                }
            }
            PvP = true;

            PvPButton.BackColor = selectedColor;
            PvPButton.ForeColor = Color.Black;
            PvCButton.BackColor = buttonColor;
            PvCButton.ForeColor = Color.White;

            Disable_Undo();
            undoDepth = 0;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            panel1.Size = new Size(BOARD_DIMENSIONS, BOARD_DIMENSIONS);
            panel1.Location = new Point(SETTINGS_WIDTH, 0);
            panel1.BackColor = panelBack;
        }
    }
}
