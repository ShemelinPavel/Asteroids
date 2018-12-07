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
        // Ширина и высота игрового поля
        public static int Width { get; set; }
        public static int Height { get; set; }
        public static BaseObject[] _objs;

        private static Random rand;

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
                obj.Update();
        }

        /// <summary>
        /// загрузка объектов игры
        /// </summary>
        public static void Load()
        {
            _objs = new BaseObject[60];

            //летающая тарелка
            //запускаем сначала ее, чтобы астероиды все двигались
            _objs[0] = new Ufo( new Point( 600, rand.Next( Height ) ), new Point( -8, 0 ), new Size( 40, 40 ) );

            //астероиды
            for (int i = 1; i <= 15; i++)
            {
                _objs[i] = new Asteroid( new Point( 600, i * 20 ), new Point( -i, -i * 2 ), new Size( 20, 20 ) );
            }

            //звезды
            for (int i = 16; i <= 35; i++)
            {
                _objs[i] = new Star( new Point( 600,  rand.Next( 0, Height ) ), new Point( i, 0 ), new Size( 15, 15 ) );
            }

            //звездная пыль
            for (int i = 36; i < _objs.Length; i++)
            {
                _objs[i] = new StarDust( new Point( 600, rand.Next( 0, Height ) ), new Point( i, 0 ), new Size( 3, 3 ) );
            }
        }
    }
}