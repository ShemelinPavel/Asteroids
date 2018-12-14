using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

namespace Asteroids
{
    /// <summary>
    /// Логирование событий игры
    /// </summary>
    public static class GameLog
    {
        /// <summary>
        /// Вывод сообщения в консоль
        /// </summary>
        /// <param name="s">Текст сообщения</param>
        public static void Write( string s )
        {
            Console.WriteLine( s );
        }
    }

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
        private static List<Bullet> _bullets = new List<Bullet>();

        /// <summary>
        /// коллекция объектов Астероид
        /// </summary>
        private static List<Asteroid> _asteroids = new List<Asteroid>();

        /// <summary>
        /// коллекция объектов Аптечка
        /// </summary>
        private static List<AidKit> _aidkits = new List<AidKit>();

        /// <summary>
        /// объект Корабль
        /// </summary>
        private static Ship _ship;

        /// <summary>
        /// таймер
        /// </summary>
        private static Timer _timer = new Timer() { Interval = 100 };

        /// <summary>
        /// набранные очки за игру
        /// </summary>
        private static int gameScore;

        /// <summary>
        /// счетчик волн атак
        /// </summary>
        private static int gameWaveCounter;

        /// <summary>
        /// счетчик кадров надписи волн атак
        /// </summary>
        private static int gameWaveSpoilerCounter;

        /// <summary>
        /// конструктор
        /// </summary>
        static Game()
        {
            rand = new Random( 0 );
            gameScore = 0;
            gameWaveCounter = 1;

            GameLog.Write( $"Начата игра. Дата: {DateTime.Now}" );
        }

        /// <summary>
        /// инициализация игрового поля
        /// </summary>
        /// <param name="form"></param>
        public static void Init( Form form )
        {
            if (form.ClientSize.Width > 1000)
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

            _timer.Start();
            _timer.Tick += Timer_Tick;

            form.KeyDown += Form_KeyDown;

            Ship.EventShipDie += EventShipDie;
            Ship.EventShipEnergyChange += EventShipEnergyChange;

            _ship.EventObjectCollision += _ship_EventObjectCollision;
        }

        /// <summary>
        /// обработчик события нажатия клавиши
        /// </summary>
        /// <param name="sender">объект-источник событие</param>
        /// <param name="e">параметры события</param>
        private static void Form_KeyDown( object sender, KeyEventArgs e )
        {
            if (e.KeyCode == Keys.ControlKey)
            {
                Bullet bullet = new Bullet( new Point( _ship.Rect.X + 35, _ship.Rect.Y + 13 ), new Point( 4, 0 ), new Size( 4, 1 ), ObjectSkinManager.GetImages( typeof( Bullet ) ), Game.Width, Game.Height );
                bullet.EventBulletShot += _bullet_EventBulletShot;
                bullet.EventObjectCollision += _bullet_EventObjectCollision;

                _bullets.Add( bullet );
            }
            if (e.KeyCode == Keys.Up) _ship.Up();
            if (e.KeyCode == Keys.Down) _ship.Down();
            if (e.KeyCode == Keys.Escape) Application.Exit();
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
            Buffer.Graphics.Clear( Color.Black );
            foreach (BaseObject obj in _objs) obj.Draw();

            foreach (Asteroid a in _asteroids)
            {
                a?.Draw();
            }

            foreach (AidKit a in _aidkits)
            {
                a?.Draw();
            }

            foreach (Bullet b in _bullets) b.Draw();

            _ship?.Draw();
            if (_ship != null)
            {
                Buffer.Graphics.DrawString( "Energy:" + _ship.Energy, SystemFonts.DefaultFont, Brushes.White, 0, 0 );
                Buffer.Graphics.DrawString( "Score:" + gameScore, SystemFonts.DefaultFont, Brushes.White, 0, Game.Height - 20 );
            }
            Buffer.Render();
        }

