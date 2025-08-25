using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ThmdPlayer
{
    public partial class Window2 : Window
    {
        private DispatcherTimer _idleTimer;
        private DispatcherTimer _shootingStarTimer;
        private DispatcherTimer _rotationTimer; // Timer for Earth's rotation
        private TimeSpan _idleTimeThreshold = TimeSpan.FromSeconds(3);
        private Storyboard _twinkleAnimation;
        private List<Ellipse> _stars = new List<Ellipse>();
        private List<Ellipse> _shootingStars = new List<Ellipse>();
        private Random _random = new Random();
        private bool _isAnimationRunning = false;
        private double _rotationSpeed = -0.5; // Pixels per frame, negative for westward motion

        public Window2()
        {
            InitializeComponent();

            try
            {
                _twinkleAnimation = (Storyboard)FindResource("StarTwinkleAnimation");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd zasobów: {ex.Message}");
                return;
            }

            _idleTimer = new DispatcherTimer { Interval = _idleTimeThreshold };
            _idleTimer.Tick += IdleTimer_Tick;

            _shootingStarTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3 + _random.NextDouble() * 3) }; // 3-6 seconds
            _shootingStarTimer.Tick += ShootingStarTimer_Tick;

            // Initialize rotation timer (update every 50ms for smooth motion)
            _rotationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _rotationTimer.Tick += RotationTimer_Tick;

            MouseMove += Window2_MouseMove;
            MouseDown += Window2_MouseMove;

            Loaded += (s, e) =>
            {
                CreateStars(200); // Create background stars
                _rotationTimer.Start(); // Start rotation effect
            };

            _idleTimer.Start();
        }

        private void RotationTimer_Tick(object sender, EventArgs e)
        {
            // Update positions of background stars
            foreach (var star in _stars)
            {
                double newX = Canvas.GetLeft(star) + _rotationSpeed;
                // Wrap around to the right side when moving off the left
                if (newX < -star.Width)
                {
                    newX += ActualWidth + star.Width;
                    Canvas.SetTop(star, _random.NextDouble() * ActualHeight);
                }
                Canvas.SetLeft(star, newX);
            }

            // Update positions of shooting stars (comets and their tails)
            foreach (var shootingStar in _shootingStars)
            {
                double newX = Canvas.GetLeft(shootingStar) + _rotationSpeed;
                Canvas.SetLeft(shootingStar, newX);
            }
        }

        private void CreateStars(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Ellipse star = new Ellipse
                {
                    Width = 1 + _random.NextDouble() * 3,
                    Height = 1 + _random.NextDouble() * 3,
                    Fill = Brushes.White,
                    Opacity = 0.5 + _random.NextDouble() * 0.5
                };
                Canvas.SetLeft(star, _random.NextDouble() * ActualWidth);
                Canvas.SetTop(star, _random.NextDouble() * ActualHeight);
                StarCanvas.Children.Add(star);
                _stars.Add(star);
            }
        }

        private void Window2_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isAnimationRunning)
            {
                StopAnimations();
                _isAnimationRunning = false;
            }
            _idleTimer.Stop();
            _idleTimer.Start();
        }

        private void IdleTimer_Tick(object sender, EventArgs e)
        {
            StartAnimations();
            _isAnimationRunning = true;
            _idleTimer.Stop();
        }

        private void StartAnimations()
        {
            foreach (var star in _stars)
            {
                var delay = TimeSpan.FromMilliseconds(_random.Next(0, 1000));
                _twinkleAnimation.BeginTime = delay;
                _twinkleAnimation.Begin(star);
            }
            _shootingStarTimer.Start();
        }

        private void StopAnimations()
        {
            foreach (var star in _stars)
            {
                _twinkleAnimation.Stop(star);
                star.Opacity = 1;
            }
            _shootingStarTimer.Stop();
            foreach (var shootingStar in _shootingStars)
            {
                StarCanvas.Children.Remove(shootingStar);
            }
            _shootingStars.Clear();
        }

        private void ShootingStarTimer_Tick(object sender, EventArgs e)
        {
            CreateShootingStar();
            _shootingStarTimer.Interval = TimeSpan.FromSeconds(3 + _random.NextDouble() * 3); // 3-6 seconds
        }

        private void CreateShootingStar()
        {
            // Create the main shooting star
            Ellipse shootingStar = new Ellipse
            {
                Width = 3 + _random.NextDouble() * 3,
                Height = 3 + _random.NextDouble() * 3,
                Fill = Brushes.White,
                Opacity = 1
            };
            double cometSize = (shootingStar.Width + shootingStar.Height) / 2;

            // Random start and end points for varied angles
            double startX, startY, endX, endY;
            int startEdge = _random.Next(0, 3); // 0 = top, 1 = left, 2 = right
            if (startEdge == 0) // Start from top
            {
                startX = _random.NextDouble() * ActualWidth;
                startY = -cometSize * 5;
            }
            else if (startEdge == 1) // Start from left
            {
                startX = -cometSize * 5;
                startY = _random.NextDouble() * ActualHeight * 0.5;
            }
            else // Start from right
            {
                startX = ActualWidth + cometSize * 5;
                startY = _random.NextDouble() * ActualHeight * 0.5;
            }

            // End point within canvas, at or before 75% height
            endX = _random.NextDouble() * ActualWidth;
            endY = _random.NextDouble() * (ActualHeight * 0.75);

            Canvas.SetLeft(shootingStar, startX);
            Canvas.SetTop(shootingStar, startY);
            StarCanvas.Children.Add(shootingStar);
            _shootingStars.Add(shootingStar);

            // Calculate tail properties based on comet's characteristics
            double durationSeconds = 2 + _random.NextDouble() * 3;
            var duration = TimeSpan.FromSeconds(durationSeconds);
            double distanceX = Math.Abs(endX - startX);
            double distanceY = Math.Abs(endY - startY);
            double speed = Math.Sqrt(Math.Pow(distanceX, 2) + Math.Pow(distanceY, 2)) / durationSeconds;
            int baseTailLength = (int)(20 + (speed / 50) + (cometSize * 2)); // Base length
            int tailLength = (int)(baseTailLength * 2.5); // 2.5x longer tail
            tailLength = Math.Min(tailLength, 100); // Cap to prevent performance issues

            // Create tail elements for glowing effect
            List<Ellipse> tailElements = new List<Ellipse>();
            for (int i = 0; i < tailLength; i++)
            {
                // Calculate dynamic properties for each tail element
                double sizeFactor = 1.8 - (i * (1.3 / tailLength)); // Adjusted size decrement
                double opacity = 0.7 - (i * (0.6 / tailLength)); // Adjusted opacity decrement
                if (sizeFactor < 0.5) sizeFactor = 0.5; // Minimum size
                if (opacity < 0.1) opacity = 0.1; // Minimum opacity

                // Use RadialGradientBrush for glowing effect
                RadialGradientBrush gradientBrush = new RadialGradientBrush
                {
                    GradientOrigin = new Point(0.5, 0.5),
                    Center = new Point(0.5, 0.5),
                    RadiusX = 0.5,
                    RadiusY = 0.5
                };
                gradientBrush.GradientStops.Add(new GradientStop(Colors.White, 0));
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(200, 255, 255, 255), 0.3));
                gradientBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 1));

                Ellipse tail = new Ellipse
                {
                    Width = cometSize * sizeFactor,
                    Height = cometSize * sizeFactor,
                    Fill = gradientBrush,
                    Opacity = opacity
                };

                // Position tail elements directly along the comet's trajectory
                double tailProgress = (double)i / (tailLength * 2); // Reduced progress for tighter trailing
                double tailX = startX + (endX - startX) * tailProgress * -0.5; // Trail behind comet
                double tailY = startY + (endY - startY) * tailProgress * -0.5; // Trail behind comet
                Canvas.SetLeft(tail, tailX);
                Canvas.SetTop(tail, tailY);
                StarCanvas.Children.Add(tail);
                _shootingStars.Add(tail);
                tailElements.Add(tail);
            }

            // Animation for the main shooting star
            var storyboard = new Storyboard();
            var animTop = new DoubleAnimation(startY, endY, duration) { AccelerationRatio = 0.3 };
            var animLeft = new DoubleAnimation(startX, endX, duration);
            var animOpacity = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(durationSeconds * 0.2))
            {
                BeginTime = TimeSpan.FromSeconds(durationSeconds * 0.1) // Comet fades early: 10% to 30%
            };

            Storyboard.SetTarget(animTop, shootingStar);
            Storyboard.SetTargetProperty(animTop, new PropertyPath("(Canvas.Top)"));
            Storyboard.SetTarget(animLeft, shootingStar);
            Storyboard.SetTargetProperty(animLeft, new PropertyPath("(Canvas.Left)"));
            Storyboard.SetTarget(animOpacity, shootingStar);
            Storyboard.SetTargetProperty(animOpacity, new PropertyPath("Opacity"));

            storyboard.Children.Add(animTop);
            storyboard.Children.Add(animLeft);
            storyboard.Children.Add(animOpacity);

            // Animations for tail elements
            for (int i = 0; i < tailLength; i++)
            {
                var tail = tailElements[i];
                var tailDelay = TimeSpan.FromMilliseconds(8 * (i + 1)); // Smooth delay for trailing
                var tailStartX = Canvas.GetLeft(tail);
                var tailStartY = Canvas.GetTop(tail);
                var tailTop = new DoubleAnimation(tailStartY, endY, duration)
                {
                    AccelerationRatio = 0.3,
                    BeginTime = tailDelay
                };
                var tailLeft = new DoubleAnimation(tailStartX, endX, duration) { BeginTime = tailDelay };
                // Sequential fade: front fades first, end fades last
                var tailFadeDelay = TimeSpan.FromSeconds(durationSeconds * 0.3 + (i * (durationSeconds * 0.2 / tailLength)));
                var tailOpacity = new DoubleAnimation(tail.Opacity, 0, TimeSpan.FromSeconds(durationSeconds * 0.2))
                {
                    BeginTime = tailFadeDelay // Tail fades from 30% to 50% duration, front to back
                };

                Storyboard.SetTarget(tailTop, tail);
                Storyboard.SetTargetProperty(tailTop, new PropertyPath("(Canvas.Top)"));
                Storyboard.SetTarget(tailLeft, tail);
                Storyboard.SetTargetProperty(tailLeft, new PropertyPath("(Canvas.Left)"));
                Storyboard.SetTarget(tailOpacity, tail);
                Storyboard.SetTargetProperty(tailOpacity, new PropertyPath("Opacity"));

                storyboard.Children.Add(tailTop);
                storyboard.Children.Add(tailLeft);
                storyboard.Children.Add(tailOpacity);
            }

            // Cleanup after animation
            storyboard.Completed += (s, ev) =>
            {
                StarCanvas.Children.Remove(shootingStar);
                _shootingStars.Remove(shootingStar);
                foreach (var tail in tailElements)
                {
                    StarCanvas.Children.Remove(tail);
                    _shootingStars.Remove(tail);
                }
            };
            storyboard.Begin();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            StarCanvas.Children.Clear();
            _stars.Clear();
            _shootingStars.Clear();
            CreateStars(200);
        }
    }
}