using Microsoft.VisualBasic.Devices;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using static System.Reflection.Metadata.BlobBuilder;
using MessageBox = System.Windows.MessageBox;

namespace FindingWordInDirectory
{
    [AddINotifyPropertyChangedInterface]
    public class ViewModel
    {
        public ObservableCollection<FoundWord> FilesWithFoundWords { get; set; }
        CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        CancellationToken token;
        bool isDisposedToken = true;
        public string DirectoryName { get; set; }
        public string Keyword { get; set; }
        public int Progress { get; set; }

        public ViewModel()
        {
            FilesWithFoundWords = new ObservableCollection<FoundWord>();
            chooseDirectoryButton_Click = new((o) => ChooseDirectory());
            startSearchButton_Click = new((o) => StartSearchAsync(), (с) => string.IsNullOrEmpty(Keyword) == false && isDisposedToken == true);
            stopButton_Click = new((o) => Stop(), (с) => isDisposedToken == false);
            saveResultButton_Click = new((o) => SaveResult(), (с) => Progress == 100);
        }

        #region Command Choose directory
        private readonly RelayCommand chooseDirectoryButton_Click;
        public ICommand ChooseDirectoryButton_Click => chooseDirectoryButton_Click;
        public void ChooseDirectory()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    DirectoryName = dialog.SelectedPath;
            }
        }
        #endregion

        #region Command Start search
        private readonly RelayCommand startSearchButton_Click;
        public ICommand StartSearchButton_Click => startSearchButton_Click;
        private async Task StartSearchAsync()
        {
            FilesWithFoundWords.Clear();
            Progress = 0;

            cancelTokenSource = new CancellationTokenSource();
            token = cancelTokenSource.Token;
            isDisposedToken = false;
            // Отримуємо всі файли *.txt в директорії та в її піддиректоріях
            string[] txtFiles = Directory.GetFiles(DirectoryName, "*.txt", SearchOption.AllDirectories);

            int totalFiles = txtFiles.Length;
            int processedFiles = 0;

            try
            {
                //Дістаємо текст з файлів
                var TasksfilesWithText = txtFiles.AsParallel().WithCancellation(token).
                    //Select(async (fileName) => new FileNameText { FileName = fileName, Text = await File.ReadAllTextAsync(fileName, token)});
                    Select(async (fileName, index) =>
                    {
                        var text = await File.ReadAllTextAsync(fileName, token);
                        Progress = (int)((double)(index + 1) / txtFiles.Length * 50);
                        return new FileNameText { FileName = fileName, Text = text };
                    });
                var filesWithText = await Task.WhenAll(TasksfilesWithText);
                //Залишаємо лише ті, які містять ключове слово
                var foundWordExamples = filesWithText.AsParallel().WithCancellation(token).
                   Where(fileNameText => 1 < fileNameText.Text.Split(new string[] { Keyword }, StringSplitOptions.RemoveEmptyEntries).Length - 1).
                   Select(fileNameText => new FoundWord 
                   {
                           FileName = fileNameText.FileName,
                           PathFolder = Path.GetDirectoryName(fileNameText.FileName),
                           NumberOfOccurrences = fileNameText.Text.Split(new string[] { Keyword }, StringSplitOptions.RemoveEmptyEntries).Length - 1
                   }).ToList();

                //Закидаємо ті, які містять 1< входження
                foreach (var foundWord in foundWordExamples)
                {
                await Task.Delay(2000);
                    if (token.IsCancellationRequested)
                        token.ThrowIfCancellationRequested();
                    FilesWithFoundWords.Add(foundWord);
                    Progress = 50 + (int)(((double)FilesWithFoundWords.Count / foundWordExamples.Count) * 50); // Оновлюємо прогрес
                }
            }
            catch (OperationCanceledException ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                cancelTokenSource.Dispose();
                isDisposedToken = true;
            }
        }
        #endregion

        #region Command Stop
        private readonly RelayCommand stopButton_Click;
        public ICommand StopButton_Click => stopButton_Click;
        private void Stop()
        {
            cancelTokenSource.Cancel();
        }
        #endregion

        #region Command Save result
        private readonly RelayCommand saveResultButton_Click;
        public ICommand SaveResultButton_Click => saveResultButton_Click;
        private void SaveResult()
        {
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log.txt");

            using (StreamWriter writer = File.AppendText(logFilePath))
            {
                writer.WriteLine($"New search\nDate {DateTime.Now}\nKeyword {Keyword}, directory {DirectoryName}");
                foreach (FoundWord foundWord in FilesWithFoundWords)
                {
                    string logEntry = $"File name {foundWord.FileNamePath}, path {foundWord.PathFolder}, number of occurrences {foundWord.NumberOfOccurrences}";
                    writer.WriteLine(logEntry);
                }
                writer.WriteLine("End of searching\n");
            }
            MessageBox.Show("Done");
        }
        #endregion
    }
    struct FileNameText
    {
        public string FileName { get; set; }
        public string Text { get; set; }
        public FileNameText(string fileName, string text)
        {
            FileName = fileName;
            Text = text;
        }
    }
}