        /// <summary>
        /// подготовка следующего кадра
        /// </summary>
        public static void Update()
        {
            foreach (BaseObject obj in _objs) obj.Update();

            foreach (Bullet b in _bullets) b.Update();

            for (int i = _aidkits.Count - 1; i >= 0; i--)
            {
                _aidkits[i].Update();
                if (_ship != null && _ship.Collision( _aidkits[i] ))
                {
                    _ship.EnergyHigh( _aidkits[i].Power );
                    _aidkits.Remove( _aidkits[i] );
                    System.Media.SystemSounds.Exclamation.Play();
                }
            }

            if (_asteroids.Count == 0)
            {
                NewWaveCreate();
            }
            else
            {
                for (int i = _asteroids.Count - 1; i >= 0; i--)
                {
                    _asteroids[i].Update();

                    for (int j = _bullets.Count - 1; j >= 0; j--)
                    {
                        //Пуля попала в Астероид
                        if (_bullets[j].Collision( _asteroids[i] ))
                        {
                            _bullets.Remove( _bullets[j] );
                            _asteroids[i].StartBlow();
                            System.Media.SystemSounds.Hand.Play();
                        }
                    }

                    //астероид взорвался "до конца" - можно его удалять со сцены
                    if (_asteroids[i].IsBlow)
                    {
                        _asteroids.Remove( _asteroids[i] );
                        continue;
                    }
                    else if (_ship != null && _ship.Collision( _asteroids[i] ))
                    {
                        _ship.EnergyLow( _asteroids[i].Power );
                        _asteroids[i].StartBlow();
                        System.Media.SystemSounds.Asterisk.Play();
                    }
                    if (_ship.Energy <= 0) _ship?.Die();
                }
            }

            //int asteroidsNullCounter = 0;
            //for (var i = 0; i < _asteroids.Length; i++)
            //{


            //    if (_asteroids[i] == null)
            //    {
            //        asteroidsNullCounter++;
            //        if (asteroidsNullCounter == _asteroids.Length)
            //        {
            //            Finish();
            //        }
            //        continue;
            //    }
            //    _asteroids[i].Update();

            //    //астероид взорвался "до конца" - можно его удалять со сцены
            //    if (_asteroids[i].IsBlow)
            //    {
            //        _asteroids[i] = null;
            //        continue;
            //    }

            //    //Пуля попала в Астероид
            //    if (_bullet != null && _bullet.Collision( _asteroids[i] ))
            //    {
            //        System.Media.SystemSounds.Hand.Play();
            //        _bullet = null;
            //        _asteroids[i].StartBlow();

            //        continue;
            //    }
            //    if (!_ship.Collision( _asteroids[i] )) continue;

            //    if (!( _asteroids[i].IsBlow ))
            //    {
            //        _ship?.EnergyLow( _asteroids[i].Power );
            //        _asteroids[i].StartBlow();
            //        System.Media.SystemSounds.Asterisk.Play();
            //    }
            //    if (_ship.Energy <= 0) _ship?.Die();
            //}
        }

        /// <summary>
        /// загрузка объектов игры
        /// </summary>
        public static void Load()
        {
            //коллекции объектов
            _objs = new BaseObject[30];

            //летающая тарелка
            _objs[0] = new Ufo( new Point( 15, rand.Next( Height ) ), new Point( -8, 0 ), new Size( 40, 40 ), ObjectSkinManager.GetImages( typeof( Ufo ) ), Game.Width, Game.Height );


            //звездная пыль
            for (int i = 1; i <= 19; i++)
            {
                _objs[i] = new StarDust( new Point( Game.Width, rand.Next( 0, Height ) ), new Point( i, 0 ), new Size( 3, 3 ), ObjectSkinManager.GetImages( typeof( StarDust ) ), Game.Width, Game.Height );
            }

            //звезды
            for (int i = 20; i < _objs.Length; i++)
            {
                _objs[i] = new Star( new Point( Game.Width, rand.Next( 0, Height ) ), new Point( i, 0 ), new Size( 15, 15 ), ObjectSkinManager.GetImages( typeof( Star ) ), Game.Width, Game.Height );
            }

            //Корабль
            _ship = new Ship( new Point( 10, 400 ), new Point( 5, 5 ), new Size( 36, 26 ), ObjectSkinManager.GetImages( typeof( Ship ) ), Game.Width, Game.Height );
        }

