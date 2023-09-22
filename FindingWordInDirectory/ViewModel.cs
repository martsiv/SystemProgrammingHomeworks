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
        public string DirectoryName { get; set; }
        public string Keyword { get; set; }
        bool isDisposedToken = true;

        public int Progress { get; set; }
        public ViewModel()
        {
            FilesWithFoundWords = new ObservableCollection<FoundWord>();
            chooseDirectoryButton_Click = new((o) => ChooseDirectory());
            startSearchButton_Click = new((o) => StartSearchAsync(), (с) => string.IsNullOrEmpty(Keyword) == false && isDisposedToken == true);
            stopButton_Click = new((o) => Stop(), (с) => isDisposedToken == false);
            saveResultButton_Click = new((o) => SaveResult(), (с) => Progress == 100);
        }

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

        private readonly RelayCommand startSearchButton_Click;
        public ICommand StartSearchButton_Click => startSearchButton_Click;
        private async Task StartSearchAsync()
        {
            FilesWithFoundWords.Clear();
            Progress = 0;

            cancelTokenSource = new CancellationTokenSource();
            token = cancelTokenSource.Token;
            isDisposedToken = false;
            try
            {
                await Task.Run(async () =>
                {
                    // Отримуємо всі файли *.txt в директорії та в її піддиректоріях
                    string[] txtFiles = Directory.GetFiles(DirectoryName, "*.txt", SearchOption.AllDirectories);

                    int totalFiles = txtFiles.Length;
                    int processedFiles = 0;

                    foreach (string filePath in txtFiles)
                    {
                        await Task.Delay(1000);
                        if (token.IsCancellationRequested)
                            token.ThrowIfCancellationRequested();
                        // Читаємо вміст файлу
                        string fileContents = File.ReadAllText(filePath);

                        // Рахуємо кількість входжень ключового слова
                        int numberOfOccurrences = await CountOccurrences(fileContents, Keyword);

                        if (numberOfOccurrences > 0)
                        {
                            FoundWord foundWord = new FoundWord
                            {
                                FileName = filePath,
                                PathFolder = Path.GetDirectoryName(filePath),
                                NumberOfOccurrences = numberOfOccurrences
                            };
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                FilesWithFoundWords.Add(foundWord);
                            });
                        }

                        processedFiles++;
                        // Оновлюємо властивість Progress, щоб відобразити прогрес

                        Progress = (int)((double)processedFiles / totalFiles * 100);
                    }
                });
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
        private async Task<int> CountOccurrences(string text, string keyword)
        {
            // Використовуємо LINQ для підрахунку кількості входжень ключового слова
            return text.Split(new string[] { keyword }, StringSplitOptions.RemoveEmptyEntries).Length - 1;
        }
        private readonly RelayCommand stopButton_Click;
        public ICommand StopButton_Click => stopButton_Click;
        private void Stop()
        {
            cancelTokenSource.Cancel();
        }
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
    }
}
