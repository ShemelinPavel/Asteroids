using System;
using System.Drawing;
using System.Reflection;

namespace Asteroids
{
    /// <summary>
    /// делегат для взимподействия
    /// </summary>
    public delegate void Message();

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
        /// прямоугольник по размеру текущего объекта
        /// </summary>
        public Rectangle Rect => new Rectangle( Pos, Size );

        /// <summary>
        /// конструктор базового объекта
        /// </summary>
        /// <param name="pos">позиция объекта</param>
        /// <param name="dir">скорость и направление смещения объекта</param>
        /// <param name="size">размер объекта</param>
        protected BaseObject( Point pos, Point dir, Size size )
        {
            Pos = pos;
            Dir = dir;
            Size = size;
        }

        /// <summary>
        /// отрисовка объекта
        /// </summary>
        public abstract void Draw();

        /// <summary>
        /// вычисление новых координат объекта
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// обработка событий покидания пределов экрана и столкновений
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// столкновение - реализация ICollision
        /// </summary>
        /// <param name="o">второй объект столкновения</param>
        /// <returns>координаты пересеклись</returns>
        public bool Collision( ICollision o ) => o.Rect.IntersectsWith( this.Rect );
    }

    /// <summary>
    /// Звезда - объект для формирования фона
    /// </summary>
    class Star : BaseObject
    {
        /// <summary>
        /// картинка
        /// </summary>
        private static Image image;

        /// <summary>
        /// статический конструктор - инициализация картинки
        /// </summary>
        static Star()
        {
            image = new Bitmap( Assembly.GetEntryAssembly().GetManifestResourceStream( "Asteroids.Resources.Star.png" ) );
        }

        /// <summary>
        /// конструктор объекта Звезда
        /// </summary>
        /// <param name="pos">позиция объекта</param>
        /// <param name="dir">направление и скорость смещения объекта</param>
        /// <param name="size">размер объекта</param>
        public Star( Point pos, Point dir, Size size ) : base( pos, dir, size ) { }

        /// <summary>
        /// отрисовка объекта Звезда
        /// </summary>
        public override void Draw()
        {
            Game.Buffer.Graphics.DrawImage( image, new Rectangle( this.Pos, this.Size ) );
        }

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
        public override void Reset()
        {
            this.Pos.X = Game.Width + Size.Width;
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
        public event EventHandler eventAstBlow;

        /// <summary>
        /// коллекция картинок
        /// </summary>
        static private Image[] imageColl;
        /// <summary>
        /// рандомайзер
        /// </summary>
        static private Random rand;

        /// <summary>
        /// картинка
        /// </summary>
        private Image image;

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
            imageColl = new Image[5];
            imageColl[0] = new Bitmap( Assembly.GetEntryAssembly().GetManifestResourceStream( "Asteroids.Resources.Asteroid1.png" ) );
            imageColl[1] = new Bitmap( Assembly.GetEntryAssembly().GetManifestResourceStream( "Asteroids.Resources.Asteroid2.png" ) );
            imageColl[2] = new Bitmap( Assembly.GetEntryAssembly().GetManifestResourceStream( "Asteroids.Resources.Blow0.png" ) );
            imageColl[3] = new Bitmap( Assembly.GetEntryAssembly().GetManifestResourceStream( "Asteroids.Resources.Blow1.png" ) );
            imageColl[4] = new Bitmap( Assembly.GetEntryAssembly().GetManifestResourceStream( "Asteroids.Resources.Blow2.png" ) );

            rand = new Random( 0 );
        }

        /// <summary>
        /// конструктор объекта Астероид
        /// </summary>
        /// <param name="pos">позиция объекта</param>
        /// <param name="dir">направление и скорость смещения объекта</param>
        /// <param name="size">размер объекта</param>
        public Asteroid( Point pos, Point dir, Size size ) : base( pos, dir, size )
        {
            image = imageColl[rand.Next( 0, 2 )];
            Power = 1;
            BlowCounter = 0;
        }

        /// <summary>
        /// отрисовка объекта Астероид
        /// </summary>
        public override void Draw()
        {
            if (blowCounter == 1)
            {
                Game.Buffer.Graphics.DrawImage( imageColl[2], new Rectangle( this.Pos, this.Size ) );
            }
            else if (blowCounter == 2)
            {
                Game.Buffer.Graphics.DrawImage( imageColl[3], new Rectangle( this.Pos, this.Size ) );
            }
            else if (blowCounter == 3)
            {
                Game.Buffer.Graphics.DrawImage( imageColl[4], new Rectangle( this.Pos, this.Size ) );
            }
            else
            {
                Game.Buffer.Graphics.DrawImage( image, new Rectangle( this.Pos, this.Size ) );
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
                    if (Pos.X > Game.Width) Dir.X = -Dir.X;
                    if (Pos.Y < 0) Dir.Y = -Dir.Y;
                    if (Pos.Y > Game.Height) Dir.Y = -Dir.Y;
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
        /// обработка столкновения с объектом Bullet
        /// </summary>
        public override void Reset()
        {
            Random rand = new Random( 0 );
            int r = rand.Next( 0, Game.Height );
            this.Pos = new Point( Game.Width, r );
        }

        /// <summary>
        /// Окончание взрыва
        /// </summary>
        private void Blow()
        {
            this.isBlow = true;

            EventHandler handler = eventAstBlow;
            if (handler != null)
            {
                EventArgs e = new EventArgs();
                handler( this, e );
            }
        }
        
        /// <summary>
        /// Начало взрыва
        /// </summary>
        public void StartBlow()
        {
            BlowCounter++;
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
        /// картинка
        /// </summary>
        private static Image image;
        /// <summary>
        /// рандомайзер
        /// </summary>
        private static Random rand;

        /// <summary>
        /// статический конструктор - инициализация коллекции картинок и рандомайзера
        /// </summary>
        static Ufo()
        {
            image = new Bitmap( Assembly.GetEntryAssembly().GetManifestResourceStream( "Asteroids.Resources.UFO.png" ) );
            rand = new Random( 0 );
        }

        /// <summary>
        /// конструктор объекта Летающая тарелка
        /// </summary>
        /// <param name="pos">позиция объекта</param>
        /// <param name="dir">направление и скорость смещения объекта</param>
        /// <param name="size">размер объекта</param>
        public Ufo( Point pos, Point dir, Size size ) : base( pos, dir, size )
        {
        }

        /// <summary>
        /// отрисовка объекта Летающая тарелка
        /// </summary>
        public override void Draw()
        {
            Game.Buffer.Graphics.DrawImage( image, new Rectangle( this.Pos, this.Size ) );
        }

        /// <summary>
        /// вычисление новых координат объекта Летающая тарелка
        /// </summary>
        public override void Update()
        {
            Pos.X = Pos.X - Dir.X;
            if (Pos.X > Game.Width) this.Reset();
        }

        //обработка покидания границ экрана
        public override void Reset()
        {
            Pos.X = 0;
            Pos.Y = rand.Next( Game.Height );
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
        public StarDust( Point pos, Point dir, Size size ) : base( pos, dir, size )
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
        public override void Reset()
        {
            this.Pos.X = Game.Width + Size.Width;
        }
    }
    #endregion StarDust

    #region Bullet
    /// <summary>
    /// Объект Пуля
    /// </summary>
    class Bullet : BaseObject
    {
        /// <summary>
        /// конструктор объекта Пуля
        /// </summary>
        /// <param name="pos">позиция объекта</param>
        /// <param name="dir">направление и скорость смещения объекта</param>
        /// <param name="size">размер объекта</param>
        public Bullet( Point pos, Point dir, Size size ) : base( pos, dir, size )
        {
        }

        /// <summary>
        /// отрисовка объекта Пуля
        /// </summary>
        public override void Draw()
        {
            Game.Buffer.Graphics.DrawRectangle( Pens.OrangeRed, Pos.X, Pos.Y, Size.Width, Size.Height );
        }

        /// <summary>
        /// вычисление новых координат объекта Пуля
        /// </summary>
        public override void Update()
        {
            Pos.X = Pos.X + 6;
            if (Pos.X > Game.Width) this.Reset();
        }

        /// <summary>
        /// перенос объекта в начало экрана в случайную позицию по высоте
        /// </summary>
        public override void Reset()
        {
            Random rand = new Random( 0 );
            int r = rand.Next( 0, Game.Height );
            this.Pos = new Point( 0, r );
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
        /// картинка
        /// </summary>
        private static Image image;

        private int _energy = 100;
        /// <summary>
        /// жизнь 
        /// </summary>
        public int Energy => _energy;

        static Ship()
        {
            image = new Bitmap( Assembly.GetEntryAssembly().GetManifestResourceStream( "Asteroids.Resources.Ship.png" ) );
        }

        /// <summary>
        /// уменьшение жизни объекта Корабль
        /// </summary>
        /// <param name="n">размер уменьшения</param>
        public void EnergyLow( int n )
        {
            _energy -= n;
        }
        public Ship( Point pos, Point dir, Size size ) : base( pos, dir, size )
        {
        }
        public override void Draw()
        {
            Game.Buffer.Graphics.DrawImage( image, new Rectangle( this.Pos, this.Size ) );
        }
        public override void Update()
        {
        }
        public void Up()
        {
            if (Pos.Y > 0) Pos.Y = Pos.Y - Dir.Y;
        }
        public void Down()
        {
            if (Pos.Y < Game.Height) Pos.Y = Pos.Y + Dir.Y;
        }

        /// <summary>
        /// перенос объекта в начало экрана в случайную позицию по высоте
        /// </summary>
        public override void Reset()
        {
            Random rand = new Random( 0 );
            int r = rand.Next( 0, Game.Height );
            this.Pos = new Point( 0, r );
        }

        /// <summary>
        /// гибель объекта Корабль
        /// </summary>
        public void Die()
        {
            MessageDie?.Invoke();
        }

        /// <summary>
        /// событие гибели объекта Корабль
        /// </summary>
        public static event Message MessageDie;
    }
    #endregion Ship
}