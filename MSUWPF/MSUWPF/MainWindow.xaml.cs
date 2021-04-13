using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.ComponentModel;
using Newtonsoft.Json;
using System.Windows.Media.Imaging;

namespace MSUWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int curIndex = 0;
        HoraroJSONSchedule curSchedule;
        string infoFolderName = "MSU_CurrentRunInformation";

        private readonly BackgroundWorker worker = new BackgroundWorker();
        
        public MainWindow()
        {
            InitializeComponent();

            if (!Directory.Exists($"{Environment.CurrentDirectory}\\{infoFolderName}"))
            {
                Directory.CreateDirectory($"{Environment.CurrentDirectory}\\{infoFolderName}");
            }

            CurrentRunnerImage.MouseUp += CurrentRunnerImage_MouseUp;

            CurrentRunnerImage.Source = new BitmapImage(new Uri($"{Environment.CurrentDirectory}\\PFP Images\\default.png"));
            NextRunnerImage.Source = new BitmapImage(new Uri($"{Environment.CurrentDirectory}\\PFP Images\\default.png")); 

            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
        }

        private void CurrentRunnerImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //O
        }

        public int getNextRunIndex()
        {
            return curIndex == curSchedule.items.Length - 1 ? 0 : curIndex + 1;
        }

        public void downloadSchedule(object sender, RoutedEventArgs e)
        {
            DownloadLabel.Content = "Downloading...";
            worker.RunWorkerAsync();
        }

        public void moveSchedulePointer(object sender, RoutedEventArgs e)
        {
            if (curSchedule == null)
            {
                MessageBox.Show("ERROR: No schedule has been downloaded yet.");
                return;
            }

            if (((Button)sender).Tag.Equals("Next"))
            {
                curIndex++;
                if(curIndex >= curSchedule.items.Length)
                {
                    curIndex = 0;
                } 
            } else if (((Button)sender).Tag.Equals("Previous")) { 
                curIndex--;
                if (curIndex <= 0)
                {
                    curIndex = curSchedule.items.Length - 1;
                }
            } else {
                MessageBox.Show("Unknown Error has occured. See logs for further detail.");
            }

            CurrentRunInformationBox.Text = updateRunInformationTextBox(curIndex);
            NextRunInformationBox.Text = updateRunInformationTextBox(getNextRunIndex());

            createTextFiles();
            createNextRunnerTextFiles();
        }

        public void executeManualUpdate(object sender, RoutedEventArgs e)
        {
            createTextFiles();
            createNextRunnerTextFiles();
        }

        public void resetUpdates(object sender, RoutedEventArgs e)
        {
            CurrentRunInformationBox.Text = updateRunInformationTextBox(curIndex);
            NextRunInformationBox.Text = updateRunInformationTextBox(getNextRunIndex());

            createTextFiles();
            createNextRunnerTextFiles();
        }

        public string updateRunInformationTextBox(int index)
        {
            string runInformation = "";
            int runnerNameIndex = -1;
            for (int i = 0; i < curSchedule.columns.Length; i++)
            {
                if (curSchedule.columns[i].Equals("Runner"))
                {
                    runnerNameIndex = 3;
                }
                runInformation += $"{curSchedule.columns[i]} --> {curSchedule.items[index].data[i]}\r\n";
            }

            string numberFormat = "00";
            int estimate = curSchedule.items[index].length_t;
            int hours = estimate / 3600;
            int minutes = (estimate % 3600) / 60;
            int seconds = (estimate % 3600) % 60;

            runInformation += $"Estimate --> {hours.ToString(numberFormat)}:{minutes.ToString(numberFormat)}:{seconds.ToString(numberFormat)}\r\n";

            string runnerPFPFileName = $"{Environment.CurrentDirectory}\\PFP Images\\{curSchedule.items[index].data[runnerNameIndex]}.png";
            BitmapImage runnerImage;
            if (File.Exists(runnerPFPFileName))
            {
                runnerImage = new BitmapImage(new Uri(runnerPFPFileName));
            } else {
                runnerImage = new BitmapImage(new Uri($"{Environment.CurrentDirectory}\\PFP Images\\default.png"));
            }
            if (index == curIndex)
            {
                CurrentRunnerImage.Source = runnerImage;
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(runnerImage));

                using (var fileStream = new System.IO.FileStream(Environment.CurrentDirectory + $"\\{infoFolderName}\\runnerpfp.png", System.IO.FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            } else {
                NextRunnerImage.Source = runnerImage;
            }
            

            return runInformation;            
        }

        public void createTextFiles(string textToAppend = "")
        {
            if (!(bool)UpdateTextFilesCheckbox.IsChecked)
            {
                return;
            }

            if(!Directory.Exists(Environment.CurrentDirectory + $"\\{infoFolderName}"))
            {
                Directory.CreateDirectory(Environment.CurrentDirectory + $"/{infoFolderName}");
            }

            string folderPathStart = $"{Environment.CurrentDirectory}\\{infoFolderName}\\{textToAppend}";
            foreach (string infoSection in CurrentRunInformationBox.Text.Split("\r\n"))
            {
                if (!infoSection.Equals(""))
                {
                    string[] splitInfo = infoSection.Split("-->");

                    File.WriteAllText($"{folderPathStart}{ splitInfo[0].Substring(0, splitInfo[0].Length-1)}.txt", $"{splitInfo[1].Trim()}");
                }
            }

            string commHostText = "";
            if (!Comm1TextBox.Text.Equals("") || !Comm2TextBox.Text.Equals("") || !Comm3TextBox.Text.Equals("") || !Comm4TextBox.Text.Equals(""))
            {
                commHostText = "Commentators: \n";

                commHostText = getCommentatorText(Comm1TextBox, commHostText);
                commHostText = getCommentatorText(Comm2TextBox, commHostText);
                commHostText = getCommentatorText(Comm3TextBox, commHostText);
                commHostText = getCommentatorText(Comm4TextBox, commHostText);
            }

            int hostColumn = -1;
            int hostPronounsCol = -1;
            for (int i = 0; i < curSchedule.columns.Length; i++)
            {
                if (curSchedule.columns[i].Equals("Host"))
                {
                    hostColumn = i;
                    
                }
                if (curSchedule.columns[i].Equals("Host Pronouns"))
                {
                    hostPronounsCol = i;

                }
            }

            commHostText += $"Host:\n    {curSchedule.items[curIndex].data[hostColumn]} ({curSchedule.items[curIndex].data[hostPronounsCol]})";
            File.WriteAllText($"{folderPathStart}Commentators.txt", commHostText);

            File.WriteAllText($"{folderPathStart}Runner_and_Pronouns.txt",
                $"{File.ReadAllText($"{folderPathStart}Runner.txt")} ({File.ReadAllText($"{folderPathStart}Runner Pronouns.txt")})");
        }

        public string getCommentatorText(TextBox textbox, string commText)
        {
            string spacer = "    ";
            return textbox.Text.Length == 0 ? commText : $"{commText}{spacer}{textbox.Text}\n";
        }

        public void createNextRunnerTextFiles(string textToAppend = "")
        {
            var lines = NextRunInformationBox.Text.Split("\r\n");

            File.WriteAllText($"{Environment.CurrentDirectory}\\Coming_Up.txt", $"Coming Up Next: {lines[3].Split("-->")[1]} - {lines[0].Split("-->")[1]} ({lines[1].Split("-->")[1]})");
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // run all background tasks here
            using (var client = new HttpClient())
            {
                var response = client.GetAsync("https://horaro.org/ffff2020/schedule.json?named=true").Result.Content.ReadAsStringAsync().Result;
                var rawContent = JsonConvert.DeserializeObject<HoraroJSONResponse>(response);

                curSchedule = rawContent.schedule;
            }
        }

        private void worker_RunWorkerCompleted(object sender,RunWorkerCompletedEventArgs e)
        {
            //update ui once worker complete his work
            DownloadLabel.Content = "Download Complete!";
            CurrentRunInformationBox.Text = updateRunInformationTextBox(curIndex);
            NextRunInformationBox.Text = updateRunInformationTextBox(getNextRunIndex());
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