        public static void NewWaveCreate()
        {

            //пули
            if (_bullets.Count != 0) _bullets.Clear();

            if (gameWaveSpoilerCounter < 50)
            {
                if (gameWaveSpoilerCounter % 2 == 0)
                {
                    Buffer.Graphics.DrawString( "The new wave!", new Font( FontFamily.GenericSansSerif, 60, FontStyle.Underline ), Brushes.White, 150, 100 );
                    Buffer.Render();
                }
                gameWaveSpoilerCounter++;
                return;
            }

            //астероиды
            int i = 1;
            while (i < gameWaveCounter + 7)
            {
                int r = rand.Next( 5, 50 );
                Asteroid asteroid = new Asteroid( new Point( Game.Width, rand.Next( 0, Game.Height ) ), new Point( -r / 5, r ), new Size( r, r ), ObjectSkinManager.GetImages( typeof( Asteroid ) ), Game.Width, Game.Height );
                asteroid.EventAstBlow += Game_EventAstBlow;
                _asteroids.Add( asteroid );

                i++;
            }

            //аптечки
            for (int j = 0; i < 3; j++)
            {
                int r = rand.Next( 5, 50 );
                _aidkits.Add( new AidKit( new Point( Game.Width - 10, rand.Next( 0, Game.Height ) ), new Point( -r / 5, r ), new Size( 35, 35 ), ObjectSkinManager.GetImages( typeof( AidKit ) ), Game.Width, Game.Height ) );
            }

            GameLog.Write( $"Новая волна астероидов. Количество: {_asteroids.Count}" );

            //увеличиваем счетчик волн
            gameWaveCounter++;

            //сбрасываем счетчик кадров для надписи
            gameWaveSpoilerCounter = 0;

        }

        /// <summary>
        /// обработка события - взрыв объекта Астероид
        /// </summary>
        /// <param name="sender">объект-источник события</param>
        /// <param name="e">параметры события</param>
        private static void Game_EventAstBlow( object sender, GameEventArgs e )
        {
            gameScore += 10;
            GameLog.Write( e.Message );
        }

        /// <summary>
        /// обработка события - выстрел объектом Пуля объекта Корабль
        /// </summary>
        /// <param name="sender">объект-источник события</param>
        /// <param name="e">параметры события</param>
        private static void _bullet_EventBulletShot( object sender, GameEventArgs e )
        {
            GameLog.Write( e.Message );
        }

        /// <summary>
        /// обработка события - столкновение объекта Пуля с другим объектом
        /// </summary>
        /// <param name="sender">объект-источник события</param>
        /// <param name="e">параметры события</param>
        private static void _bullet_EventObjectCollision( object sender, GameEventArgs e )
        {
            GameLog.Write( e.Message );
        }

        /// <summary>
        /// обработка события - столкновение объекта Корабль с другим объектом
        /// </summary>
        /// <param name="sender">объект-источник события</param>
        /// <param name="e">параметры события</param>
        private static void _ship_EventObjectCollision( object sender, GameEventArgs e )
        {
            GameLog.Write( e.Message );
        }

        /// <summary>
        /// обработка события - повреждение/лечение объекта Корабль
        /// </summary>
        /// <param name="sender">объект-источник события</param>
        /// <param name="e">параметры события</param>
        private static void EventShipEnergyChange( object sender, GameEventArgs e )
        {
            GameLog.Write( e.Message );
        }

        /// <summary>
        /// обработка события - смерть объекта Корабль
        /// </summary>
        /// <param name="sender">объект-источник события</param>
        /// <param name="e">параметры события</param>
        public static void EventShipDie( object sender, GameEventArgs e )
        {
            GameLog.Write( e.Message );
            Finish();
        }

        /// <summary>
        /// Конец игры
        /// </summary>
        public static void Finish()
        {
            GameLog.Write( $"Игра завершена. Очков набрано: {gameScore}. Дата: {DateTime.Now}" );

            _timer.Stop();
            Buffer.Graphics.DrawString( "The End", new Font( FontFamily.GenericSansSerif, 60, FontStyle.Underline ), Brushes.White, 200, 100 );
            Buffer.Graphics.DrawString( $"The game score: {gameScore}", new Font( FontFamily.GenericSansSerif, 30, FontStyle.Underline ), Brushes.White, 175, 200 );
            Buffer.Render();
        }
    }
}