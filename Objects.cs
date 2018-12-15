using System;
using System.Drawing;
using System.Reflection;

namespace Asteroids
{
    /// <summary>
    /// менеджер скинов для объектов игры
    /// </summary>
    public static class ObjectSkinManager
    {
        /// <summary>
        /// рандомайзер
        /// </summary>
        private static Random rand;

        /// <summary>
        /// Конструктор менеджера скинов для объектов игры
        /// </summary>
        static ObjectSkinManager()
        {
            rand = new Random( 0 );
        }

        /// <summary>
        /// возвращает коллекцию картинок в зависимости от типа объекта
        /// </summary>
        /// <param name="type">тип объекта</param>
        /// <returns>коллекция картинок</returns>
        public static Image[] GetImages( Type type )
        {

            if (type == typeof( Star ))
            {
                return new Image[]
                {
                    new Bitmap( Assembly.GetEntryAssembly().GetManifestResourceStream( "Asteroids.Resources.Star.png" ))
                };
            }
            else if (type == typeof( Asteroid ))
            {
                int r = rand.Next( 0, 2 );
                return new Image[]
                {
                    new Bitmap( (r == 0) ? Assembly.GetEntryAssembly().GetManifestResourceStream( "Asteroids.Resources.Asteroid1.png" ) : Assembly.GetEntryAssembly().GetManifestResourceStream( "Asteroids.Resources.Asteroid2.png" )),
                    new Bitmap( Assembly.GetEntryAssembly().GetManifestResourceStream( "Asteroids.Resources.Blow0.png" )),
                    new Bitmap( Assembly.GetEntryAssembly().GetManifestResourceStream( "Asteroids.Resources.Blow1.png" )),
                    new Bitmap( Assembly.GetEntryAssembly().GetManifestResourceStream( "Asteroids.Resources.Blow2.png" ))
                };
            }
            else if (type == typeof( Ufo ))
            {
                return new Image[]
                {
                    new Bitmap( Assembly.GetEntryAssembly().GetManifestResourceStream( "Asteroids.Resources.UFO.png" ))
                };
            }
            else if (type == typeof( Ship ))
            {
                return new Image[]
                {
                    new Bitmap( Assembly.GetEntryAssembly().GetManifestResourceStream( "Asteroids.Resources.Ship.png" ))
                };
            }
            else if (type == typeof( AidKit ))
            {
                return new Image[]
                {
                    new Bitmap( Assembly.GetEntryAssembly().GetManifestResourceStream( "Asteroids.Resources.AidKit.png" ))
                };
            }
            else return new Image[1];
        }
    }

    /// <summary>
    /// данные событий игры
    /// </summary>
    public class GameEventArgs : EventArgs
    {
        /// <summary>
        /// сообщение подписчику события
        /// </summary>
        private string message;

        /// <summary>
        /// сообщение подписчику события
        /// </summary>
        public string Message => message;

        /// <summary>
        /// сообщение подписчику события
        /// </summary>
        /// <param name="s">тест сообщения</param>
        public GameEventArgs( string s )
        {
            this.message = s;
        }
    }

    /// <summary>
    /// столкновение
    /// </summary>
    interface ICollision
    {
        /// <summary>
        /// столкновение
        /// </summary>
        /// <param name="obj">второй объект</param>
        /// <returns>true/false - объекты столкнулись</returns>
        bool Collision( ICollision obj );
        Rectangle Rect { get; }
    }

    /// <summary>
    /// бзовый класс для отрисовки фугур
    /// </summary>
    abstract class BaseObject : ICollision
    {
        /// <summary>
        /// событие столкновения объекта с другим объектом
        /// </summary>
        public event EventHandler<GameEventArgs> EventObjectCollision;

        /// <summary>
        /// позиция объекта
        /// </summary>
        protected Point Pos;
        /// <summary>
        /// скорость и направление смещения объекта
        /// </summary>
        protected Point Dir;
        /// <summary>
        /// размер объекта
        /// </summary>
        protected Size Size;
        /// <summary>
        /// коллекция скинов объекта
        /// </summary>
        protected Image[] ImageColl;
        /// <summary>
        /// описание границы экрана через точку на ней
        /// </summary>
        protected Point ScreenLimitPoint;

