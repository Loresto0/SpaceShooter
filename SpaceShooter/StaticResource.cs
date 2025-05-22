using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceShooter
{
    static class StaticResource
    {
        public static string PathBullet = AppDomain.CurrentDomain.BaseDirectory + "/Assets/bullet1.png";

        public static string PathShip = AppDomain.CurrentDomain.BaseDirectory + "/Assets/ship1.png";

        public static string PathAsteroid = AppDomain.CurrentDomain.BaseDirectory + "/Assets/asteroid1.png";

        public static string PathBoss = AppDomain.CurrentDomain.BaseDirectory + "/Assets/boss1.png";

        public static void ChangePath(int level)
        {
            switch (level)
            {
                case 2:
                    PathBullet = AppDomain.CurrentDomain.BaseDirectory + "/Assets/bullet2.png";
                    PathShip = AppDomain.CurrentDomain.BaseDirectory + "/Assets/ship2.png";
                    PathAsteroid = AppDomain.CurrentDomain.BaseDirectory + "/Assets/asteroid2.png";
                    break;
            }

            switch (level)
            {
                case 6:
                    PathBullet = AppDomain.CurrentDomain.BaseDirectory + "/Assets/bullet2.png";
                    PathShip = AppDomain.CurrentDomain.BaseDirectory + "/Assets/ship2.png";
                    PathAsteroid = AppDomain.CurrentDomain.BaseDirectory + "/Assets/asteroid2.png";
                    PathBoss = AppDomain.CurrentDomain.BaseDirectory + "/Assets/boss2.png";
                    break;
            }
        }
    }
}
