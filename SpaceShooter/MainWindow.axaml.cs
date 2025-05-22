using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpaceShooter
{
    public partial class MainWindow : Window
    {
        public DispatcherTimer Timer;
        // Игровые константы
        private const int PlayerSpeed = 5;
        private const int BulletSpeed = 10;
        private const int AsteroidSpeed = 3;

        // Игровое состояние
        private int score = 0;
        private int lives = 3;
        private int level = 1;
        private bool isGameOver = false;
        private bool isLevelComplete = false;

        // Игровые объекты
        private readonly List<Image> bullets = new List<Image>();
        private readonly List<Image> asteroids = new List<Image>();
        private readonly List<Image> explosions = new List<Image>();
        private Image boss = null;
        private int bossHealth = 0;

        // Управление
        private bool isLeftPressed = false;
        private bool isRightPressed = false;
        private bool isUpPressed = false;
        private bool isDownPressed = false;
        private bool isSpacePressed = false;

        // Таймеры
        private DispatcherTimer gameTimer;
        private DateTime lastShotTime = DateTime.MinValue;
        private Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            // Настройка игрока
            Canvas.SetLeft(playerShip, Width / 2 - playerShip.Width / 2);
            Canvas.SetTop(playerShip, Height - 100);
            playerShip.Source = new Bitmap(StaticResource.PathShip);

            // Настройка UI
            UpdateUI();

            // Настройка игрового таймера
            gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
            };
            gameTimer.Tick += GameLoop;
            gameTimer.Start();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (isGameOver || isLevelComplete) return;

            MovePlayer();
            MoveBullets();
            MoveAsteroids();
            SpawnEnemies();
            CheckCollisions();
            CheckLevelCompletion();
        }

        private void MovePlayer()
        {
            double x = Canvas.GetLeft(playerShip);
            double y = Canvas.GetTop(playerShip);

            if (isLeftPressed) x = Math.Max(0, x - PlayerSpeed);
            if (isRightPressed) x = Math.Min(Width - playerShip.Width, x + PlayerSpeed);
            if (isUpPressed) y = Math.Max(0, y - PlayerSpeed);
            if (isDownPressed) y = Math.Min(Height - playerShip.Height, y + PlayerSpeed);

            Canvas.SetLeft(playerShip, x);
            Canvas.SetTop(playerShip, y);

            // Ограничение скорости стрельбы
            if (isSpacePressed && (DateTime.Now - lastShotTime).TotalMilliseconds > 300)
            {
                Shoot();
                lastShotTime = DateTime.Now;
            }
        }

        private void Shoot()
        {
            var bullet = new Image
            {
                Width = 5,
                Height = 15,
                Source = new Bitmap(StaticResource.PathBullet)
            };

            double x = Canvas.GetLeft(playerShip) + playerShip.Width / 2 - bullet.Width / 2;
            double y = Canvas.GetTop(playerShip) - bullet.Height;

            gameCanvas.Children.Add(bullet);
            Canvas.SetLeft(bullet, x);
            Canvas.SetTop(bullet, y);
            bullets.Add(bullet);
        }

        private void MoveBullets()
        {
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                var bullet = bullets[i];
                double y = Canvas.GetTop(bullet) - BulletSpeed;
                Canvas.SetTop(bullet, y);

                if (y < -bullet.Height)
                {
                    gameCanvas.Children.Remove(bullet);
                    bullets.RemoveAt(i);
                }
            }
        }

        private void SpawnEnemies()
        {
            // Спавн астероидов
            if (random.Next(0, 200) < 5) // 5% шанс
            {
                var asteroid = new Image
                {
                    Width = random.Next(30, 60),
                    Height = random.Next(30, 60),
                    Source = new Bitmap(StaticResource.PathAsteroid)
                };

                double x = random.Next(0, (int)Width - (int)asteroid.Width);
                gameCanvas.Children.Add(asteroid);
                Canvas.SetLeft(asteroid, x);
                Canvas.SetTop(asteroid, -asteroid.Height);
                asteroids.Add(asteroid);
            }

            // Спавн босса каждые 3 уровня
            if (boss == null && level % 3 == 0 && score > level * 500)
            {
                boss = new Image
                {
                    Width = 100,
                    Height = 80,
                    Source = new Bitmap(StaticResource.PathBoss)
                };

                bossHealth = level * 10;
                gameCanvas.Children.Add(boss);
                Canvas.SetLeft(boss, Width / 2 - boss.Width / 2);
                Canvas.SetTop(boss, 50);
            }
        }

        private void MoveAsteroids()
        {
            for (int i = asteroids.Count - 1; i >= 0; i--)
            {
                var asteroid = asteroids[i];
                double y = Canvas.GetTop(asteroid) + AsteroidSpeed;
                Canvas.SetTop(asteroid, y);

                if (y > Height)
                {
                    gameCanvas.Children.Remove(asteroid);
                    asteroids.RemoveAt(i);
                }
            }

            // Движение босса
            if (boss != null)
            {
                double x = Canvas.GetLeft(boss);
                x += Math.Sin(DateTime.Now.TimeOfDay.TotalSeconds) * 2;
                x = Math.Max(0, Math.Min(Width - boss.Width, x));
                Canvas.SetLeft(boss, x);
            }
        }

        private void CheckCollisions()
        {
            // Пули vs астероиды
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                var bullet = bullets[i];
                var bulletRect = new Rect(
                    Canvas.GetLeft(bullet),
                    Canvas.GetTop(bullet),
                    bullet.Width,
                    bullet.Height);

                // Проверка столкновений с астероидами
                for (int j = asteroids.Count - 1; j >= 0; j--)
                {
                    var asteroid = asteroids[j];
                    var asteroidRect = new Rect(
                        Canvas.GetLeft(asteroid),
                        Canvas.GetTop(asteroid),
                        asteroid.Width,
                        asteroid.Height);

                    if (bulletRect.Intersects(asteroidRect))
                    {
                        CreateExplosion(asteroid);
                        gameCanvas.Children.Remove(bullet);
                        bullets.RemoveAt(i);
                        gameCanvas.Children.Remove(asteroid);
                        asteroids.RemoveAt(j);
                        score += 100;
                        UpdateUI();
                        break;
                    }
                }

                // Проверка столкновений с боссом
                if (boss != null)
                {
                    var bossRect = new Rect(
                        Canvas.GetLeft(boss),
                        Canvas.GetTop(boss),
                        boss.Width,
                        boss.Height);

                    if (bulletRect.Intersects(bossRect))
                    {
                        gameCanvas.Children.Remove(bullet);
                        bullets.RemoveAt(i);
                        bossHealth--;

                        if (bossHealth <= 0)
                        {
                            CreateExplosion(boss);
                            gameCanvas.Children.Remove(boss);
                            boss = null;
                            score += 100;
                            UpdateUI();
                        }
                    }
                }
            }

            // Игрок vs враги
            var playerRect = new Rect(
                Canvas.GetLeft(playerShip),
                Canvas.GetTop(playerShip),
                playerShip.Width,
                playerShip.Height);

            foreach (var asteroid in asteroids.ToList())
            {
                var asteroidRect = new Rect(
                    Canvas.GetLeft(asteroid),
                    Canvas.GetTop(asteroid),
                    asteroid.Width,
                    asteroid.Height);

                if (playerRect.Intersects(asteroidRect))
                {
                    CreateExplosion(asteroid);
                    gameCanvas.Children.Remove(asteroid);
                    asteroids.Remove(asteroid);
                    lives--;
                    UpdateUI();

                    if (lives <= 0)
                    {
                        GameOver();
                    }
                    break;
                }
            }
        }

        private async void CreateExplosion(Image target)
        {
            var explosion = new Image
            {
                Width = target.Width,
                Height = target.Height,
                Source = new Bitmap(AppDomain.CurrentDomain.BaseDirectory + "/Assets/explosion.png")
            };

            gameCanvas.Children.Add(explosion);
            Canvas.SetLeft(explosion, Canvas.GetLeft(target));
            Canvas.SetTop(explosion, Canvas.GetTop(target));
            explosions.Add(explosion);

            // Удаление взрыва через 500 мс
                await Task.Delay(300);
                gameCanvas.Children.Remove(explosion);
                explosions.Remove(explosion);
        }

        private void CheckLevelCompletion()
        {
            if (/*asteroids.Count == 0 && boss == null && */score >= level * 1000)
            {
                LevelComplete();
            }
        }

        private async void LevelComplete()
        {
            isLevelComplete = true;
            levelCompleteText.IsVisible = true;

            // Переход на следующий уровень через 3 секунды
             await Task.Delay(500);
                level++;
                isLevelComplete = false;
                levelCompleteText.IsVisible = false;
                UpdateUI();
                StaticResource.ChangePath(level);
                playerShip.Source = new Bitmap(StaticResource.PathShip);
        }

        private void GameOver()
        {
            isGameOver = true;
            gameOverText.IsVisible = true;
            gameTimer.Stop();
        }

        private void UpdateUI()
        {
            scoreText.Text = $"Счет: {score}";
            livesText.Text = $"Жизни: {lives}";
            levelText.Text = $"Уровень: {level}";
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    isLeftPressed = true;
                    break;
                case Key.Right:
                    isRightPressed = true;
                    break;
                case Key.Up:
                    isUpPressed = true;
                    break;
                case Key.Down:
                    isDownPressed = true;
                    break;
                case Key.Space:
                    isSpacePressed = true;
                    break;
                case Key.R:
                    if (isGameOver) ResetGame();
                    break;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            switch (e.Key)
            {
                case Key.Left:
                    isLeftPressed = false;
                    break;
                case Key.Right:
                    isRightPressed = false;
                    break;
                case Key.Up:
                    isUpPressed = false;
                    break;
                case Key.Down:
                    isDownPressed = false;
                    break;
                case Key.Space:
                    isSpacePressed = false;
                    break;
            }
        }

        private void ResetGame()
        {
            // Очистка объектов
            foreach (var bullet in bullets)
                gameCanvas.Children.Remove(bullet);
            foreach (var asteroid in asteroids)
                gameCanvas.Children.Remove(asteroid);
            foreach (var explosion in explosions)
                gameCanvas.Children.Remove(explosion);

            bullets.Clear();
            asteroids.Clear();
            explosions.Clear();

            if (boss != null)
                gameCanvas.Children.Remove(boss);
            boss = null;

            // Сброс состояния
            score = 0;
            lives = 3;
            level = 1;
            isGameOver = false;
            isLevelComplete = false;
            gameOverText.IsVisible = false;
            levelCompleteText.IsVisible = false;

            // Возврат игрока
            Canvas.SetLeft(playerShip, Width / 2 - playerShip.Width / 2);
            Canvas.SetTop(playerShip, Height - 100);

            // Обновление UI
            UpdateUI();

            // Перезапуск игры
            gameTimer.Start();
        }
    }
}