        /// <summary>
        /// прямоугольник по размеру текущего объекта
        /// </summary>
        public Rectangle Rect => new Rectangle( Pos, Size );

        /// <summary>
        /// конструктор базового объекта
        /// </summary>
        /// <param name="pos">позиция объекта</param>
        /// <param name="dir">скорость и направление смещения объекта</param>
        /// <param name="size">размер объекта</param>
        /// <param name="images">коллекция скинов объекта</param>
        /// <param name="limit">граница экрана в виде точки</param>
        protected BaseObject( Point pos, Point dir, Size size, Image[] images, Point limit )
        {
            Pos = pos;
            Dir = dir;
            Size = size;
            ImageColl = images;
            ScreenLimitPoint = limit;
        }


        /// <summary>
        /// отрисовка объекта
        /// </summary>
        public virtual void Draw()
        {
            if (this.ImageColl[0] != null) Game.Buffer.Graphics.DrawImage( this.ImageColl[0], new Rectangle( this.Pos, this.Size ) );
        }

        /// <summary>
        /// вычисление новых координат объекта
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// столкновение - реализация ICollision
        /// </summary>
        /// <param name="o">второй объект столкновения</param>
        /// <returns>координаты пересеклись</returns>
        public bool Collision( ICollision o )
        {
            bool _collision = o.Rect.IntersectsWith( this.Rect );

            if (_collision)
            {
                //регистрируем все события столкновений кроме Астероидов уже ударившихся о Корабль
                if (!( o is Asteroid && ( (Asteroid)o ).Power == 0 ))
                {
                    EventObjectCollision?.Invoke( this, new GameEventArgs( $"Объект: {this.ToString()} столкнулся с объектом: {o.ToString()}" ) );
                }
            }
            return _collision;
        }
    }

    /// <summary>
    /// Звезда - объект для формирования фона
    /// </summary>
    class Star : BaseObject
    {
        /// <summary>
        /// конструктор объекта Звезда
        /// </summary>
        /// <param name="pos">позиция объекта</param>
        /// <param name="dir">направление и скорость смещения объекта</param>
        /// <param name="size">размер объекта</param>
        /// <param name="images">коллекция скинов объекта</param>
        /// <param name="lim">граница экрана</param>
        public Star( Point pos, Point dir, Size size, Image[] images, Point lim ) : base( pos, dir, size, images, lim ) { }

        /// <summary>
        /// вычисление новых координат объекта Звезда
        /// </summary>
        public override void Update()
        {
            Pos.X = Pos.X - Dir.X;
            if (Pos.X < 0) this.Reset();
        }

        /// <summary>
        /// обработка покидания границ экрана
        /// </summary>
        public void Reset()
        {
            this.Pos.X = this.ScreenLimitPoint.X + Size.Width;
        }

        /// <summary>
        /// переопределение представления объекта Звезда
        /// </summary>
        /// <returns>Текстовое представление объекта</returns>
        public override string ToString()
        {
            return "Звезда";
        }
    }

    #region Asteroid
    /// <summary>
    /// Астероид - объект-препятствие
    /// </summary>
    class Asteroid : BaseObject
    {
        /// <summary>
        /// событие - взрыв объекта Астероид
        /// </summary>
        public event EventHandler<GameEventArgs> EventAstBlow;

        /// <summary>
        /// рандомайзер
        /// </summary>
        static private Random rand;

        /// <summary>
        /// счетчик кадров взрыва
        /// </summary>
        private ushort blowCounter;
        /// <summary>
        /// счетчик кадров взрыва
        /// </summary>
        public ushort BlowCounter
        {
            get { return blowCounter; }
            set { blowCounter = value; }
        }

        public int Power { get; set; }

        /// <summary>
        /// объект Астероид взорван
        /// </summary>
        private bool isBlow;
        /// <summary>
        /// объект Астероид взорван
        /// </summary>
        public bool IsBlow => isBlow;

        /// <summary>
        /// статический конструктор - инициализация коллекции картинок и рандомайзера
        /// </summary>
        static Asteroid()
        {
            rand = new Random( 0 );
        }

