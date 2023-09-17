using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using Path = System.IO.Path;

namespace _02._1_Tasks_Text_analizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [AddINotifyPropertyChangedInterface]

    public partial class MainWindow : Window
    {
        string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
        private string _filename = "result.txt";
        public string TextBeforeCalculating { get; set; }
        public Stata TextStatistic { get; set; } = new();
        private WindowResults windowResults;
        private CancellationTokenSource cancelTokenSource;
        private CancellationToken token;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            Calculate();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (!token.IsCancellationRequested)
                cancelTokenSource.Cancel();
        }
        private async Task Calculate()
        {
            cancelTokenSource = new CancellationTokenSource();
            token = cancelTokenSource.Token;
            TextStatistic = new Stata();
            TextBeforeCalculating = TextBoxMain.Text;
            var allTasks = new List<Task>();

            try
            {
                if (CheckBoxSentences.IsChecked == true)
                    allTasks.Add(Task.Run(() => CalculateSentencesInText(TextBeforeCalculating), token));
                if (CheckBoxSymbols.IsChecked == true)
                    allTasks.Add(Task.Run(() => CalculateSymbolsInText(TextBeforeCalculating), token));
                if (CheckBoxWords.IsChecked == true)
                    allTasks.Add(Task.Run(() => CalculateWordsInText(TextBeforeCalculating), token));
                if (CheckBoxQuestion.IsChecked == true)
                    allTasks.Add(Task.Run(() => CalculateQuestionSentencesInText(TextBeforeCalculating), token));
                if (CheckBoxExclamatory.IsChecked == true)
                    allTasks.Add(Task.Run(() => CalculateExclamatorySentencesInText(TextBeforeCalculating), token));

                await Task.WhenAll(allTasks);

                if (RadioButtonWindow.IsChecked == true)
                {
                    windowResults = new WindowResults(TextStatistic);
                    windowResults.Show();
                }
                else if (RadioButtonFile.IsChecked == true)
                {
                    using (StreamWriter writer = new StreamWriter(Path.Combine(projectDirectory, _filename), false))
                    {
                        writer.WriteLine(await CombineTextIntoFile());
                        MessageBox.Show("Saved successfull!");
                    }
                }
            }
            catch (Exception ex)
            {
                    if (ex is TaskCanceledException)
                        MessageBox.Show("Stopped operation!");
                    else
                        MessageBox.Show(ex.Message);
            }
            finally
            {
                cancelTokenSource.Dispose();
            }
        }
        private async Task<string> CombineTextIntoFile()
        {
            string result = "Number of sentences: " + TextStatistic.Sentences + "\n";
            result += "Number of characters:" + TextStatistic.Symbols + "\n";
            result += "Number of words:" + TextStatistic.Words + "\n";
            result += "Number of question sentences:" + TextStatistic.QuestionSentences + "\n";
            result += "Number of exclamatory sentences:" + TextStatistic.ExclamatorySentences + "\n";
            return result;
        }

        private async Task CalculateSentencesInText(string text)
        {
            string[] sentences = text.Split(new char[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            int count = sentences.Length;
            foreach (string sentence in sentences)
            {
                Thread.Sleep(5000);
                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();
                string trimmedSentence = sentence.Trim();
                if (string.IsNullOrWhiteSpace(trimmedSentence))
                    --count;
            }
            TextStatistic.Sentences = count;
        }
        private async Task CalculateWordsInText(string text)
        {
            // Розділіть текст на слова за допомогою пробілів і розділових знаків
            string[] words = text.Split(new char[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

            // Вирахуйте кількість слів
            int count = words.Length;
            foreach (string word in words)
            {
                //Thread.Sleep(1000);
                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();
                string trimmedWord = word.Trim();
                if (string.IsNullOrWhiteSpace(trimmedWord))
                    --count;
            }
            TextStatistic.Words = count;
        }
        private async Task CalculateSymbolsInText(string text)
        {
            // Створюємо масив символів пунктуації, які вас цікавлять
            char[] punctuationChars = { '.', ',', '!', '?', ';', ':', '(', ')', '[', ']', '{', '}', '<', '>', '"', '\'' };

            // Ініціалізуємо лічильник пунктуації
            int count = 0;

            // Перебираємо кожен символ в тексті
            foreach (char character in text)
            {
                //Thread.Sleep(1000);
                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();
                // Якщо символ є пунктуацією, збільшуємо лічильник
                if (Array.IndexOf(punctuationChars, character) != -1)
                    count++;
            }

            TextStatistic.Symbols = count;
        }
        private async Task CalculateQuestionSentencesInText(string text)
        {
            int counter = 0;
            string pattern = @"(?<=[.!?])";
            string[] sentences = Regex.Split(text, pattern);
            foreach (string sentence in sentences)
            {
                //Thread.Sleep(1000);
                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();
                string trimmedSentence = sentence.Trim();
                if (trimmedSentence != "?" && trimmedSentence.EndsWith("?"))
                    ++counter;
            }
            TextStatistic.QuestionSentences = counter;
        }
        private async Task CalculateExclamatorySentencesInText(string text)
        {
            int counter = 0;
            string pattern = @"(?<=[.!?])";
            string[] sentences = Regex.Split(text, pattern);
            foreach (string sentence in sentences)
            {
                //Thread.Sleep(1000);
                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();
                string trimmedSentence = sentence.Trim();
                if (trimmedSentence != "!" && trimmedSentence.EndsWith("!"))
                    ++counter;
            }
            TextStatistic.ExclamatorySentences = counter;
        }
    }
    public class Stata
    {
        public int Sentences { get; set; } = 0;
        public int Symbols { get; set; } = 0;
        public int Words { get; set; } = 0;
        public int QuestionSentences { get; set; } = 0;
        public int ExclamatorySentences { get; set; } = 0;
    }
    public class CheckBoxesToButtonEnabledConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool isTextBoxEmpty = string.IsNullOrWhiteSpace(values[values.Length - 1] as string);

            // Перевіряємо, чи всі значення в масиві - true (вибрані всі чекбокси)
            foreach (var item in values)
            {
                if (item is bool res && res == true && !isTextBoxEmpty)
                    return true;
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}