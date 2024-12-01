using System;
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

        public MainWindow()
        {
            InitializeComponent();
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

            // Update de venstertitel
            UpdateWindowTitle();

            // Reset ComboBoxen
            ResetComboBoxes();

            // Start de timer
            StartCountdown();
        }

        /// <summary>
        /// Start of reset de countdown-timer voor de huidige beurt.
        /// </summary>
        private void StartCountdown()
        {
            // Reset de timer en verstreken tijd
            elapsedTime = 0;

            // Controleer of de timer al bestaat
            if (timer == null)
            {
                timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                timer.Tick += Timer_Tick;
            }

            // Start de timer
            timer.Start();

            // Update de timerweergave
            TimerLabel.Content = $"Timer: {elapsedTime}";
        }

        /// <summary>
        /// Stopt de countdown-timer en voert acties uit als de tijdslimiet is bereikt.
        /// </summary>
        private void StopCountdown()
        {
            if (timer != null)
            {
                timer.Stop(); // Stop de timer
            }

            // Actie als de speler de beurt verliest
            MessageBox.Show("Tijd is op! Je beurt is verloren.", "Tijdslimiet bereikt", MessageBoxButton.OK, MessageBoxImage.Warning);

            // Verhoog het aantal pogingen
            attempts++;
            UpdateWindowTitle();

            // Controleer of het spel voorbij is
            if (attempts >= MaxAttempts)
            {
                EndGame();
                return;
            }

            // Reset UI en start opnieuw
            ResetComboBoxes();
            StartCountdown();
        }

        /// <summary>
        /// Logica die elke seconde wordt uitgevoerd door de timer.
        /// </summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            // Verhoog de verstreken tijd
            elapsedTime++;

            // Update de timerweergave
            TimerLabel.Content = $"Timer: {elapsedTime}";

            // Controleer of de tijdslimiet is bereikt
            if (elapsedTime >= TimeLimit)
            {
                StopCountdown(); // Stop de timer en verlies de beurt
            }
        }

        /// <summary>
        /// Valideert de ingevoerde code en controleert deze tegen de gegenereerde code.
        /// </summary>
        private void ValidateButton_Click(object sender, RoutedEventArgs e)
        {
            // Controleer of het maximum aantal pogingen is bereikt
            if (attempts >= MaxAttempts)
            {
                MessageBox.Show("Je hebt het maximale aantal pogingen bereikt! Het spel is voorbij.", "Spel voorbij", MessageBoxButton.OK, MessageBoxImage.Information);
                EndGame();
                return;
            }

            // Valideer de code
            string[] userInput = {
                ComboBox1.SelectedItem as string,
                ComboBox2.SelectedItem as string,
                ComboBox3.SelectedItem as string,
                ComboBox4.SelectedItem as string
            };

            // Controleer of de invoer overeenkomt met de gegenereerde code
            if (userInput.SequenceEqual(GeneratedCode))
            {
                MessageBox.Show("Gefeliciteerd! Je hebt de code gekraakt!", "Spel gewonnen", MessageBoxButton.OK, MessageBoxImage.Information);
                EndGame();
                return;
            }

            // Verhoog het aantal pogingen
            attempts++;
            UpdateWindowTitle();

            // Start de timer opnieuw
            StartCountdown();
        }

        /// <summary>
        /// Schakelt de debug-modus in of uit.
        /// </summary>
        private void ToggleDebug()
        {
            DebugTextBox.Visibility = DebugTextBox.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        /// <summary>
        /// Koppelt de sneltoets voor het toggelen van de debug-modus.
        /// </summary>
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F12 && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                ToggleDebug();
            }
        }

        /// <summary>
        /// Reset alle ComboBoxen naar hun initiële staat.
        /// </summary>
        private void ResetComboBoxes()
        {
            ComboBox[] comboBoxes = { ComboBox1, ComboBox2, ComboBox3, ComboBox4 };
            foreach (var comboBox in comboBoxes)
            {
                comboBox.ItemsSource = AvailableColors;
                comboBox.SelectedIndex = -1; // Geen selectie
            }
        }

        /// <summary>
        /// Update de venstertitel met het huidige aantal pogingen.
        /// </summary>
        private void UpdateWindowTitle()
        {
            this.Title = $"Mastermind - Poging {attempts + 1}/{MaxAttempts}";
        }

        /// <summary>
        /// Eindigt het spel en geeft de gebruiker de optie om opnieuw te spelen.
        /// </summary>
        private void EndGame()
        {
            // Stop de timer
            if (timer != null)
            {
                timer.Stop();
            }

            // Toon de juiste code aan de speler
            MessageBox.Show($"De juiste code was: {string.Join(", ", GeneratedCode)}", "Spel afgelopen", MessageBoxButton.OK, MessageBoxImage.Information);

            // Vraag of de speler opnieuw wil spelen
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
}