        /// <summary>
        /// конструктор объекта Астероид
        /// </summary>
        /// <param name="pos">позиция объекта</param>
        /// <param name="dir">направление и скорость смещения объекта</param>
        /// <param name="size">размер объекта</param>
        /// <param name="images">коллекция скинов объекта</param>
        /// <param name="lim">граница экрана</param>
        public Asteroid( Point pos, Point dir, Size size, Image[] images, Point lim ) : base( pos, dir, size, images, lim )
        {
            Power = rand.Next( 1, 11 );
            blowCounter = 0;
        }

        /// <summary>
        /// отрисовка объекта Астероид
        /// </summary>
        public override void Draw()
        {
            if (blowCounter == 1)
            {
                if (this.ImageColl[1] != null) Game.Buffer.Graphics.DrawImage( this.ImageColl[1], new Rectangle( this.Pos, this.Size ) );
            }
            else if (blowCounter == 2)
            {
                if (this.ImageColl[2] != null) Game.Buffer.Graphics.DrawImage( this.ImageColl[2], new Rectangle( this.Pos, this.Size ) );
            }
            else if (blowCounter == 3)
            {
                if (this.ImageColl[3] != null) Game.Buffer.Graphics.DrawImage( this.ImageColl[3], new Rectangle( this.Pos, this.Size ) );
            }
            else
            {
                if (this.ImageColl[0] != null) Game.Buffer.Graphics.DrawImage( this.ImageColl[0], new Rectangle( this.Pos, this.Size ) );
            }
        }

        /// <summary>
        /// вычисление новых координат объекта Астероид
        /// </summary>
        public override void Update()
        {
            if (!( IsBlow ))
            {
                if (blowCounter == 0)
                {
                    Pos.X = Pos.X + Dir.X;
                    Pos.Y = Pos.Y + Dir.Y;
                    if (Pos.X < 0) Dir.X = -Dir.X;
                    if (Pos.X > this.ScreenLimitPoint.X) Dir.X = -Dir.X;
                    if (Pos.Y < 0) Dir.Y = -Dir.Y;
                    if (Pos.Y > this.ScreenLimitPoint.Y) Dir.Y = -Dir.Y;
                }
                else if (blowCounter == 3)
                {
                    Blow();
                }
                else
                {
                    blowCounter++;
                }
            }
        }

        /// <summary>
        /// Окончание взрыва объекта Астероид
        /// </summary>
        private void Blow()
        {
            this.isBlow = true;

            EventAstBlow?.Invoke( this, new GameEventArgs( $"Объект: {this.ToString()} взорвался" ) );
        }

        /// <summary>
        /// Начало взрыва объекта Астероид
        /// </summary>
        public void StartBlow()
        {
            this.blowCounter++;
            this.Power = 0;
        }

        /// <summary>
        /// переопределение представления объекта Астероид
        /// </summary>
        /// <returns>Текстовое представление объекта</returns>
        public override string ToString()
        {
            return "Астероид";
        }
    }
    #endregion Asteroid

    #region Ufo
    /// <summary>
    /// Летающая тарелка - объект для формирования фона
    /// </summary>
    class Ufo : BaseObject
    {
        /// <summary>
        /// рандомайзер
        /// </summary>
        private static Random rand;

        /// <summary>
        /// статический конструктор - инициализация коллекции картинок и рандомайзера
        /// </summary>
        static Ufo()
        {
            rand = new Random( 0 );
        }

        /// <summary>
        /// конструктор объекта Летающая тарелка
        /// </summary>
        /// <param name="pos">позиция объекта</param>
        /// <param name="dir">направление и скорость смещения объекта</param>
        /// <param name="size">размер объекта</param>
        /// <param name="images">коллекция скинов объекта</param>
        /// <param name="lim">граница экрана</param>
        public Ufo( Point pos, Point dir, Size size, Image[] images, Point lim ) : base( pos, dir, size, images, lim )
        {
        }

        /// <summary>
        /// вычисление новых координат объекта Летающая тарелка
        /// </summary>
        public override void Update()
        {
            Pos.X = Pos.X - Dir.X;
            if (Pos.X > this.ScreenLimitPoint.X) this.Reset();
        }

        //обработка покидания границ экрана
        public void Reset()
        {
            Pos.X = 0;
            Pos.Y = rand.Next( this.ScreenLimitPoint.Y );
        }

