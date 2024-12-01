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
        // Timer-gerelateerde velden
        private DispatcherTimer timer;
        private int elapsedTime;
        private const int TimeLimit = 10;

        // Spel-logica velden
        private readonly string[] AvailableColors = { "Rood", "Geel", "Oranje", "Wit", "Groen", "Blauw" };
        private string[] GeneratedCode;
        private int attempts;
        private const int MaxAttempts = 10; // Maximaal aantal pogingen

        // Score en pogingenlijst
        private int score;
        private readonly ObservableCollection<Attempt> Attempts = new ObservableCollection<Attempt>();

        public MainWindow()
        {
            InitializeComponent();

            // Voeg de Closing handler toe
            this.Closing += MainWindow_Closing;

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
            // Genereer een random code
            Random random = new Random();
            GeneratedCode = Enumerable.Range(0, 4)
                                      .Select(_ => AvailableColors[random.Next(AvailableColors.Length)])
                                      .ToArray();

            // Toon de code in debug-mode
            DebugTextBox.Text = string.Join(", ", GeneratedCode);

            // Reset het aantal pogingen
            attempts = 0;
            score = 0;

            // Reset pogingenlijst
            Attempts.Clear();

            // Update de venstertitel
            UpdateWindowTitle();

            // Reset ComboBoxen
            ResetComboBoxes();

            // Start de timer
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
                EndGame(false);  // Verlies bij 10 pogingen
            }
            else
            {
                ResetComboBoxes();
                StartCountdown();
            }
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
                EndGame(false); // Verlies bij 10 pogingen
                return;
            }

            string[] userInput = {
                ComboBox1.SelectedItem as string,
                ComboBox2.SelectedItem as string,
                ComboBox3.SelectedItem as string,
                ComboBox4.SelectedItem as string
            };

            // Bereken de feedback en score
            string feedback = GetFeedback(userInput, out int attemptScore);

            // Voeg de poging en feedback toe aan de lijst
            Attempts.Add(new Attempt
            {
                Guess = string.Join(", ", userInput),
                Feedback = feedback
            });

            // Update de score
            score += attemptScore;
            ScoreLabel.Content = $"Score: {score}";

            // Controleer of de code correct is
            if (userInput.SequenceEqual(GeneratedCode))
            {
                MessageBox.Show("Gefeliciteerd! Je hebt de code gekraakt!", "Spel gewonnen", MessageBoxButton.OK, MessageBoxImage.Information);
                EndGame(true);  // Gewonnen
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
            attemptScore = 0; // Start score per poging

            bool[] codeMatched = new bool[GeneratedCode.Length];
            bool[] inputMatched = new bool[userInput.Length];

            // Bereken de juiste posities (0 strafpunten)
            for (int i = 0; i < GeneratedCode.Length; i++)
            {
                if (userInput[i] == GeneratedCode[i])
                {
                    correctPosition++;
                    codeMatched[i] = true;
                    inputMatched[i] = true;
                }
            }

            // Bereken de juiste kleuren (1 strafpunt)
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

            // Bereken de strafpunten
            int wrongColor = userInput.Length - correctPosition - correctColor;
            attemptScore = correctPosition * 0 + correctColor * 1 + wrongColor * 2; // 0 strafpunten voor juiste posities, 1 voor kleuren die verkeerd staan, 2 voor kleuren die niet in de code voorkomen

            // Return feedback in een string
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

        private void EndGame(bool won)
        {
            timer?.Stop();

            if (won)
            {
                MessageBox.Show("Je hebt de code gekraakt!", "Gefeliciteerd", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Je hebt verloren! De code was: {string.Join(", ", GeneratedCode)}", "Spel afgelopen", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // Vraag of de speler opnieuw wil spelen
            if (MessageBox.Show("Wil je opnieuw spelen?", "Opnieuw spelen", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                InitializeGame(); // Begin een nieuwe ronde
            }
            else
            {
                Close(); // Sluit de applicatie
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F12 && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                ToggleDebug();
            }
        }

        private void ToggleDebug()
        {
            DebugTextBox.Visibility = DebugTextBox.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (attempts < MaxAttempts && !CodeGuessed())
            {
                // Als het spel niet is afgelopen, vraag dan of de speler wil afsluiten
                var result = MessageBox.Show("Je hebt het spel nog niet voltooid. Ben je zeker dat je het spel wilt beëindigen?", "Beëindigen", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true; // Annuleer de afsluiting en laat het spel doorgaan
                }
                else
                {
                    // Als de speler kiest voor Ja, wordt de applicatie afgesloten
                    Application.Current.Shutdown();
                }
            }
        }

        private bool CodeGuessed()
        {
            // Controleer of de code correct is geraden
            string[] userInput = {
                ComboBox1.SelectedItem as string,
                ComboBox2.SelectedItem as string,
                ComboBox3.SelectedItem as string,
                ComboBox4.SelectedItem as string
            };

            return userInput.SequenceEqual(GeneratedCode);
        }
    }

    public class Attempt
    {
        public string Guess { get; set; }
        public string Feedback { get; set; }
    }
}
