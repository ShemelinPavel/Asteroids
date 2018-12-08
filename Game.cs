using System;
using System.Windows.Forms;
using System.Drawing;
namespace Asteroids
{
    /// <summary>
    /// основной класс игры
    /// </summary>
    static class Game
    {
        private static BufferedGraphicsContext _context;
        public static BufferedGraphics Buffer;
        // Свойства

        /// <summary>
        /// ширина игрового поля
        /// </summary>
        public static int Width { get; set; }

        /// <summary>
        /// высота игрового поля
        /// </summary>
        public static int Height { get; set; }

        /// <summary>
        /// коллекция объектов игры
        /// </summary>
        public static BaseObject[] _objs;

        /// <summary>
        /// рандомайзер
        /// </summary>
        private static Random rand;

        /// <summary>
        /// объект Пуля
        /// </summary>
        private static Bullet _bullet;

        /// <summary>
        /// конструктор
        /// </summary>
        static Game()
        {
            rand = new Random( 0 );
        }
        
        /// <summary>
        /// инициализация игрового поля
        /// </summary>
        /// <param name="form"></param>
        public static void Init( Form form )
        {
            if(form.ClientSize.Width > 1000)
            {
                throw new ArgumentOutOfRangeException( "Width", "Ширина окна превышает 1000 точек" );
            }
            if (form.ClientSize.Height > 1000)
            {
                throw new ArgumentOutOfRangeException( "Height", "Высота окна превышает 1000 точек" );
            }

            // Графическое устройство для вывода графики            
            Graphics g;
            // Предоставляет доступ к главному буферу графического контекста для текущего приложения
            _context = BufferedGraphicsManager.Current;
            g = form.CreateGraphics();
            // Создаем объект (поверхность рисования) и связываем его с формой
            // Запоминаем размеры формы
            Width = form.ClientSize.Width;
            Height = form.ClientSize.Height;
            // Связываем буфер в памяти с графическим объектом, чтобы рисовать в буфере
            Buffer = _context.Allocate( g, new Rectangle( 0, 0, Width, Height ) );

            //загрузка 
            Load();

            Timer timer = new Timer { Interval = 100 };
            timer.Start();
            timer.Tick += Timer_Tick;
        }

        /// <summary>
        /// обработчик события таймера
        /// </summary>
        /// <param name="sender">объект-источник события</param>
        /// <param name="e">параметры события</param>
        private static void Timer_Tick( object sender, EventArgs e )
        {
            Draw();
            Update();
        }

        /// <summary>
        /// отрисовка кадра
        /// </summary>
        public static void Draw()
        {
            // Проверяем вывод графики
            //Buffer.Graphics.Clear( Color.Black );
            //Buffer.Graphics.DrawRectangle( Pens.White, new Rectangle( 100, 100, 200, 200 ) );
            //Buffer.Graphics.FillEllipse( Brushes.Wheat, new Rectangle( 100, 100, 200, 200 ) );
            //Buffer.Render();

            Buffer.Graphics.Clear( Color.Black );
            foreach (BaseObject obj in _objs) obj.Draw();
            Buffer.Render();
        }

        /// <summary>
        /// подготовка следующего кадра
        /// </summary>
        public static void Update()
        {
            foreach (BaseObject obj in _objs)
            {
                obj.Update();

                if (obj is Asteroid)
                {
                    if (obj.Collision( _bullet ))
                    {
                        System.Media.SystemSounds.Hand.Play();
                        _bullet.Reset();
                        obj.Reset();
                    }
                }
            }
        }

        /// <summary>
        /// загрузка объектов игры
        /// </summary>
        public static void Load()
        {
            _objs = new BaseObject[40];

            //пуля
            _bullet = new Bullet( new Point( 0, 200 ), new Point( 5, 0 ), new Size( 4, 1 ) );
            _objs[0] = _bullet;

            //летающая тарелка
            //запускем сначала ее, чтобы астероиды все двигались
            _objs[1] = new Ufo( new Point( 600, rand.Next( Height ) ), new Point( -8, 0 ), new Size( 40, 40 ) );

            //астероиды
            for (int i = 2; i <= 6; i++)
            {
                int r = rand.Next( 5, 50 );
                _objs[i] = new Asteroid( new Point( 600, rand.Next( 0, Game.Height ) ), new Point( -r / 5, r ), new Size( r, r ) );
            }

            //звезды
            for (int i = 7; i <= 15; i++)
            {
                _objs[i] = new Star( new Point( 600,  rand.Next( 0, Height ) ), new Point( i, 0 ), new Size( 15, 15 ) );
            }

            //звездная пыль
            for (int i = 16; i < _objs.Length; i++)
            {
                _objs[i] = new StarDust( new Point( 600, rand.Next( 0, Height ) ), new Point( i, 0 ), new Size( 3, 3 ) );
            }
        }
    }
}