        /// <summary>
        /// переопределение представления объекта Летающая тарелка
        /// </summary>
        /// <returns>Текстовое представление объекта</returns>
        public override string ToString()
        {
            return "Летающая тарелка";
        }
    }
    #endregion Ufo

    #region StarDust
    /// <summary>
    /// Звездная пыль - объект для формирования фона
    /// </summary>
    class StarDust : BaseObject
    {
        /// <summary>
        /// конструктор объекта Звездная пыль
        /// </summary>
        /// <param name="pos">позиция объекта</param>
        /// <param name="dir">направление и скорость смещения объекта</param>
        /// <param name="size">размер объекта</param>
        /// <param name="images">коллекция скинов объекта</param>
        /// <param name="lim">граница экрана</param>
        public StarDust( Point pos, Point dir, Size size, Image[] images, Point lim ) : base( pos, dir, size, images, lim )
        {
        }

        /// <summary>
        /// отрисовка объекта Звездная пыль
        /// </summary>
        public override void Draw()
        {
            Game.Buffer.Graphics.DrawLine( Pens.White, Pos.X, Pos.Y, Pos.X + Size.Width, Pos.Y + Size.Height );
            Game.Buffer.Graphics.DrawLine( Pens.White, Pos.X + Size.Width, Pos.Y, Pos.X, Pos.Y + Size.Height );
        }

        /// <summary>
        /// вычисление новых координат объекта Звездная пыль
        /// </summary>
        public override void Update()
        {
            Pos.X = Pos.X - Dir.X;
            if (Pos.X < 0) this.Reset();
        }

        /// <summary>
        /// обработка покидания границ экрана
        /// </summary>
        public void Reset()
        {
            this.Pos.X = this.ScreenLimitPoint.X + Size.Width;
        }

        /// <summary>
        /// переопределение представления объекта Звездная пыль
        /// </summary>
        /// <returns>Текстовое представление объекта</returns>
        public override string ToString()
        {
            return "Звездная пыль";
        }
    }
    #endregion StarDust

    #region Bullet
    /// <summary>
    /// Объект Пуля
    /// </summary>
    class Bullet : BaseObject
    {
        public event EventHandler<GameEventArgs> EventBulletShot;

        /// <summary>
        /// флаг - был выстрел (вызывалось событие) или нет
        /// </summary>
        private bool wasEventShot;

        /// <summary>
        /// конструктор объекта Пуля
        /// </summary>
        /// <param name="pos">позиция объекта</param>
        /// <param name="dir">направление и скорость смещения объекта</param>
        /// <param name="size">размер объекта</param>
        /// <param name="images">коллекция скинов объекта</param>
        /// <param name="lim">максимальный размер экрана</param>
        public Bullet( Point pos, Point dir, Size size, Image[] images, Point lim ) : base( pos, dir, size, images, lim )
        {
        }

        /// <summary>
        /// отрисовка объекта Пуля
        /// </summary>
        public override void Draw()
        {
            if (!( wasEventShot ))
            {
                EventBulletShot?.Invoke( this, new GameEventArgs( "Выстрел" ) );
                this.wasEventShot = true;
            }

            Game.Buffer.Graphics.DrawRectangle( Pens.OrangeRed, Pos.X, Pos.Y, Size.Width, Size.Height );
        }

        /// <summary>
        /// вычисление новых координат объекта Пуля
        /// </summary>
        public override void Update()
        {
            Pos.X = Pos.X + 6;
            if (Pos.X > this.ScreenLimitPoint.X) this.Reset();
        }

        /// <summary>
        /// перенос объекта в начало экрана
        /// </summary>
        public void Reset()
        {
            this.Pos = new Point( 0, this.Pos.Y );
        }

        /// <summary>
        /// переопределение представления объекта Пуля
        /// </summary>
        /// <returns>Текстовое представление объекта</returns>
        public override string ToString()
        {
            return "Пуля";
        }
    }
    #endregion Bullet

    #region Ship
    /// <summary>
    /// Корабль - объект управления игрока
    /// </summary>
    class Ship : BaseObject
    {
        /// <summary>
        /// событие гибели объекта Корабль
        /// </summary>
        public static event EventHandler<GameEventArgs> EventShipDie;

