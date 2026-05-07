using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;

namespace TempWpf
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private bool _isRunning = false;
        private HttpClient _client = new HttpClient();
        private Random _rand = new Random();

        public MainWindow()
        {
            InitializeComponent();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(5);
            _timer.Tick += async (s, e) => await SendTemperature();
        }

        private async Task SendTemperature()
        {
            var data = new
            {
                temperature = Math.Round(_rand.NextDouble() * 30, 2),
                timestamp = DateTime.Now
            };

            var json = JsonSerializer.Serialize(data);

            try
            {
                await _client.PostAsync("https://localhost:5001/api/temperature",
                    new StringContent(json, Encoding.UTF8, "application/json"));

                StatusText.Text = $"Отправлено: {data.temperature}";
            }
            catch
            {
                StatusText.Text = "Ошибка отправки, повтор...";
            }
        }

        private void StartStopBtn_Click(object sender, RoutedEventArgs e)
        {
            _isRunning = !_isRunning;

            if (_isRunning)
            {
                _timer.Start();
                StartStopBtn.Content = "Стоп";
            }
            else
            {
                _timer.Stop();
                StartStopBtn.Content = "Старт";
            }
        }
    }
}
