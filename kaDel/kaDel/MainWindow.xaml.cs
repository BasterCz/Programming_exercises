using System;
using System.Diagnostics;
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
using System.Threading;
using System.IO;
using Path = System.IO.Path;

namespace kaDel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    enum DirectionsEnum { Up, Right, Down, Left };
    enum AlowedCommands { krok, otoc, vypln, vymaz }
    struct IntPoint
    {
        public int x;
        public int y;
        public IntPoint(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
    public static class Extensions
    {
        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) + 1;
            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }
    }
    public partial class MainWindow : Window
    {
        int[,] playfieldArray = new int[10, 10];
        IntPoint robotPosition = new IntPoint(9, 0);
        Queue<string> actionsQueue = new Queue<string>();
        DirectionsEnum direction = DirectionsEnum.Right;
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            dispatcherTimer.Tick += DispatcherTimer_Tick;

            for (int i = 0; i < 10; i++)
            {
                ColumnDefinition colDef = new ColumnDefinition();
                RowDefinition rowDef = new RowDefinition();
                colDef.Width = new GridLength(1, GridUnitType.Star);
                rowDef.Height = new GridLength(1, GridUnitType.Star);
                GridRect.ColumnDefinitions.Add(colDef);
                GridRect.RowDefinitions.Add(rowDef);
            }
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Rectangle rect = new Rectangle();
                    rect.Fill = Brushes.White;
                    rect.Stroke = Brushes.LightGray;
                    rect.Tag = new IntPoint(i, j);
                    Grid.SetColumn(rect, j);
                    Grid.SetRow(rect, i);
                    GridRect.Children.Add(rect);
                }
            }
            Rerender();
        }

        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            if (actionsQueue.Count > 0)
            {
                string action = actionsQueue.Dequeue();
                bool outOfRange = false;
                switch (action)
                {
                    case "krok":
                        switch (direction)
                        {
                            case DirectionsEnum.Up:
                                if (robotPosition.x > 0)
                                {
                                    robotPosition.x--;
                                }
                                else outOfRange = true;
                                break;
                            case DirectionsEnum.Right:
                                if (robotPosition.y < 9)
                                {
                                    robotPosition.y++;
                                }
                                else outOfRange = true;
                                break;
                            case DirectionsEnum.Down:
                                if (robotPosition.x < 9)
                                {
                                    robotPosition.x++;
                                }
                                else outOfRange = true;
                                break;
                            case DirectionsEnum.Left:
                                if (robotPosition.y > 0)
                                {
                                    robotPosition.y--;
                                }
                                else outOfRange = true;
                                break;
                        }
                        break;
                    case "otoc":
                        direction = direction.Next();
                        break;
                    case "vypln":
                        playfieldArray[robotPosition.x, robotPosition.y] = 1;
                        break;
                    case "vymaz":
                        playfieldArray[robotPosition.x, robotPosition.y] = 0;
                        break;
                    default:
                        MessageBox.Show("Neplatny prikaz: " + action);
                        dispatcherTimer.Stop();
                        break;
                }
                if (outOfRange)
                {
                    MessageBox.Show("Robot je mimo hraci pole");
                    dispatcherTimer.Stop();
                }
                Rerender();
            }
            else
            {
                dispatcherTimer.Stop();
            }
        }

        private void BtnKrok_Click(object sender, RoutedEventArgs e)
        {
            bool step = false;
            for (int i = 0; i < 10; i++)
            {
                if (step) break;
                for (int j = 0; j < 10; j++)
                {
                    if (step) break;
                    if (robotPosition.x == i && robotPosition.y == j)
                    {
                        switch (direction)
                        {
                            case DirectionsEnum.Left:
                                if (j > 0)
                                {
                                    robotPosition = new IntPoint(i, j - 1);
                                    step = true;
                                }
                                break;
                            case DirectionsEnum.Right:
                                if (j < 9)
                                {
                                    robotPosition = new IntPoint(i, j + 1);
                                    step = true;
                                }
                                break;
                            case DirectionsEnum.Up:
                                if (i > 0)
                                {
                                    robotPosition = new IntPoint(i - 1, j);
                                    step = true;
                                }
                                break;
                            case DirectionsEnum.Down:
                                if (i < 9)
                                {
                                    robotPosition = new IntPoint(i + 1, j);
                                    step = true;
                                }
                                break;
                        }
                    }
                }
            }
            //write to debug console the current playfield
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Debug.Write(playfieldArray[i, j] + " ");
                }
                Debug.WriteLine("");
            }
            Rerender();
        }

        private void BtnOtoc_Click(object sender, RoutedEventArgs e)
        {
            direction = direction.Next();
            Rerender();
        }

        private void BtnVypln_Click(object sender, RoutedEventArgs e)
        {
            playfieldArray[robotPosition.x, robotPosition.y] = 1;
            Rerender();
        }

        private void BtnVymaz_Click(object sender, RoutedEventArgs e)
        {
            playfieldArray[robotPosition.x, robotPosition.y] = 0;
            Rerender();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            playfieldArray = new int[10, 10];
            robotPosition = new IntPoint(9, 0);
            direction = DirectionsEnum.Right;
            Rerender();
        }

        private void BtnSpustit_Click(object sender, RoutedEventArgs e)
        {
            actionsQueue = new Queue<string>(TBInterpret.Text.Split('\n').Select(x => x.Trim().ToLower()).Where(x => x != "").ToArray());
            playfieldArray = new int[10, 10];
            robotPosition = new IntPoint(9, 0);
            direction = DirectionsEnum.Right;
            Rerender();
            dispatcherTimer.Start();
            Debug.WriteLine("Spuštění");
        }

        private void BtnNahrat_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnUlozit_Click(object sender, RoutedEventArgs e)
        {

        }


        private void Rerender()
        {
            foreach (Rectangle rect in GridRect.Children)
            {
                IntPoint point = (IntPoint)rect.Tag;
                if (point.x == robotPosition.x && point.y == robotPosition.y)
                {
                    switch (direction)
                    {
                        case DirectionsEnum.Left:
                            rect.Fill = new ImageBrush(new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "\\Resources\\vlevo.png")));
                            break;
                        case DirectionsEnum.Right:
                            rect.Fill = new ImageBrush(new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "\\Resources\\vpravo.png")));
                            break;
                        case DirectionsEnum.Up:
                            rect.Fill = new ImageBrush(new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "\\Resources\\nahoru.png")));
                            break;
                        case DirectionsEnum.Down:
                            rect.Fill = new ImageBrush(new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "\\Resources\\dolu.png")));
                            break;
                    }
                    if (playfieldArray[point.x, point.y] == 1)
                    {
                        rect.Stroke = Brushes.Pink;
                        rect.StrokeThickness = 3;
                    }
                    else
                    {
                        rect.Stroke = Brushes.LightGray;
                        rect.StrokeThickness = 1;
                    }

                }
                else if (playfieldArray[point.x, point.y] == 1)
                {
                    rect.Fill = Brushes.Pink;
                    rect.Stroke = Brushes.LightGray;
                    rect.StrokeThickness = 1;
                }
                else
                {
                    rect.Fill = Brushes.White;
                    rect.Stroke = Brushes.LightGray;
                    rect.StrokeThickness = 1;
                }
            }
        }

    }
}