        /// <summary>
        /// событие повреждения/лечения объекта Корабль
        /// </summary>
        public static event EventHandler<GameEventArgs> EventShipEnergyChange;

        private int _energy = 100;
        /// <summary>
        /// жизнь 
        /// </summary>
        public int Energy => _energy;

        /// <summary>
        /// уменьшение жизни объекта Корабль
        /// </summary>
        /// <param name="n">размер уменьшения</param>
        public void EnergyLow( int n )
        {
            if (n != 0)
            {
                _energy -= n;
                EventShipEnergyChange?.Invoke( this, new GameEventArgs( $"Объект: {this.ToString()} поврежден на {n}" ) );
            }
        }

        /// <summary>
        /// увеличение жизни объекта Корабль
        /// </summary>
        /// <param name="n">размер увеличения</param>
        public void EnergyHigh( int n )
        {
            if (n != 0)
            {
                _energy = ( _energy + n > 100 ) ? 100 : _energy + n;
                EventShipEnergyChange?.Invoke( this, new GameEventArgs( $"Объект: {this.ToString()} восстановлен на {n}" ) );
            }
        }
        /// <summary>
        /// конструктор объекта Корабль
        /// </summary>
        /// <param name="pos">позиция объекта</param>
        /// <param name="dir">направление и скорость смещения объекта</param>
        /// <param name="size">размер объекта</param>
        /// <param name="images">коллекция скинов объекта</param>
        /// <param name="lim">максимальный размер экрана</param>
        public Ship( Point pos, Point dir, Size size, Image[] images, Point lim ) : base( pos, dir, size, images, lim )
        {
        }

        /// <summary>
        /// вычисление новых координат объекта Корабль
        /// </summary>
        public override void Update()
        {
        }

        /// <summary>
        /// смещение вверх по экрану объекта Корабль
        /// </summary>
        public void Up()
        {
            if (Pos.Y > 0) Pos.Y = Pos.Y - Dir.Y;
        }

        /// <summary>
        /// смещение вниз по экрану объекта Корабль
        /// </summary>
        public void Down()
        {
            if (Pos.Y < this.ScreenLimitPoint.Y) Pos.Y = Pos.Y + Dir.Y;
        }

        /// <summary>
        /// смерть объекта Корабль
        /// </summary>
        public void Die()
        {
            EventShipDie?.Invoke( this, new GameEventArgs( $"Объект {this.ToString()} уничтожен" ) );
        }

        /// <summary>
        /// переопределение представления объекта Корабль
        /// </summary>
        /// <returns>Текстовое представление объекта</returns>
        public override string ToString()
        {
            return $"Корабль. Состояние: {this.Energy}";
        }
    }
    #endregion Ship

    #region AidKit
    /// <summary>
    /// Аптечка - объект-восстановления энергии
    /// </summary>
    class AidKit : BaseObject
    {
        /// <summary>
        /// Количество энерги восстановления
        /// </summary>
        public int Power { get; set; }

        /// <summary>
        /// конструктор объекта Аптечка
        /// </summary>
        /// <param name="pos">позиция объекта</param>
        /// <param name="dir">направление и скорость смещения объекта</param>
        /// <param name="size">размер объекта</param>
        /// <param name="images">коллекция скинов объекта</param>
        /// <param name="lim">максимальный размер экрана</param>
        public AidKit( Point pos, Point dir, Size size, Image[] images, Point lim ) : base( pos, dir, size, images, lim )
        {
            Power = 20;
        }

        /// <summary>
        /// вычисление новых координат объекта Астероид
        /// </summary>
        public override void Update()
        {
            Pos.X = Pos.X + Dir.X;
            Pos.Y = Pos.Y + Dir.Y;
            if (Pos.X < 0) Dir.X = -Dir.X;
            if (Pos.X > this.ScreenLimitPoint.X) Dir.X = -Dir.X;
            if (Pos.Y < 0) Dir.Y = -Dir.Y;
            if (Pos.Y > this.ScreenLimitPoint.Y) Dir.Y = -Dir.Y;
        }

        /// <summary>
        /// переопределение представления объекта Аптечка
        /// </summary>
        /// <returns>Текстовое представление объекта</returns>
        public override string ToString()
        {
            return "Аптечка";
        }
    }
    #endregion AidKit
}