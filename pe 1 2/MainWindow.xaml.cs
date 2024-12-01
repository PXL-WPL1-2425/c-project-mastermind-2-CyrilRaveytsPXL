using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Mastermind
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private int elapsedTime;
        private const int TimeLimit = 10;
        private int score;



        private readonly string[] AvailableColors = { "Rood", "Geel", "Oranje", "Wit", "Groen", "Blauw" };
        private string[] GeneratedCode;
        private int attempts;
        private const int MaxAttempts = 10; 

        private readonly ObservableCollection<Attempt> Attempts = new ObservableCollection<Attempt>();

        public MainWindow()
        {
            InitializeComponent();

            // Koppel de pogingenlijst aan de ListBox
            AttemptsListBox.ItemsSource = Attempts;

            // Initialiseer het spel
            InitializeGame();

            // Koppel de sneltoets voor debug-mode
            KeyDown += MainWindow_KeyDown;
        }

        /// <summary>
        /// Initialisatie van het spel.
        /// Dit genereert een nieuwe code, reset de pogingen en start de timer.
        /// </summary>
        private void InitializeGame()
        {
            Random random = new Random();
            GeneratedCode = Enumerable.Range(0, 4)
                                      .Select(_ => AvailableColors[random.Next(AvailableColors.Length)])
                                      .ToArray();

            DebugTextBox.Text = string.Join(", ", GeneratedCode);

            attempts = 0;

            Attempts.Clear();

            UpdateWindowTitle();

            ResetComboBoxes();

            StartCountdown();
        }

        private void StartCountdown()
        {
            elapsedTime = 0;

            if (timer == null)
            {
                timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                timer.Tick += Timer_Tick;
            }

            timer.Start();
            TimerLabel.Content = $"Timer: {elapsedTime}";
        }

        private void StopCountdown()
        {
            timer?.Stop();
            MessageBox.Show("Tijd is op! Je beurt is verloren.", "Tijdslimiet bereikt", MessageBoxButton.OK, MessageBoxImage.Warning);

            attempts++;
            UpdateWindowTitle();

            if (attempts >= MaxAttempts)
            {
                EndGame();
                return;
            }

            ResetComboBoxes();
            StartCountdown();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            elapsedTime++;

            TimerLabel.Content = $"Timer: {elapsedTime}";

            if (elapsedTime >= TimeLimit)
            {
                StopCountdown();
            }
        }

        private void ValidateButton_Click(object sender, RoutedEventArgs e)
        {
            if (attempts >= MaxAttempts)
            {
                MessageBox.Show("Je hebt het maximale aantal pogingen bereikt! Het spel is voorbij.", "Spel voorbij", MessageBoxButton.OK, MessageBoxImage.Information);
                EndGame();
                return;
            }

            string[] userInput = {
        ComboBox1.SelectedItem as string,
        ComboBox2.SelectedItem as string,
        ComboBox3.SelectedItem as string,
        ComboBox4.SelectedItem as string
    };

            string feedback = GetFeedback(userInput, out int attemptScore);

            Attempts.Add(new Attempt
            {
                Guess = string.Join(", ", userInput),
                Feedback = feedback
            });

            score += attemptScore;
            ScoreLabel.Content = $"Score: {score}";

            if (userInput.SequenceEqual(GeneratedCode))
            {
                MessageBox.Show("Gefeliciteerd! Je hebt de code gekraakt!", "Spel gewonnen", MessageBoxButton.OK, MessageBoxImage.Information);
                EndGame();
                return;
            }

            attempts++;
            UpdateWindowTitle();
            StartCountdown();
        }


        private string GetFeedback(string[] userInput, out int attemptScore)
        {
            int correctPosition = 0;
            int correctColor = 0;
            attemptScore = 0; 

            bool[] codeMatched = new bool[GeneratedCode.Length];
            bool[] inputMatched = new bool[userInput.Length];

            for (int i = 0; i < GeneratedCode.Length; i++)
            {
                if (userInput[i] == GeneratedCode[i])
                {
                    correctPosition++;
                    codeMatched[i] = true;
                    inputMatched[i] = true;
                }
            }

            for (int i = 0; i < userInput.Length; i++)
            {
                if (inputMatched[i]) continue;

                for (int j = 0; j < GeneratedCode.Length; j++)
                {
                    if (codeMatched[j]) continue;

                    if (userInput[i] == GeneratedCode[j])
                    {
                        correctColor++;
                        codeMatched[j] = true;
                        break;
                    }
                }
            }

            int wrongColor = userInput.Length - correctPosition - correctColor;

            return $"{correctPosition} Rood, {correctColor} Wit";
        }


        private void ResetComboBoxes()
        {
            ComboBox[] comboBoxes = { ComboBox1, ComboBox2, ComboBox3, ComboBox4 };
            foreach (var comboBox in comboBoxes)
            {
                comboBox.ItemsSource = AvailableColors;
                comboBox.SelectedIndex = -1;
            }
        }

        private void UpdateWindowTitle()
        {
            this.Title = $"Mastermind - Poging {attempts + 1}/{MaxAttempts}";
        }

        private void ToggleDebug()
        {
            DebugTextBox.Visibility = DebugTextBox.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F12 && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                ToggleDebug();
            }
        }

        private void EndGame()
        {
            timer?.Stop();

            MessageBox.Show($"De juiste code was: {string.Join(", ", GeneratedCode)}", "Spel afgelopen", MessageBoxButton.OK, MessageBoxImage.Information);

            if (MessageBox.Show("Wil je opnieuw spelen?", "Opnieuw spelen", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                InitializeGame();
            }
            else
            {
                Close();
            }
        }
    }

    public class Attempt
    {
        public string Guess { get; set; }
        public string Feedback { get; set; }
    }
}
