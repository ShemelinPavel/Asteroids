using System;
using System.Drawing;
using System.Reflection;

namespace Asteroids
{
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
    abstract class BaseObject: ICollision
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
        /// реализация ICollision
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
        public Star( Point pos, Point dir, Size size ) : base( pos, dir, size ) {}

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

    /// <summary>
    /// Астероид - объект-препятствие
    /// </summary>
    class Asteroid : BaseObject
    {
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

        public int Power { get; set; }

        /// <summary>
        /// статический конструктор - инициализация коллекции картинок и рандомайзера
        /// </summary>
        static Asteroid()
        {
            imageColl = new Image[2];
            imageColl[0] = new Bitmap( Assembly.GetEntryAssembly().GetManifestResourceStream( "Asteroids.Resources.Asteroid1.png" ) );
            imageColl[1] = new Bitmap( Assembly.GetEntryAssembly().GetManifestResourceStream( "Asteroids.Resources.Asteroid2.png" ) );
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
            image = imageColl[rand.Next(0, 2)];
            Power = 1;
        }

        /// <summary>
        /// отрисовка объекта Астероид
        /// </summary>
        public override void Draw()
        {
            Game.Buffer.Graphics.DrawImage( image, new Rectangle( this.Pos, this.Size ) );
        }

        /// <summary>
        /// вычисление новых координат объекта Астероид
        /// </summary>
        public override void Update()
        {
            Pos.X = Pos.X + Dir.X;
            Pos.Y = Pos.Y + Dir.Y;
            if (Pos.X < 0) Dir.X = -Dir.X;
            if (Pos.X > Game.Width) Dir.X = -Dir.X;
            if (Pos.Y < 0) Dir.Y = -Dir.Y;
            if (Pos.Y > Game.Height) Dir.Y = -Dir.Y;
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
    }

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

    /// <summary>
    /// Объект Пуля
    /// </summary>
    class Bullet: BaseObject
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
            Pos.X = Pos.X + 3;
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
}