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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Save_the_Humans_
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Random random = new Random();
        DispatcherTimer tmrEnemy = new DispatcherTimer();
        DispatcherTimer tmrTarget = new DispatcherTimer();
        bool humanCaptured = false;
        // Is the game running at the moment?
        bool playingGame = false;

        int score = 0;
        int numAliens = 0;
        string gameOverMessage = string.Empty;

        public MainWindow()
        {
            InitializeComponent();

            tmrEnemy.Tick += TmrEnemy_Tick;
            tmrEnemy.Interval = TimeSpan.FromSeconds(2);

            tmrTarget.Tick += TmrTarget_Tick;
            tmrTarget.Interval = TimeSpan.FromSeconds(0.05);

            // Improvements

            // Hide Human and target until start of game

            human.Visibility = Visibility.Collapsed;
            target.Visibility = Visibility.Collapsed;
        }

        private void TmrTarget_Tick(object sender, EventArgs e)
        {
            progressBar.Value -= 1;
            if (progressBar.Value <= progressBar.Minimum) gameOver();
        }

        private void gameOver()
        {
            if (playingGame)
            {
                playingGame = false;

                tmrEnemy.Stop();
                tmrTarget.Stop();
                humanCaptured = false;
                btnStart.Visibility = Visibility.Visible;

                // Display score

                gameOverMessage = "Game Over \r\n Score: " + score + "\r\n Aliens: " + numAliens;
                txtGameOver.Text = gameOverMessage;

                txtGameOver.Visibility = Visibility.Visible;
                cvsPlayArea.Children.Add(txtGameOver);
                

                // Hide Human and target until start of game
                human.Visibility = Visibility.Collapsed;
                target.Visibility = Visibility.Collapsed;

                // Score
                score = 0;
                numAliens = 0;
                txtScore.Text = score.ToString();
                txtScore.Visibility = Visibility.Collapsed;
            }
        }

        private void TmrEnemy_Tick(object sender, EventArgs e)
        {
            // Not used at the moment.
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        private void StartGame()
        {
            playingGame = true;
            // Show human and target
            human.Visibility = Visibility.Visible;
            target.Visibility = Visibility.Visible;
            resetHumanAndTarget();
            txtScore.Visibility = Visibility.Visible;
            txtGameOver.Visibility = Visibility.Collapsed;


            human.IsHitTestVisible = true;
            humanCaptured = false;
            progressBar.Value = progressBar.Maximum;
            btnStart.Visibility = Visibility.Collapsed;
            cvsPlayArea.Children.Clear();
            cvsPlayArea.Children.Add(target);
            cvsPlayArea.Children.Add(human);
            tmrEnemy.Start();
            tmrTarget.Start();
        }

        private void AddEnemy()
        {
            ContentControl enemy = new ContentControl();
            enemy.Template = Resources["tmplEnemy"] as ControlTemplate;
            AnimateEnemy(enemy, 0, cvsPlayArea.ActualWidth - 100, "(Canvas.Left)");
            AnimateEnemy(enemy, random.Next((int)cvsPlayArea.ActualHeight - 100),
                random.Next((int)cvsPlayArea.ActualHeight - 100), "(Canvas.Top)");
            cvsPlayArea.Children.Add(enemy);

            enemy.MouseEnter += Enemy_MouseEnter;

            numAliens++;
        }

        private void Enemy_MouseEnter(object sender, MouseEventArgs e)
        {
            if (humanCaptured) gameOver();
        }

        private void AnimateEnemy(ContentControl enemy, double from, double to, string propertyToAnimate)
        {
            Storyboard storyboard = new Storyboard()
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            DoubleAnimation animation = new DoubleAnimation()
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(random.Next(4, 6)))
            };

            Storyboard.SetTarget(animation, enemy);
            Storyboard.SetTargetProperty(animation, new PropertyPath(propertyToAnimate));
            storyboard.Children.Add(animation);
            storyboard.Begin();

        }

        private void Human_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (playingGame)
            {
                humanCaptured = true;
                human.IsHitTestVisible = false;
            }
        }

        private void Target_MouseEnter(object sender, MouseEventArgs e)
        {

            if (playingGame && humanCaptured)
            {
                resetHumanAndTarget();
                AddEnemy();

                humanCaptured = false;
                human.IsHitTestVisible = true;

                // Calculate new score;

        

                score += (int)progressBar.Value;

                txtScore.Text = score.ToString();

                progressBar.Value = progressBar.Maximum;
            }


        }

        private void resetHumanAndTarget()
        {
            Canvas.SetLeft(target, random.Next(100, (int)cvsPlayArea.ActualWidth - 100));
            Canvas.SetTop(target, random.Next(100, (int)cvsPlayArea.ActualHeight - 100));
            Canvas.SetLeft(human, random.Next(100, (int)cvsPlayArea.ActualWidth - 100));
            Canvas.SetTop(human, random.Next(100, (int)cvsPlayArea.ActualHeight - 100));
        }

        private void CvsPlayArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (humanCaptured)
            {
                Point pointerPosition = e.GetPosition(null);
                Point relativePosition = grid.TransformToVisual(cvsPlayArea).Transform(pointerPosition);

                if ((Math.Abs(relativePosition.X - Canvas.GetLeft(human)) > human.ActualWidth * 3) ||
                    (Math.Abs(relativePosition.Y - Canvas.GetTop(human)) > human.ActualHeight * 3 ))
                {
                    humanCaptured = false;
                    human.IsHitTestVisible = true;
                }
                else
                {
                    Canvas.SetLeft(human, relativePosition.X - human.ActualWidth / 2);
                    Canvas.SetTop(human, relativePosition.Y - human.ActualHeight / 2);
                }
            }
        }

        private void CvsPlayArea_MouseLeave(object sender, MouseEventArgs e)
        {
            if (humanCaptured)
            {
                gameOver();
            }
        }
    }
}
