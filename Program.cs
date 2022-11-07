using System;
using System.Drawing;
using System.Security.Cryptography;

namespace Snake
{
    public enum HeroState
        {
             Alive,
             IsDead 
        }

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
    }
    internal sealed class Snake
    {

        public static Snake Instance => snake.Value;

        public Queue<Point> Body { get; private set; }

        public int Lenght => Body.Count;

        public Direction Direction { get; set; }

        public HeroState State { get; set; }

        public Point Head { get; set; }

        public event Action EatenApple;

        public event Action DirectionChanged;

        public event Action HeroDied;

        private Snake()
        {
            Body = new Queue<Point>();
            for (int i = 18; i < 20; i++)
            {
                Point point = new Point(i, 20);
                Body.Enqueue(point);
            }
            State = HeroState.Alive;
            Head = Body.Last();
        }

        public void Move()
        {
            EatenApple?.Invoke();
            DirectionChanged?.Invoke();

            if (State == HeroState.IsDead)
                HeroDied?.Invoke();

            Body.Dequeue();
            Head = directionChangedReactions[Direction].Invoke(Head);
            Body.Enqueue(Head);

        }

        private static readonly Lazy<Snake> snake = new Lazy<Snake>(() => new Snake());
        private const int step = 1;
        private readonly Dictionary<Direction, Func<Point, Point>> directionChangedReactions =
            new Dictionary<Direction, Func<Point, Point>>
        {
            {Direction.Up, point => new Point(point.X, point.Y -= step)},
            {Direction.Down, point => new Point(point.X, point.Y += step)},
            {Direction.Left, point => new Point(point.X -= step, point.Y)},
            {Direction.Right, point => new Point(point.X += step, point.Y)}
        };
    }



    internal static class Drawer
    {
        private static readonly Dictionary<Type, char> displayedChars = new Dictionary<Type, char>
        {
            {typeof(Snake), '@' },
            {typeof(Apple), '*' }
        };

        public static void Draw<T>(Point point)
        {
            Console.SetCursorPosition(point.X, point.Y);
            Console.WriteLine(displayedChars[typeof(T)]);
        }

        public static void Draw(Apple apple)
        {
            if (apple is null)
                throw new ArgumentNullException();

            Console.ForegroundColor = ConsoleColor.Red;

            Draw<Apple>(apple.Point);
        }

        public static void Draw(Snake snake)
        {

            if (snake is null)
                throw new ArgumentNullException();

            Console.ForegroundColor = ConsoleColor.Green;

            foreach (var item in snake.Body)
            {
                if (item == snake.Head)
                    Console.ForegroundColor = ConsoleColor.Yellow;

                Draw<Snake>(item);
            }
        }

        public static void Clear() => Console.Clear();

    }



    class Program
    {

        private static int appleCounter;

        private static readonly Snake snake = Snake.Instance;
        private static readonly Apple apple = Apple.Instance;
        private static readonly Dictionary<ConsoleKey, Action> commands = new Dictionary<ConsoleKey, Action>
        {
            {ConsoleKey.UpArrow,  () => snake.Direction = Direction.Up},
            {ConsoleKey.DownArrow, () => snake.Direction = Direction.Down},
            {ConsoleKey.LeftArrow, () => snake.Direction = Direction.Left},
            {ConsoleKey.RightArrow, () => snake.Direction = Direction.Right},
            {ConsoleKey.Escape, () => snake.State = HeroState.IsDead}
        };
        static void Main(string[] args)
        {
            Console.Title = "Snake";
            Console.CursorVisible = false;

            snake.EatenApple += AddSnakeSegment;
            snake.DirectionChanged += () => Drawer.Clear();
            snake.HeroDied += ShowStatistics;
            snake.BodyIsBitten += ShowStatistics;

            try
            {
                while (snake.State == HeroState.Alive)
                {
                    Drawer.Draw(snake);
                    Drawer.Draw(apple);

                    if (snake.Head.X < 0 || snake.Head.Y < 0
                        || snake.Head.X > 118 || snake.Head.Y > 28)
                    {
                        snake.State = HeroState.IsDead;
                    }

                    ConsoleKeyInfo pressedKey = Console.ReadKey();
                    commands[pressedKey.Key].Invoke();
                    snake.Move();

                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                SetStandartConsoleSettings();
                Console.WriteLine("Персонаж находится за пределами диапазона допустимых значений");
                Console.WriteLine($"Метод, который вызвал исключение: {ex.TargetSite}");
                Console.WriteLine($"Параметр, который вызвал исключение: {ex.ParamName}");
            }
            catch (KeyNotFoundException ex)
            {
                SetStandartConsoleSettings();
                Console.WriteLine("Неизвестная операция!Допустимо только управление стрелками!");
                Console.WriteLine($"Метод, который вызвал исключение: {ex.TargetSite}");
            }
            catch (Exception ex)
            {
                SetStandartConsoleSettings();
                Console.WriteLine(ex.Message);
            }

        }

        private static void ShowStatistics()
        {
            SetStandartConsoleSettings();
            Console.WriteLine("Конец игры!");
            Console.WriteLine($"Число съеденных яблок:{appleCounter}");
            Console.WriteLine($"Длина змеи за игру:{snake.Lenght}");
        }

        private static void AddSnakeSegment()
        {
            if (snake.Head != apple.Point)
                return;

            Point point = new Point(snake.Head.X, snake.Head.Y);
            snake.Body.Enqueue(point);
            apple.Point = new Point(new Random().Next(10, 20), new Random().Next(10, 20));
            appleCounter++;
        }

        private static void SetStandartConsoleSettings()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
        }


    
    }
    
}