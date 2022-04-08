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
using Microsoft.Win32;

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
        List<string> actionsQueue = new List<string>();
        DirectionsEnum direction = DirectionsEnum.Right;
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        bool isSaved = false;
        int operations = 0;
        
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

        private void StopProgram(bool failed)
        {
            dispatcherTimer.Stop();
            if (failed)
            {
                TBOutput.Text += "\nProgram selhal a byl ukoncen";
            }
            else
            {
                TBOutput.Text += "\nProgram byl dokoncen";
            }
        }

        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            if (actionsQueue.Count > 0)
            {
                operations--;
                string action = actionsQueue[actionsQueue.Count - 1];
                actionsQueue.RemoveAt(actionsQueue.Count - 1);
                bool outOfRange = false;
                bool problem = false;
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
                        if (playfieldArray[robotPosition.x, robotPosition.y] == 0)
                        {
                            playfieldArray[robotPosition.x, robotPosition.y] = 1;
                        }
                        else { 
                            TBOutput.Text += "\n" + action + ": Nelze";
                            problem = true;
                            StopProgram(problem);
                        }
                        break;
                    case "vymaz":
                        if (playfieldArray[robotPosition.x, robotPosition.y] == 1)
                        {
                            playfieldArray[robotPosition.x, robotPosition.y] = 0;
                        }
                        else
                        {
                            TBOutput.Text += "\n" + action + ": Nelze";
                            problem = true;
                            StopProgram(problem);
                        }
                        break;
                    default:
                        TBOutput.Text += "\n" + action + ": Neznam";
                        problem = true;
                        StopProgram(problem);
                        break;
                }
                if (outOfRange)
                {
                    TBOutput.Text += "\n" + action + ": KaDel spadne pres okraj";
                    problem = true;
                    StopProgram(problem);
                }
                Rerender();
                if (!problem) TBOutput.Text += "\n" + action + ": OK";
            }
            else
            {
                StopProgram(false);
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
            actionsQueue = new List<string>(TBInterpret.Text.Split('\n').Select(x => x.Trim().ToLower()).Where(x => x != "").Where(x => !x.StartsWith("#")).ToArray());
            int op = operations = actionsQueue.Count;
            for (int i = 0; i < actionsQueue.Count && op != 0; i++)
            {
                if (actionsQueue.ElementAt(i).StartsWith("otoc") || actionsQueue.ElementAt(i).StartsWith("krok"))
                {
                    if (actionsQueue.ElementAt(i).Length > 4)
                    {
                        int num = 0;
                        if (int.TryParse(actionsQueue.ElementAt(i).Substring(4), out num))
                        {
                            for (int j = 0; j < num; j++)
                            {
                                actionsQueue.Add(actionsQueue.ElementAt(i).Substring(0, 4));
                            }
                            actionsQueue.RemoveAt(i);
                            i--;
                            op--;
                        }
                    }
                    else
                    {
                        actionsQueue.Add(actionsQueue.ElementAt(i));
                        actionsQueue.RemoveAt(i);
                        i--;
                        op--;
                    }
                }
                else
                {
                    actionsQueue.Add(actionsQueue.ElementAt(i));
                    actionsQueue.RemoveAt(i);
                    i--;
                    op--;
                }
            }
            actionsQueue.Reverse();
            playfieldArray = new int[10, 10];
            robotPosition = new IntPoint(9, 0);
            direction = DirectionsEnum.Right;
            Rerender();
            dispatcherTimer.Start();
            TBOutput.Text += "\nProgram spusten";
        }

        private void BtnNahrat_Click(object sender, RoutedEventArgs e)
        {
            if (TBInterpret.Text != "" && !isSaved)
            {
                MessageBoxResult result = MessageBox.Show("Chcete ulozit zmeny?", "Ulozit", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    BtnUlozit_Click(sender, e);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                TBInterpret.Text = File.ReadAllText(openFileDialog.FileName);
                isSaved = true;
            }
        }

        private void BtnUlozit_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog.FileName = "program.txt";
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == true)
            {
                using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName))
                {
                    sw.Write(TBInterpret.Text);
                }
            }
            isSaved = true;
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

        // start dispatcher timer on shortcut Ctrl + Enter on window listener
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                actionsQueue = new List<string>(TBInterpret.Text.Split('\n').Select(x => x.Trim().ToLower()).Where(x => x != "").ToArray());
                playfieldArray = new int[10, 10];
                robotPosition = new IntPoint(9, 0);
                direction = DirectionsEnum.Right;
                Rerender();
                dispatcherTimer.Start();
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(SliderSpeed.Value);
        }

        private void TBInterpret_TextChanged(object sender, TextChangedEventArgs e)
        {
            isSaved = false;
        }
    }
}
