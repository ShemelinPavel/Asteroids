using System;
using System.Windows.Forms;

namespace Asteroids
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Form form = new Form();
            form.Width = 800;
            form.Height = 600;

            try
            {
                Game.Init( form );
                form.Show();
                Game.Draw();
                Application.Run( form );
            }
            catch (ArgumentOutOfRangeException e)
            {
                MessageBox.Show( $"Ошибка запуска игры: {e.Message}" );
            }
        }
    }
}
