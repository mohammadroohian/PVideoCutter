using MediaToolkit;
using MediaToolkit.Util;
using MediaToolkit.Options;
using MediaToolkit.Model;
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
using System.Windows.Threading;


namespace VideoCutter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// All video formats tath program accept.
        /// </summary>
        public static string[] VIDEO_FORMATS = { "mp4", "mkv", "avi", "mov", "wmv", "flv", "amv" };
        public static string[] SUBTITLE_FORMATS = { "srt" }; // { "srt", "ass" };
        public static string TEXTBOX_VIDEO_DEFULT_VALUE = "video file address";
        public static string TEXTBOX_SRT_DEFULT_VALUE = "srt file address";
        /// <summary>
        /// Get start time from program user interface.
        /// </summary>
        public TimeSpan StartTime
        {
            get
            {
                return new TimeSpan(0, int.Parse(textBox_startTimeM.Text), int.Parse(textBox_startTimeS.Text));
            }
        }
        /// <summary>
        /// Get end time from program user interface.
        /// </summary>
        public TimeSpan EndTime
        {
            get
            {
                return new TimeSpan(0, int.Parse(textBox_endTimeM.Text), int.Parse(textBox_endTimeS.Text));
            }
        }
        public string OutputDirectory
        {
            get
            {
                string inputPath = textBox_videoAddress.Text;
                int tmpStart = inputPath.LastIndexOf('\\');
                int tmpEnd = inputPath.LastIndexOf('.');
                return inputPath.Substring(tmpStart, tmpEnd - tmpStart);
            }
        }
        public MainWindow()
        {
            InitializeComponent();

            textBox_videoAddress.Text = TEXTBOX_VIDEO_DEFULT_VALUE;
            textBox_subtitleAddress.Text = TEXTBOX_SRT_DEFULT_VALUE;
        }

        // VLC.________________________________________________

        System.Diagnostics.Process vlcProcess;
        DispatcherTimer vlcPreviewTimer = new DispatcherTimer();
        private void btn_check_environment_variable_Click(object sender, RoutedEventArgs e)
        {
            CheckEnvironmentVariable();
        }
        private void KillVlc(object sender, EventArgs eventArgs)
        {
            vlcPreviewTimer.Stop();
            if (vlcProcess.HasExited)
                return;
            //vlcProcess.Kill();
        }
        private void CheckEnvironmentVariable()
        {
            const string name = "PATH";
            string pathvar = System.Environment.GetEnvironmentVariable(name);
            if (pathvar.ToLower().Contains("vlc"))
                return;
            var value = pathvar + ";" + textBox_address_vlc.Text;
            var target = EnvironmentVariableTarget.Machine;
            System.Environment.SetEnvironmentVariable(name, value, target);
        }

        // Browse files._______________________________________

        private void btn_browse_vlcAddress_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box 
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Videos"; // Default file name 

            // Show open file dialog box 
            Nullable<bool> result = dialog.ShowDialog();

            // Process open file dialog box results  
            if (result == true)
            {
                // Open document  
                textBox_address_vlc.Text = dialog.FileName;
            }
        }
        private void btn_browseSrt_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box 
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Videos"; // Default file name 
            dialog.Filter = "Files|";

            foreach (string item in SUBTITLE_FORMATS)
            {
                if (item != SUBTITLE_FORMATS[0])
                    dialog.Filter += ";";
                dialog.Filter += "*." + item;
            }

            // Show open file dialog box 
            Nullable<bool> result = dialog.ShowDialog();

            // Process open file dialog box results  
            if (result == true)
            {
                // Open document  
                textBox_subtitleAddress.Text = dialog.FileName;
            }

            SetBrowsLableColor_Srt();
        }
        private void btn_browseVideo_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box 
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Videos"; // Default file name 
            //More video formats at: https://en.wikipedia.org/wiki/Video_file_format
            dialog.Filter = "Files|";


            foreach (string item in VIDEO_FORMATS)
            {
                if (item != VIDEO_FORMATS[0])
                    dialog.Filter += ";";
                dialog.Filter += "*." + item;
            }

            // Show open file dialog box 
            Nullable<bool> result = dialog.ShowDialog();

            // Process open file dialog box results  
            if (result == true)
            {
                // Open document  
                textBox_videoAddress.Text = dialog.FileName;
            }

            SetBrowsLableColor_Video();
        }
        private void BrowseSrt_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Get droped file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                // Check file format.
                if (!CheckIsSrtFile(files[0]))
                {
                    MessageBox.Show("This file is not subtitle!, just .str format will be accepted.", "Format Error!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    SetBrowsLableColor_Srt();
                    return;
                }
                textBox_subtitleAddress.Text = files[0];
                SetBrowsLableColor_Srt();
            }
        }
        private void BrowseVideo_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Get droped file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                // Check file format.
                if (!CheckIsVideoFile(files[0]))
                {
                    MessageBox.Show("This file format is not suported!.", "Format Error.", MessageBoxButton.OK, MessageBoxImage.Warning);
                    SetBrowsLableColor_Video();
                    return;
                }
                textBox_videoAddress.Text = files[0];
                SetBrowsLableColor_Video();
            }
        }
        private void label_enterVideo_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                // Get droped file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                // Check file type and set element color.
                if (CheckIsVideoFile(files[0]))
                    label_enterVideo.Background = Brushes.DarkGreen;
                else
                    label_enterVideo.Background = Brushes.DarkRed;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void label_enterSrt_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                // Get droped file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                // Check file type and set element color.
                if (CheckIsSrtFile(files[0]))
                    label_enterSrt.Background = Brushes.DarkGreen;
                else
                    label_enterSrt.Background = Brushes.DarkRed;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void label_enterVideo_DragLeave(object sender, DragEventArgs e)
        {
            SetBrowsLableColor_Video();
        }
        private void label_enterSrt_DragLeave(object sender, DragEventArgs e)
        {
            SetBrowsLableColor_Srt();
        }
        private void btn_browse_video_delete_Click(object sender, RoutedEventArgs e)
        {
            textBox_videoAddress.Text = TEXTBOX_VIDEO_DEFULT_VALUE;
            SetBrowsLableColor_Video();
        }
        private void btn_browse_srt_delete_Click(object sender, RoutedEventArgs e)
        {
            textBox_subtitleAddress.Text = TEXTBOX_SRT_DEFULT_VALUE;
            SetBrowsLableColor_Srt();
        }

        /// <summary>
        /// Set color of drag and drop lable.
        /// If video address be exist then color will be dark blue, otherwise will be defult color.
        /// </summary>
        private void SetBrowsLableColor_Video()
        {
            // Check video file exist.
            if (System.IO.File.Exists(textBox_videoAddress.Text))
                label_enterVideo.Background = Brushes.DarkCyan;
            else
                label_enterVideo.Background = textBox_videoAddress.Background;
        }
        /// <summary>
        /// Set color of drag and drop lable.
        /// If srt address be exist then color will be dark blue, otherwise will be defult color.
        /// </summary>
        private void SetBrowsLableColor_Srt()
        {
            // Check srt file exist.
            if (System.IO.File.Exists(textBox_subtitleAddress.Text))
                label_enterSrt.Background = Brushes.DarkCyan;
            else
                label_enterSrt.Background = textBox_subtitleAddress.Background;
        }

        // Preview.____________________________________________

        private void textBox_startTimeM_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!CheckTimeIsAccepted(textBox_startTimeM.Text))
                textBox_startTimeM.Text = "0";
        }
        private void textBox_startTimeS_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!CheckTimeIsAccepted(textBox_startTimeS.Text))
                textBox_startTimeS.Text = "0";
        }
        private void textBox_endTimeM_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!CheckTimeIsAccepted(textBox_endTimeM.Text))
                textBox_endTimeM.Text = "0";
        }
        private void textBox_endTimeS_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!CheckTimeIsAccepted(textBox_endTimeS.Text))
                textBox_endTimeS.Text = "0";
        }
        private bool CheckTimeIsAccepted(string text)
        {
            // Check number.
            if (int.TryParse(text, out int n))
                return true;
            else
                return false;
        }
        private void btn_videoPreview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check video file exist.
                if (!System.IO.File.Exists(textBox_videoAddress.Text))
                {
                    MessageBox.Show("The video path is not currect!", "File Not Founded.", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Show preview.
                string strCmdText;
                strCmdText = textBox_videoAddress.Text + " --start-time=" + (int)StartTime.TotalSeconds + " --stop-time=" + (int)EndTime.TotalSeconds + " --fullscreen";

                // Open VLC.
                vlcProcess = System.Diagnostics.Process.Start("vlc.exe", strCmdText);

                //Start timer for close VLC.
                vlcPreviewTimer.Stop();
                vlcPreviewTimer.Tick += KillVlc;
                vlcPreviewTimer.Interval = new TimeSpan(0, 0, 0, (int)EndTime.TotalSeconds - (int)StartTime.TotalSeconds + 2, 0); //50 ms is fast enough
                vlcPreviewTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void btn_subtitlePreview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check srt file exist.
                if (!System.IO.File.Exists(textBox_subtitleAddress.Text))
                {
                    MessageBox.Show("The srt path is not currect!", "File Not Founded.", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // show cutted subtitle.
                List<string> lines = CutSubtitle(this.StartTime, this.EndTime);
                int limitLen = 30;
                string[] limittedLines = new string[limitLen + 3];

                if (lines.Count > limitLen)
                {
                    Array.Copy(lines.ToArray(), 0, limittedLines, 0, limitLen / 2);
                    limittedLines[limitLen / 2] = "...";
                    limittedLines[limitLen / 2 + 1] = "...";
                    limittedLines[limitLen / 2 + 2] = "...";
                    Array.Copy(lines.ToArray(), lines.Count - limitLen / 2, limittedLines, limitLen / 2 + 3, limitLen / 2);
                    MessageBox.Show(string.Join("\r\n", limittedLines), "Cutted subtitles.", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(string.Join("\r\n", lines), "Cutted subtitles.", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void btn_hashWordsPreview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check subtitle file exist.
                if (!System.IO.File.Exists(textBox_subtitleAddress.Text))
                {
                    MessageBox.Show("The srt path is not currect!", "File Not Founded.", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // show hash words.
                List<string> words = FindingHashWords();
                MessageBox.Show(string.Join("\t", words.ToArray()), "cutted Hash Words.", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // Create. Buttons_____________________________________

        private void btn_createVideo_Click(object sender, RoutedEventArgs e)
        {
            CreateVideoFile();
        }
        private void btn_createSubtitle_Click(object sender, RoutedEventArgs e)
        {
            CreateSubtitleFiles();
        }
        private void btn_createHashWords_Click(object sender, RoutedEventArgs e)
        {
            CreateHashWordsFile();
        }
        private void Btn_mergeVideoAndSubtitle_Click(object sender, RoutedEventArgs e)
        {
            CreateHardsubVideoAndSubtitle();
        }
        private void Btn_createAll_Click(object sender, RoutedEventArgs e)
        {
            if (!CreateVideoFile()) MessageBox.Show("Video file can not be created!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            if (!CreateSubtitleFiles()) MessageBox.Show("Subtitle files can not be created!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            if (!CreateHashWordsFile()) MessageBox.Show("Hashtags file can not be created!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            if (!CreateHardsubVideoAndSubtitle()) MessageBox.Show("Hardsub merged video file can not be created!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // Advanced options. Buttons_____________________________________

        private void Btn_AdvanceOutputOptions_SetToDefault_Click(object sender, RoutedEventArgs e)
        {
            outputSubtitleFontSize.Text = "30";
            outputSubtitleFormat.Text = "srt";
            outputVideoFormat.Text = "mp4";
            tbOutputVideoHeight.Text = "480";
        }

        // main creation functions.____________________________

        /// <summary>
        /// Creating video from input video file.
        /// </summary>
        /// <returns>operation works properly.</returns>
        private bool CreateVideoFile()
        {
            try
            {
                // Check video file exist.
                if (!System.IO.File.Exists(textBox_videoAddress.Text))
                {
                    MessageBox.Show("The video path is not currect!", "File Not Founded.", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // prepare paths.
                string inputPath = textBox_videoAddress.Text;
                string outputPath = GetRootInputFilesDirectory();

                // check output directory exists.
                if (!CheckOutputDirectoryExists(outputPath)) return false;

                var inputFile = new MediaFile { Filename = inputPath };
                var outputFile = new MediaFile { Filename = outputPath + "\\" + OutputDirectory + "\\" + tbOutputName.Text + "." + outputVideoFormat.Text };

                // Cut video.
                using (var engine = new Engine())
                {
                    engine.GetMetadata(inputFile);

                    var options = new ConversionOptions();

                    // This example will create a 25 second video, starting from the 
                    // 30th second of the original video.
                    //// First parameter requests the starting frame to cut the media from.
                    //// Second parameter requests how long to cut the video.
                    options.CutMedia(StartTime, EndTime - StartTime);

                    engine.Convert(inputFile, outputFile, options);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
        /// <summary>
        /// creating subtitle files. srt and ass.
        /// </summary>
        /// <returns>operation works properly.</returns>
        private bool CreateSubtitleFiles()
        {
            if (CreateSrtFile())
                if (CreateAssFileFromSrt())
                    if (CreateSrtFileFromAss()) return true;
            return false;
        }
        /// <summary>
        /// create subtitle from cutting information and input files.
        /// </summary>
        /// <returns>operation works properly.</returns>
        private bool CreateSrtFile()
        {
            try
            {
                // check srt file exist.
                if (!System.IO.File.Exists(textBox_subtitleAddress.Text))
                {
                    MessageBox.Show("The srt path is not currect!", "File Not Founded.", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // prepare paths.
                string outputPath = GetRootInputFilesDirectory();

                // check output directory exists.
                if (!CheckOutputDirectoryExists(outputPath)) return false;

                // cut subtitle.
                List<string> lines = CutSubtitle(this.StartTime, this.EndTime);

                // save cutted subtitle to file.
                System.IO.File.WriteAllLines(outputPath + "//" + OutputDirectory + "//" + tbOutputName.Text + ".srt", lines);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
        /// <summary>
        /// create ass file from srt file in output directory.
        /// </summary>
        /// <returns>operation works properly.</returns>
        private bool CreateAssFileFromSrt()
        {
            // prepare command input values for ffmpeg.
            string strCmdText = "-y -f srt -i \"{0}.srt\" \"{0}.ass\"";
            strCmdText = string.Format(strCmdText, tbOutputName.Text);

            var processStartInfo = new System.Diagnostics.ProcessStartInfo();
            processStartInfo.WorkingDirectory = GetRootInputFilesDirectory() + "\\" + OutputDirectory;
            processStartInfo.FileName = "ffmpeg.exe";
            processStartInfo.Arguments = strCmdText;

            // convert srt to ass with ffmpeg.
            vlcProcess = System.Diagnostics.Process.Start(processStartInfo);
            return true;
        }
        /// <summary>
        /// create srt file from ass file in output directory.
        /// </summary>
        /// <returns>operation works properly.</returns>
        private bool CreateSrtFileFromAss()
        {
            // prepare command input values for ffmpeg.
            string strCmdText = "-y -i \"{0}.ass\" \"{0}.srt\"";
            strCmdText = string.Format(strCmdText, tbOutputName.Text);

            var processStartInfo = new System.Diagnostics.ProcessStartInfo();
            processStartInfo.WorkingDirectory = GetRootInputFilesDirectory() + "\\" + OutputDirectory;
            processStartInfo.FileName = "ffmpeg.exe";
            processStartInfo.Arguments = strCmdText;

            // convert srt to ass with ffmpeg.
            vlcProcess = System.Diagnostics.Process.Start(processStartInfo);
            return true;
        }
        /// <summary>
        /// create merged hardsub video file from cutted video and subtitle.
        /// </summary>
        /// <returns></returns>
        private bool CreateHardsubVideoAndSubtitle()
        {
            // prepare command input values for ffmpeg.
            string strCmdText = "-y -i \"{0}.{1}\" -vf scale=-1:{2},\"subtitles={0}.srt:force_style=\'Fontsize={4}\'\" \"{3}.{1}\"";
            strCmdText = string.Format(strCmdText,
                tbOutputName.Text,
                outputVideoFormat.Text,
                tbOutputVideoHeight.Text,
                tbOutputName.Text + "_final",
                outputSubtitleFontSize.Text);

            var processStartInfo = new System.Diagnostics.ProcessStartInfo();
            processStartInfo.WorkingDirectory = GetRootInputFilesDirectory() + "\\" + OutputDirectory;
            processStartInfo.FileName = "ffmpeg.exe";
            processStartInfo.Arguments = strCmdText;

            // convert srt to ass with ffmpeg.
            vlcProcess = System.Diagnostics.Process.Start(processStartInfo);
            return true;
        }
        /// <summary>
        /// Create hash words from cutting information input files.
        /// </summary>
        /// <returns>operation works properly.</returns>
        private bool CreateHashWordsFile()
        {
            try
            {
                // Check srt file exist.
                if (!System.IO.File.Exists(textBox_subtitleAddress.Text))
                {
                    MessageBox.Show("The srt path is not currect!", "File Not Founded.", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // prepare paths.
                string outputPath = GetRootInputFilesDirectory();

                // check output directory exists.
                if (!CheckOutputDirectoryExists(outputPath)) return false;

                // Save word hashtags file.
                System.IO.File.WriteAllText(outputPath + "//" + OutputDirectory + "//" + tbOutputName.Text + ".txt", string.Join(" ", FindingHashWords()));

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        // related creation functions._________________________
        private string GetRootInputFilesDirectory()
        {
            // prepare paths.
            string inputPath = textBox_videoAddress.Text;
            return inputPath.Substring(0, inputPath.LastIndexOf('\\'));
        }
        /// <summary>
        /// check output directory exists, if not then create directory.
        /// </summary>
        /// <param name="path">path of files.</param>
        /// <returns>operation works properly.</returns>
        private bool CheckOutputDirectoryExists(string path)
        {
            try
            {
                if (System.IO.Directory.Exists(path + "//" + OutputDirectory)) return true;
                else
                {
                    System.IO.Directory.CreateDirectory(path + "//" + OutputDirectory);
                    return true;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Directory can not be created!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        /// <summary>
        /// Finding hastags in cutted subtitle.
        /// </summary>
        /// <returns>lsit of specific words.</returns>
        private List<string> FindingHashWords()
        {
            char[] delimiterChars = { ' ', ',', '.', ':', '\t' };
            List<string> lines = CutSubtitle(this.StartTime, this.EndTime);
            List<string> words = new List<string>();
            // Create words collection.
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains("-->"))
                    continue;

                // Add words. 
                foreach (string word in lines[i].ToLower().Split(delimiterChars))
                {
                    string cleanedWord = CleanWord(word);
                    if (cleanedWord.Trim() == "") continue;

                    string tmpCleanedWord = WordToHashtag(cleanedWord);
                    if (!words.Contains(tmpCleanedWord) && tmpCleanedWord != "")
                        words.Add(tmpCleanedWord);
                }
            }
            return words;
        }
        /// <summary>
        /// Clean word to standard and accepted styles.
        /// </summary>
        /// <param name="word">Spicific word.</param>
        /// <returns>Cleaned word.</returns>
        private string CleanWord(string word)
        {
            string[] replaceStringToEmpty = { "?", ".", "!", "\"" };
            string[] deniedStrings = { "<", ">", "|", "//" };

            // ToLower.
            word = word.ToLower();

            // Replaces.
            foreach (string item in replaceStringToEmpty)
            {
                word = word.Replace(item, "");
            }

            // Chck for denied.
            foreach (string item in deniedStrings)
            {
                if (word.Contains(item))
                    return "";
            }

            // Check for '-' character and replace with '_' character.
            word = word.Replace('-', '_');

            // Chcek for one char words!
            if (word == "_")
                return "";

            // Trim.
            word = word.Trim();

            // Check is number.
            if (int.TryParse(word, out int n))
                return "";

            return word;
        }
        /// <summary>
        /// Creating hastag word from cleand word by adding # symbol and other things.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private string WordToHashtag(string word)
        {
            return "#" + char.ToUpper(word[0]) + word.Substring(1);
        }
        /// <summary>
        /// Getting time span from srt times.
        /// </summary>
        /// <param name="timeStr">Srt time string.</param>
        /// <param name="timeSP">converted srt time to TimeSpan.</param>
        /// <returns>Is possible to convert or not.</returns>
        private bool ConvertSubtitleTimeToSeconds(string timeStr, out TimeSpan timeSP)
        {
            // Trim.
            timeStr = timeStr.Trim();
            // Change millisecond seperator.
            timeStr = timeStr.Replace(',', '.');
            // Parse to timeSpan.
            if (TimeSpan.TryParse(timeStr, out timeSP))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Cut subtitle trow start and end time.
        /// The srt file is getted from texbox in user interface.
        /// </summary>
        /// <param name="startTime">Start time to cut.</param>
        /// <param name="endTime">End time ro cut</param>
        /// <returns>Cutted subtitle in lines.</returns>
        private List<string> CutSubtitle(TimeSpan startTime, TimeSpan endTime)
        {
            List<string> lines = new List<string>(System.IO.File.ReadAllLines(textBox_subtitleAddress.Text));
            List<string> subLines = new List<string>();

            TimeSpan tmpStartTime = new TimeSpan(), tmpEndTime = new TimeSpan();
            for (int i = 0; i < lines.Count; i++)
            {
                string newLine = lines[i];
                if (lines[i].Contains("-->"))
                {
                    // Get start time.
                    ConvertSubtitleTimeToSeconds(lines[i].Split(new string[] { "-->" }, StringSplitOptions.RemoveEmptyEntries)[0], out tmpStartTime);
                    // Get end time.
                    ConvertSubtitleTimeToSeconds(lines[i].Split(new string[] { "-->" }, StringSplitOptions.RemoveEmptyEntries)[1], out tmpEndTime);

                    TimeSpan tmpSTR = tmpStartTime - startTime;
                    TimeSpan tmpEND = tmpEndTime - startTime;

                    //Add line to subline.
                    string newDialogTimeStart = "{0}:{1}:{2},{3}".FormatInvariant(tmpSTR.Hours,
                                                                            tmpSTR.Minutes,
                                                                            tmpSTR.Seconds,
                                                                            tmpSTR.Milliseconds);
                    string newDialogTimeEnd = "{0}:{1}:{2},{3}".FormatInvariant(tmpEND.Hours,
                                                                            tmpEND.Minutes,
                                                                            tmpEND.Seconds,
                                                                            tmpEND.Milliseconds);
                    newLine = newDialogTimeStart + " --> " + newDialogTimeEnd;
                }
                // Check start.
                if (tmpStartTime < startTime)
                {
                    continue;
                }

                // Check end.
                if (tmpEndTime > endTime)
                {
                    int tmpEndindex = subLines.Count - 1;
                    if (int.TryParse(subLines[tmpEndindex], out int n))
                        subLines.RemoveAt(tmpEndindex);
                    break;
                }

                subLines.Add(newLine);
            }
            return subLines;
        }
        /// <summary>
        /// Check the video file is exits.
        /// </summary>
        /// <param name="path">Path of video file.</param>
        /// <returns>The file is exist or not.</returns>
        private bool CheckIsVideoFile(string path)
        {
            try
            {
                // Check file format.
                bool tmpCheckFormatIsCurrect = false;
                foreach (string item in VIDEO_FORMATS)
                {
                    if (path.Contains("." + item))
                    {
                        tmpCheckFormatIsCurrect = true;
                        break;
                    }
                }
                return tmpCheckFormatIsCurrect;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
        /// <summary>
        /// Check the srt file is exits.
        /// </summary>
        /// <param name="path">Path of srt file.</param>
        /// <returns>The file is exist or not.</returns>
        private bool CheckIsSrtFile(string path)
        {
            try
            {
                // Check file format.
                bool tmpCheckFormatIsCurrect = false;
                foreach (string item in SUBTITLE_FORMATS)
                {
                    if (path.Contains("." + item))
                    {
                        tmpCheckFormatIsCurrect = true;
                        break;
                    }
                }
                return tmpCheckFormatIsCurrect;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }


    }
}