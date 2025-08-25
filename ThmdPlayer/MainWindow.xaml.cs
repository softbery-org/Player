using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Thmd.Configuration;
using Thmd.Controls;
using Thmd.Helpers;
using Thmd.Media;
using Thmd.Windowses;

namespace ThmdPlayer
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PlayerControl _player;
        private bool _fullscreen = false;

        public MainWindow()
        {
            InitializeComponent();

            /*
            Window1 w1 = new Window1();
            Window2 w2 = new Window2();
            Window3 w3 = new Window3();
            Window4 w4 = new Window4();
            w1.Show();
            w2.Show();
            w3.Show();
            w4.Show();
            */
            this.KeyDown += OnKeybord_KeyDown;
            this.Loaded += OnLoad;

            _player = new PlayerControl();

            _player.Playlist.Foreground = Brushes.White;
            _player.Playlist.Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 10,
                ShadowDepth = 0,
                Opacity = 0.5
            };
            _player.Playlist.Background = new SolidColorBrush(Color.FromArgb(155, 53, 53, 53));

            var s = Config.Instance.PlaylistConfig.MediaList;

            foreach (var item in s)
            {
                _player.Playlist.Add(new Video(item));
            }

            var grid = new Grid();

            grid.Children.Add(_player);

            this.Content = grid;

            _player.Play();

            OpenWindowByControlBarButton(this, null);
        }

        private void OpenWindowByControlBarButton(object sender, RoutedEventArgs e)
        {
            _player.ControlBar.BtnSettingsWindow.Click += (s, args) =>
            {
                SettingsWindow settingsWindow = new SettingsWindow();
                settingsWindow.Owner = this;
                settingsWindow.ShowDialog();
            };
        }

        private void OnKeybord_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    if (e.IsDown)
                    {
                        _player.Seek(TimeSpan.FromSeconds(5), SeekDirection.Backward);
                    }
                    break;
                case Key.Right:
                    if (e.IsDown)
                    {
                        _player.Seek(TimeSpan.FromSeconds(5), SeekDirection.Forward);
                    }
                    break;
                case Key.L:
                    if (_player.Playlist.Visibility == Visibility.Visible)
                    {
                        _player.Playlist.Visibility = Visibility.Hidden;
                    }
                    else if(_player.Playlist.Visibility == Visibility.Hidden)
                    {
                        _player.Playlist.Visibility = Visibility.Visible;
                    }
                    break;
                case Key.F:
                    if (_player.Fullscreen)
                        _player.Fullscreen = false;
                    else
                        _player.Fullscreen = true;
                    break;
                case Key.Escape:
                    if (_player.Fullscreen)
                    {
                        _player.Fullscreen = false;
                    }
                    break;
                case Key.Space:
                    if (_player.isPlaying)
                    {
                        _player.Pause();
                    }
                    else
                    {
                        _player.Play();
                    }
                    break;
                case Key.M:
                    if (_player.isMute)
                    {
                        _player.isMute = false;
                    }
                    else
                    {
                        _player.isMute = true;
                    }
                    break;
                case Key.S:
                    if (_player.SubtitleVisibility == Visibility.Visible)
                    {
                        _player.SubtitleVisibility = Visibility.Hidden;
                    }
                    else
                    {
                        _player.SubtitleVisibility = Visibility.Visible;
                    }
                    break;
                case Key.R:

                    break;
            }
        }

        private void OnLoad(object sender, RoutedEventArgs e) 
        {
            this.ActivateCenteredToMouse();
        }
    }
}
