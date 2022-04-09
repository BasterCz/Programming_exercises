
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
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
using Rectangle = System.Windows.Shapes.Rectangle;

namespace MazeAPI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
        RestClient client = new RestClient("https://maturita.delta-www.cz/prakticka/2020-maze/maze-api");
        string[,] maze = new string[25, 25];
        IntPoint position = new IntPoint(12, 12);
        bool haveKey = false;
        bool isDisabled = false;
        bool SouthEnable = false;
        bool NorthEnable = false;
        bool WestEnable = false;
        bool EastEnable = false;
        public MainWindow()
        {
            InitializeComponent();
            for (int i = 0; i < 25; i++)
            {
                ColumnDefinition col = new ColumnDefinition();
                col.Width = new GridLength(1, GridUnitType.Star);
                GridMap.ColumnDefinitions.Add(col);
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(1, GridUnitType.Star);
                GridMap.RowDefinitions.Add(row);
            }
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    Rectangle rect = new Rectangle();
                    if (i == 12 && j == 12)
                    {
                        rect.Fill = HexToBrush("#ff68ff");
                    }
                    else
                    {
                        rect.Fill = Brushes.White;
                    }
                    rect.Tag = new IntPoint(i, j);
                    GridMap.Children.Add(rect);
                    Grid.SetColumn(rect, i);
                    Grid.SetRow(rect, j);

                }
            }
            for (int i = 0; i < maze.GetLength(0); i++)
            {
                for (int j = 0; j < maze.GetLength(1); j++)
                {
                    maze[i, j] = "";
                }
            }
            BtnReset_Click(null, null);
        }

        private async void BtnNorth_Click(object sender, RoutedEventArgs e)
        {
            DisableActionButtons();
            await GoNorth(false, true);
            await LookAround();
            EnableActionButtons();
            Rerender();
        }

        private async void BtnSouth_Click(object sender, RoutedEventArgs e)
        {
            DisableActionButtons();
            await GoSouth(false, true);
            await LookAround();
            EnableActionButtons();
            Rerender();
        }

        private async void BtnWest_Click(object sender, RoutedEventArgs e)
        {
            DisableActionButtons();
            await GoWest(false, true);
            await LookAround();
            EnableActionButtons();
            Rerender();
        }

        private async void BtnEast_Click(object sender, RoutedEventArgs e)
        {
            DisableActionButtons();
            await GoEast(false, true);
            await LookAround();
            EnableActionButtons();
            Rerender();
        }

        private async void BtnKey_Click(object sender, RoutedEventArgs e)
        {
            DisableActionButtons();
            if (await DoGrabKey())
            {
                haveKey = true;
                BorderKey.Background = Brushes.Yellow;
                await LookAround();
                Rerender();
                EnableActionButtons();
            }
            else
            {
                await LookAround();
                Rerender();
                EnableActionButtons();
            }
        }



        private async void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            DisableActionButtons();
            if (await DoExit())
            {
                Rerender();
            }
        }

        private async void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            DisableActionButtons();
            if (await DoReset())
            {
                await LookAround();
                Rerender();
                EnableActionButtons();
            }
        }
        private void DisableActionButtons()
        {
            isDisabled = true;
            SouthEnable = false;
            NorthEnable = false;
            WestEnable = false;
            EastEnable = false;
            BtnSouth.IsEnabled = false;
            BtnExit.IsEnabled = false;
            BtnNorth.IsEnabled = false;
            BtnEast.IsEnabled = false;
            BtnWest.IsEnabled = false;
            BtnKey.IsEnabled = false;
        }
        private async void EnableActionButtons()
        {
            await Task.Delay(500);
            isDisabled = false;

            BtnSouth.IsEnabled = SouthEnable;
            BtnExit.IsEnabled = true;
            BtnNorth.IsEnabled = NorthEnable;
            BtnEast.IsEnabled = EastEnable;
            BtnWest.IsEnabled = WestEnable;
            BtnKey.IsEnabled = true;
        }
        private SolidColorBrush HexToBrush(string hex)
        {
            return new SolidColorBrush(System.Windows.Media.Color.FromArgb(ColorTranslator.FromHtml(hex).A, ColorTranslator.FromHtml(hex).R, ColorTranslator.FromHtml(hex).G, ColorTranslator.FromHtml(hex).B));
        }

        private void Rerender()
        {
            foreach (Rectangle rect in GridMap.Children)
            {
                IntPoint point = (IntPoint)rect.Tag;
                string content = maze[point.x, point.y];
                if (content.StartsWith("#"))
                {
                    rect.Fill = HexToBrush(content);
                }
                else if (content == "wall")
                {
                    rect.Fill = Brushes.Black;
                }
                else
                {
                    rect.Fill = Brushes.White;
                }
                if (point.x == position.x && point.y == position.y)
                {
                    rect.Stroke = Brushes.Red;
                }
                else
                {
                    rect.Stroke = Brushes.Transparent;
                }
            }
        }
        private void Reset()
        {
            for (int i = 0; i < maze.GetLength(0); i++)
            {
                for (int j = 0; j < maze.GetLength(1); j++)
                {
                    maze[i, j] = "";
                }
            }
            position = new IntPoint(12, 12);
            haveKey = false;
            BorderColor.Background = Brushes.White;
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (isDisabled) return;
            if ((e.Key == Key.W || e.Key == Key.Up) && NorthEnable)
            {
                BtnNorth_Click(null, null);
            }
            else if ((e.Key == Key.S || e.Key == Key.Down) && SouthEnable)
            {
                BtnSouth_Click(null, null);
            }
            else if ((e.Key == Key.A || e.Key == Key.Left) && WestEnable)
            {
                BtnWest_Click(null, null);
            }
            else if ((e.Key == Key.D || e.Key == Key.Right) && EastEnable)
            {
                BtnEast_Click(null, null);
            }
            else if (e.Key == Key.E)
            {
                BtnKey_Click(null, null);
            }
            else if (e.Key == Key.Q)
            {
                BtnExit_Click(null, null);
            }
            else if (e.Key == Key.R)
            {
                BtnReset_Click(null, null);
            }
            else if (e.Key == Key.Escape)
            {
                Close();
            }
        }
        private async Task<bool> LookAround()
        {
            bool success = false;
            success = await GoNorth(true);
            if (success)
            {
                await GoSouth(true, true);
            }
            success = await GoWest(true);
            if (success)
            {
                await GoEast(true, true);
            }
            success = await GoSouth(true);
            if (success)
            {
                await GoNorth(true, true);
            }
            success = await GoEast(true);
            if (success)
            {
                await GoWest(true, true);
            }
            return true;

        }

        private async Task<bool> GoNorth(bool isLookAround, bool skipCheck = false)
        {
            if (maze[position.x, position.y - 1] != "" && !skipCheck)
            {
                if (maze[position.x, position.y - 1] == "wall") NorthEnable = false;
                else NorthEnable = true;
                return false;
            }
            RestRequest req = new RestRequest("", Method.POST);
            req.AlwaysMultipartFormData = true;
            req.AddParameter("token", "849bd685");
            req.AddParameter("command", "north");
            IRestResponse res = await client.ExecuteAsync(req);
            string lightColor = JsonConvert.DeserializeObject<dynamic>(res.Content).lightColor;
            string youSee = JsonConvert.DeserializeObject<dynamic>(res.Content).youSee;
            if (JsonConvert.DeserializeObject<dynamic>(res.Content).success == false)
            {
                maze[position.x, position.y - 1] = "wall";
                if (!skipCheck) NorthEnable = false;
                if (!isLookAround)
                {
                    RTBResponse.Document.Blocks.Add(new Paragraph(this.Dispatcher.Invoke(() =>
                    {
                        Run run = new Run("You can't go north. You see " + youSee + ".");
                        run.Background = Brushes.Orange;
                        return run;
                    })));
                }
                return false;
            }
            else
            {
                position = new IntPoint(position.x, position.y - 1);
                if (youSee == "key")
                {
                    maze[position.x, position.y] = "#ffff00";
                    if (!isLookAround)
                    {
                        RTBResponse.Document.Blocks.Add(new Paragraph(this.Dispatcher.Invoke(() =>
                        {
                            Run run = new Run("You see a key.");
                            run.Background = Brushes.Yellow;
                            return run;
                        })));
                    }
                }
                else if (youSee == "exit")
                {
                    maze[position.x, position.y] = "#ff0000";
                    if (!isLookAround)
                    {
                        RTBResponse.Document.Blocks.Add(new Paragraph(this.Dispatcher.Invoke(() =>
                        {
                            Run run = new Run("You see an exit.");
                            run.Background = Brushes.LightBlue;
                            return run;
                        })));
                    }
                }
                else
                {
                    maze[position.x, position.y] = lightColor;
                    if (!isLookAround)
                    {
                        RTBResponse.Document.Blocks.Add(new Paragraph(this.Dispatcher.Invoke(() =>
                        {
                            Run run = new Run("You moved north.");
                            run.Background = Brushes.LightGray;
                            return run;
                        })));
                    }
                }
                if (!skipCheck) NorthEnable = true;

            }
            BorderColor.Background = HexToBrush(lightColor);
            return true;
        }
        private async Task<bool> GoSouth(bool isLookAround, bool skipCheck = false)
        {
            if (maze[position.x, position.y + 1] != "" && !skipCheck)
            {
                if (maze[position.x, position.y + 1] == "wall") SouthEnable = false;
                else SouthEnable = true;
                return false;
            }
            RestRequest req = new RestRequest("", Method.POST);
            req.AlwaysMultipartFormData = true;
            req.AddParameter("token", "849bd685");
            req.AddParameter("command", "south");
            IRestResponse res = await client.ExecuteAsync(req);
            string lightColor = JsonConvert.DeserializeObject<dynamic>(res.Content).lightColor;
            string youSee = JsonConvert.DeserializeObject<dynamic>(res.Content).youSee;
            if (JsonConvert.DeserializeObject<dynamic>(res.Content).success == false)
            {
                maze[position.x, position.y + 1] = "wall";
                if (!skipCheck) SouthEnable = false;
                if (!isLookAround)
                {
                    RTBResponse.Document.Blocks.Add(new Paragraph(this.Dispatcher.Invoke(() =>
                    {
                        Run run = new Run("You can't go south. You see " + youSee + ".");
                        run.Background = Brushes.Orange;
                        return run;
                    })));
                }
                return false;
            }
            else
            {
                position = new IntPoint(position.x, position.y + 1);
                if (youSee == "key")
                {
                    maze[position.x, position.y] = "#ffff00";
                    if (!isLookAround)
                    {
                        RTBResponse.Document.Blocks.Add(new Paragraph(this.Dispatcher.Invoke(() =>
                        {
                            Run run = new Run("On the floor is lying a key.");
                            run.Background = Brushes.Yellow;
                            return run;
                        })));
                    }
                }
                else if (youSee == "exit")
                {
                    maze[position.x, position.y] = "#ff0000";
                    if (!isLookAround)
                    {

                        RTBResponse.Document.Blocks.Add(new Paragraph(this.Dispatcher.Invoke(() =>
                        {
                            Run run = new Run("You see a door.");
                            run.Background = Brushes.LightBlue;
                            return run;
                        })));

                    }
                }
                else
                {
                    maze[position.x, position.y] = lightColor;
                    if (!isLookAround)
                    {
                        RTBResponse.Document.Blocks.Add(new Paragraph(this.Dispatcher.Invoke(() =>
                        {
                            Run run = new Run("You moved south.");
                            run.Background = Brushes.LightGray;
                            return run;
                        })));
                    }
                }
                if (!skipCheck) SouthEnable = true;

            }
            BorderColor.Background = HexToBrush(lightColor);
            return true;
        }
        private async Task<bool> GoEast(bool isLookAround, bool skipCheck = false)
        {
            if (maze[position.x + 1, position.y] != "" && !skipCheck)
            {
                if (maze[position.x + 1, position.y] == "wall") EastEnable = false;
                else EastEnable = true;
                return false;
            }
            RestRequest req = new RestRequest("", Method.POST);
            req.AlwaysMultipartFormData = true;
            req.AddParameter("token", "849bd685");
            req.AddParameter("command", "east");
            IRestResponse res = await client.ExecuteAsync(req);
            string lightColor = JsonConvert.DeserializeObject<dynamic>(res.Content).lightColor;
            string youSee = JsonConvert.DeserializeObject<dynamic>(res.Content).youSee;
            if (JsonConvert.DeserializeObject<dynamic>(res.Content).success == false)
            {
                maze[position.x + 1, position.y] = "wall";
                if (!skipCheck) EastEnable = false;
                if (!isLookAround)
                {
                    RTBResponse.Document.Blocks.Add(new Paragraph(this.Dispatcher.Invoke(() =>
                    {
                        Run run = new Run("You can't go east. You see " + youSee + ".");
                        run.Background = Brushes.Orange;
                        return run;
                    })));
                }
                return false;
            }
            else
            {
                position = new IntPoint(position.x + 1, position.y);
                if (youSee == "key")
                {
                    maze[position.x, position.y] = "#ffff00";
                    if (!isLookAround)
                    {
                        RTBResponse.Document.Blocks.Add(new Paragraph(this.Dispatcher.Invoke(() =>
                        {
                            Run run = new Run("On the floor is lying a key.");
                            run.Background = Brushes.Yellow;
                            return run;
                        })));
                    }
                }
                else if (youSee == "exit")
                {
                    maze[position.x, position.y] = "#ff0000";
                    if (!isLookAround)
                    {

                        RTBResponse.Document.Blocks.Add(new Paragraph(this.Dispatcher.Invoke(() =>
                        {
                            Run run = new Run("You see a door.");
                            run.Background = Brushes.LightBlue;
                            return run;
                        })));

                    }
                }
                else
                {
                    maze[position.x, position.y] = lightColor;
                    if (!isLookAround)
                    {
                        RTBResponse.Document.Blocks.Add(new Paragraph(this.Dispatcher.Invoke(() =>
                        {
                            Run run = new Run("You moved east.");
                            run.Background = Brushes.LightGray;
                            return run;
                        })));
                    }
                }

                if (!skipCheck) EastEnable = true;

            }
            BorderColor.Background = HexToBrush(lightColor);
            return true;
        }
        private async Task<bool> GoWest(bool isLookAround, bool skipCheck = false)
        {
            if (maze[position.x - 1, position.y] != "" && !skipCheck)
            {
                if (maze[position.x - 1, position.y] == "wall") WestEnable = false;
                else WestEnable = true;
                return false;
            }
            RestRequest req = new RestRequest("", Method.POST);
            req.AlwaysMultipartFormData = true;
            req.AddParameter("token", "849bd685");
            req.AddParameter("command", "west");
            IRestResponse res = await client.ExecuteAsync(req);
            string lightColor = JsonConvert.DeserializeObject<dynamic>(res.Content).lightColor;
            string youSee = JsonConvert.DeserializeObject<dynamic>(res.Content).youSee;
            if (JsonConvert.DeserializeObject<dynamic>(res.Content).success == false)
            {
                maze[position.x - 1, position.y] = "wall";
                if (!skipCheck) WestEnable = false;
                if (!isLookAround)
                {
                    RTBResponse.Document.Blocks.Add(new Paragraph(this.Dispatcher.Invoke(() =>
                    {
                        Run run = new Run("You can't go west. You see " + youSee + ".");
                        run.Background = Brushes.Orange;
                        return run;
                    })));
                }
                return false;
            }
            else
            {
                position = new IntPoint(position.x - 1, position.y);
                if (youSee == "key")
                {
                    maze[position.x, position.y] = "#ffff00";
                    if (!isLookAround)
                    {
                        RTBResponse.Document.Blocks.Add(new Paragraph(this.Dispatcher.Invoke(() =>
                        {
                            Run run = new Run("On the floor is lying a key.");
                            run.Background = Brushes.Yellow;
                            return run;
                        })));
                    }
                }
                else if (youSee == "exit")
                {
                    maze[position.x, position.y] = "#ff0000";
                    if (!isLookAround)
                    {

                        RTBResponse.Document.Blocks.Add(new Paragraph(this.Dispatcher.Invoke(() =>
                        {
                            Run run = new Run("You see a door.");
                            run.Background = Brushes.LightBlue;
                            return run;
                        })));

                    }
                }
                else
                {
                    maze[position.x, position.y] = lightColor;
                    if (!isLookAround)
                    {
                        RTBResponse.Document.Blocks.Add(new Paragraph(this.Dispatcher.Invoke(() =>
                        {
                            Run run = new Run("You moved west.");
                            run.Background = Brushes.LightGray;
                            return run;
                        })));
                    }
                }

                if (!skipCheck) WestEnable = true;

            }
            BorderColor.Background = HexToBrush(lightColor);
            return true;
        }

        private async Task<bool> DoReset()
        {
            RestRequest req = new RestRequest("", Method.POST);
            req.AlwaysMultipartFormData = true;
            req.AddParameter("token", "849bd685");
            req.AddParameter("command", "reset");
            IRestResponse res = await client.ExecuteAsync(req);
            string lightColor = JsonConvert.DeserializeObject<dynamic>(res.Content).lightColor;
            Reset();
            if (JsonConvert.DeserializeObject<dynamic>(res.Content).success == true)
            {
                maze[12, 12] = lightColor;
                return true;
            }
            else return false;
        }
        private async Task<bool> DoGrabKey()
        {
            RestRequest req = new RestRequest("", Method.POST);
            req.AlwaysMultipartFormData = true;
            req.AddParameter("token", "849bd685");
            req.AddParameter("command", "grab");
            IRestResponse res = await client.ExecuteAsync(req);
            string lightColor = JsonConvert.DeserializeObject<dynamic>(res.Content).lightColor;
            string youSee = JsonConvert.DeserializeObject<dynamic>(res.Content).youSee;
            if (JsonConvert.DeserializeObject<dynamic>(res.Content).success == true)
            {
                maze[position.x, position.y] = lightColor;

                RTBResponse.Document.Blocks.Add(new Paragraph(this.Dispatcher.Invoke(() =>
                {
                    Run run = new Run("You grabbed the key.");
                    run.Background = Brushes.Yellow;
                    return run;
                })));



                return true;
            }
            else return false;
        }
        private async Task<bool> DoExit()
        {
            RestRequest req = new RestRequest("", Method.POST);
            req.AlwaysMultipartFormData = true;
            req.AddParameter("token", "849bd685");
            req.AddParameter("command", "exit");
            IRestResponse res = await client.ExecuteAsync(req);
            string lightColor = JsonConvert.DeserializeObject<dynamic>(res.Content).lightColor;
            string youSee = JsonConvert.DeserializeObject<dynamic>(res.Content).youSee;
            if (JsonConvert.DeserializeObject<dynamic>(res.Content).success == true)
            {
                maze[position.x, position.y] = "#00ff00";
                RTBResponse.Document.Blocks.Add(new Paragraph(this.Dispatcher.Invoke(() =>
                {
                    Run run = new Run("You exit the dungeon/");
                    run.Background = Brushes.LightBlue;
                    return run;
                })));
                return true;
            }
            else
            {
                if (haveKey)
                {
                    RTBResponse.Document.Blocks.Add(new Paragraph(this.Dispatcher.Invoke(() =>
                    {
                        Run run = new Run("You can't exit the dungeon without the key.");
                        run.Background = Brushes.Orange;
                        return run;
                    })));
                }
                else
                {
                    RTBResponse.Document.Blocks.Add(new Paragraph(this.Dispatcher.Invoke(() =>
                    {
                        Run run = new Run("There is no exit here.");
                        run.Background = Brushes.Orange;
                        return run;
                    })));
                }
            
                return false;
            }
        }
        private void RTBResponse_TextChanged(object sender, EventArgs e)
        {
            RTBResponse.ScrollToEnd();
        }
    }
}
