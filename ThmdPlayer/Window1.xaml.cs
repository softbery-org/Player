using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ThmdPlayer
{
    /// <summary>
    /// Logika interakcji dla klasy Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private DispatcherTimer _idleTimer;
        private DispatcherTimer _particleTimer;
        private TimeSpan _idleTimeThreshold = TimeSpan.FromSeconds(3);
        private Storyboard _backgroundAnimation;
        private List<(Ellipse Particle, double VelocityX, double VelocityY)> _particles;
        private Random _random = new Random();
        private bool _isAnimationRunning = false;

        public Window1()
        {
            InitializeComponent();
            _backgroundAnimation = (Storyboard)FindResource("BackgroundAnimation");
            _particles = new List<(Ellipse, double, double)>();

            // Timer dla bezczynności
            _idleTimer = new DispatcherTimer
            {
                Interval = _idleTimeThreshold
            };
            _idleTimer.Tick += IdleTimer_Tick;

            // Timer dla animacji cząsteczek
            _particleTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
            };
            _particleTimer.Tick += ParticleTimer_Tick;

            // Zdarzenia myszy
            MouseMove += MainWindow_MouseMove;
            MouseDown += MainWindow_MouseMove;

            // Utwórz cząsteczki
            CreateParticles(50);

            // Uruchom timer bezczynności
            _idleTimer.Start();
        }

        private void CreateParticles(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Ellipse particle = new Ellipse
                {
                    Width = 4 + _random.NextDouble() * 6, // Rozmiar 4-10
                    Height = 4 + _random.NextDouble() * 6,
                    Fill = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)),
                    Opacity = 0.3 + _random.NextDouble() * 0.5
                };
                Canvas.SetLeft(particle, _random.NextDouble() * MainGrid.ActualWidth);
                Canvas.SetTop(particle, _random.NextDouble() * MainGrid.ActualHeight);
                ParticleCanvas.Children.Add(particle);

                // Losowa prędkość
                double velocityX = (_random.NextDouble() - 0.5) * 2; // -1 do 1
                double velocityY = (_random.NextDouble() - 0.5) * 2;
                _particles.Add((particle, velocityX, velocityY));
            }
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            // Zatrzymaj animacje i zresetuj timer
            if (_isAnimationRunning)
            {
                _backgroundAnimation.Stop();
                _particleTimer.Stop();
                _isAnimationRunning = false;
            }
            _idleTimer.Stop();
            _idleTimer.Start();
        }

        private void IdleTimer_Tick(object sender, EventArgs e)
        {
            // Uruchom animacje po bezczynności
            _backgroundAnimation.Begin();
            _particleTimer.Start();
            _isAnimationRunning = true;
            _idleTimer.Stop();
        }

        private void ParticleTimer_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < _particles.Count; i++)
            {
                var (particle, vx, vy) = _particles[i];
                double x = Canvas.GetLeft(particle) + vx;
                double y = Canvas.GetTop(particle) + vy;

                // Odbicie od krawędzi
                if (x < 0 || x > MainGrid.ActualWidth - particle.Width)
                    vx = -vx;
                if (y < 0 || y > MainGrid.ActualHeight - particle.Height)
                    vy = -vy;

                Canvas.SetLeft(particle, x);
                Canvas.SetTop(particle, y);

                // Aktualizacja prędkości w liście
                _particles[i] = (particle, vx, vy);
            }
        }

        // Obsługa zmiany rozmiaru okna
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            // Ograniczenie pozycji cząsteczek przy zmianie rozmiaru
            for (int i = 0; i < _particles.Count; i++)
            {
                var (particle, vx, vy) = _particles[i];
                double x = Canvas.GetLeft(particle);
                double y = Canvas.GetTop(particle);
                x = Math.Min(x, MainGrid.ActualWidth - particle.Width);
                y = Math.Min(y, MainGrid.ActualHeight - particle.Height);
                Canvas.SetLeft(particle, x);
                Canvas.SetTop(particle, y);
                _particles[i] = (particle, vx, vy);
            }
        }
    }
}
