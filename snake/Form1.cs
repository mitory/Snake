using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace snake
{
    public partial class Form1 : Form
    {
        private string path = AppDomain.CurrentDomain.BaseDirectory + "file.dat";
        private static int timesScaleButton = 0, maxTimesScaleButton = 5;
        private static bool isAnimationUp = true;
        private static bool isNotFirstGame = false;
        private static bool isMoveed = false;
        private static int bestScore = 0;
        private static int sizeCell = 20;
        private static int sizeBord = 400;
        private GameBord gameBord = new GameBord();
        private Snake snake = new Snake();
        private PictureBox fruit = new PictureBox { Size = new Size(sizeCell, sizeCell), BackColor = Color.Red };
        public Form1()
        {
            InitializeComponent();
            try
            {
                using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
                {
                    while (reader.PeekChar() > -1)
                    {
                        bestScore = reader.ReadInt32();
                    }
                }
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show($"Не удалось найти файл по пути {path}");
            }
            if(bestScore > 0)
            {
                isNotFirstGame = true;
            }
            showMenu();
        }
        private void showMenu()
        {
            this.Controls.Clear();
            this.Controls.Add(button1);
            if (bestScore == sizeBord)
            {
                winLabel.Visible = true;
                this.Controls.Add(winLabel);
            }
            button1.Enabled = true;
            button1.Visible = true;
            timer.Tick -= update;
            timer.Tick += new EventHandler(updateMenu);
            timer.Start();
        }
        private void updateMenu(object sender, EventArgs e)
        {
            int dir = isAnimationUp ? 1 : -1;
            timesScaleButton += dir;
            button1.Size = new Size(button1.Size.Width + dir, button1.Size.Height + dir);
            isAnimationUp = (timesScaleButton == maxTimesScaleButton || timesScaleButton == 0) ? !isAnimationUp : isAnimationUp;
        }
        private void startGame()
        {
            this.Controls.Clear();
            timer.Tick -= updateMenu;
            label1.Visible = true;
            scoreLabel.Visible = true;
            if (isNotFirstGame)
            {
                label2.Visible = true;
                bestScoreLabel.Visible = true;
                bestScoreLabel.Text = bestScore.ToString();
            }
            this.Controls.Add(label1);
            this.Controls.Add(scoreLabel);
            this.Controls.Add(label2);
            this.Controls.Add(bestScoreLabel);
            this.Controls.AddRange(gameBord.HorizonLine);
            this.Controls.AddRange(gameBord.VerticalLine);
            this.Controls.AddRange(snake.Head);
            this.Controls.Add(fruit);
            this.KeyDown += new KeyEventHandler(keyDown);
            timer.Tick += new EventHandler(update);
            timer.Start();
            genFruit();
        }
        private void update(object sender, EventArgs e)
        {
            isMoveed = false;
            snake.Move();
            if(isEat())
            {
                snake.Eat();
                if (isWin())
                {
                    bestScore = sizeBord;
                    showMenu();
                }
                this.Controls.Add(snake.Head[snake.size - 1]);
                genFruit();
            }
            if(isMoveToBorder() || isMoveToTail())
            {
                for (int i = 0; i < snake.size; ++i)
                {
                    this.Controls.Remove(snake.Head[i]);
                }
                snake.Dead();
                this.Controls.AddRange(snake.Head);
                genFruit();
                isNotFirstGame = true;
                if(bestScore < int.Parse(scoreLabel.Text))
                {
                    bestScore = int.Parse(scoreLabel.Text);
                }
            }
            scoreLabel.Text = (snake.size - 1).ToString();
            if (isNotFirstGame)
            {
                label2.Visible = true;
                bestScoreLabel.Visible = true;
                bestScoreLabel.Text = bestScore.ToString();
            }
        }
        private bool isEat()
        {
            return snake.Head[0].Location == fruit.Location;
        }
        private bool isMoveToTail()
        {
            bool answer = false;
            for(int i = 1; i < snake.size; ++i)
            {
                if(snake.Head[0].Location == snake.Head[i].Location)
                {
                    answer = true;
                }
            }
            return answer;
        }
        private bool isMoveToBorder()
        {
            return snake.Head[0].Location.X > sizeBord - 1
                || snake.Head[0].Location.Y > sizeBord - 1
                || snake.Head[0].Location.X < 0
                || snake.Head[0].Location.Y < 0;
        }
        private void keyDown(object sender, KeyEventArgs e)
        {
            if (!isMoveed)
            {
                switch (e.KeyCode)
                {
                    case Keys.Up:
                        snake.Dir = (snake.Dir.Y != 1) ? new Point(0, -1) : snake.Dir;
                        break;
                    case Keys.Down:
                        snake.Dir = (snake.Dir.Y != -1) ? new Point(0, 1) : snake.Dir;
                        break;
                    case Keys.Left:
                        snake.Dir = (snake.Dir.X != 1) ? new Point(-1, 0) : snake.Dir;
                        break;
                    case Keys.Right:
                        snake.Dir = (snake.Dir.X != -1) ? new Point(1, 0) : snake.Dir;
                        break;
                    case Keys.Escape:
                        showMenu();
                        break;
                }
                isMoveed = true;
            }
        }
        private class Snake
        {
            int sizeMax = sizeBord;
            public PictureBox[] Head { get; private set; }
            public int size { get; private set; }
            public Point Dir{ get; set; }
            public Snake()
            {
                initial();
            }
            private void initial()
            {
                size = 1;
                Head = new PictureBox[sizeMax];
                Head[0] = new PictureBox
                {
                    Size = new Size(sizeCell, sizeCell),
                    Location = new Point(sizeBord / 2, sizeBord / 2),
                    BackColor = Color.Lime
                };
                Dir = new Point(0, 0);
            }
            public void Move()
            {
                for(int i = size - 1; i >= 1; --i)
                {
                    Head[i].Location = Head[i - 1].Location;
                }
                Head[0].Location = new Point(Head[0].Location.X + sizeCell * Dir.X, Head[0].Location.Y + sizeCell * Dir.Y);
            }
            public void Eat()
            {
                ++size;
               
                Head[size - 1] = new PictureBox
                {
                    Size = new Size(sizeCell, sizeCell),
                    Location = new Point(Head[size - 2].Location.X - Dir.X, Head[size - 2].Location.Y - Dir.Y),
                    BackColor = Color.Lime
                };
            }
            public void Dead()
            {
                initial();
            }
        }
        private void genFruit()
        {
            Random rnd = new Random();
            int x = rnd.Next(1, sizeCell) * sizeCell;
            int y = rnd.Next(1, sizeCell) * sizeCell;
            fruit.Location = new Point(x, y);
            if (isFruitInSnake())
            {
                genFruit();
            }
            return;
        }
        private bool isFruitInSnake()
        {
            bool answer = false;
            for(int i = 0; i < snake.size; ++i)
            {
                if(fruit.Location == snake.Head[i].Location)
                {
                    answer = true;
                }
            }
            return answer;
        }
        private class GameBord
        {
            public PictureBox[] HorizonLine { get; }
            public PictureBox[] VerticalLine { get; }
            public GameBord()
            {
                HorizonLine = createLine((width, height) => new Size(width, height), (x, y) => new Point(y, x));
                VerticalLine = createLine((width, height) => new Size(height, width), (x, y) => new Point(x, y));

            }
            private delegate Size SizeLine(int width, int height);
            private delegate Point PointLine(int x, int y);
            private PictureBox[] createLine(SizeLine sizeLine, PointLine pointLine)
            {
                PictureBox[] line = new PictureBox[sizeCell];
                for(int i = 0, shift = 20; i < line.Length; ++i, shift += 20)
                {
                    line[i] = new PictureBox()
                    {
                        Size = sizeLine(sizeBord, 1),
                        Location = pointLine(shift, 0),
                        BackColor = Color.White
                    };
                }
                return line;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button1.Visible = false;
            timer.Stop();
            startGame();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.Create(path)))
                {
                    writer.Write(bestScore);
                }
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show($"Не удалось найти файл по пути {path}");
            }
        }

        private bool isWin()
        {
            return snake.size == sizeBord;
        }
    }
}
