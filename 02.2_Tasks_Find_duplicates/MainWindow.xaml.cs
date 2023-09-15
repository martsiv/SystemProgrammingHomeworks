using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using Path = System.IO.Path;
using XSystem.Security.Cryptography;

namespace _02._2_Tasks_Find_duplicates
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string sourceDirectory = string.Empty;
        private string targetDirectory = string.Empty;
        //Логер зберігає файл у папці з проектом
        private Logger logger = new Logger("log.txt");
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SourceDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    sourceDirectory = dialog.SelectedPath;
            }
        }
        private void TargetDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    targetDirectory = dialog.SelectedPath;
            }
        }
        private void StartCopyButton_Click(object sender, RoutedEventArgs e)
        {
            CopyUniqueFiles(sourceDirectory, targetDirectory, logger);
        }
        static void CopyUniqueFiles(string sourceDirectory, string targetDirectory, Logger logger)
        {
            logger.Log("Запуск нового копіювання");
            logger.Log($"Директорія-джерело: {sourceDirectory}");
            logger.Log($"Директорія-ціль: {targetDirectory}");
            if (!Directory.Exists(sourceDirectory) || !Directory.Exists(targetDirectory))
            {
                logger.Log("Директорії-джерело або-і ціль не існують.");
                return;
            }

            Dictionary<string, string> fileHashes = new Dictionary<string, string>();

            DirectoryInfo sourceInfo = new DirectoryInfo(sourceDirectory);
            DirectoryInfo targetInfo = new DirectoryInfo(targetDirectory);

            foreach (FileInfo sourceFile in sourceInfo.GetFiles())
            {
                string sourceFilePath = sourceFile.FullName;
                string fileHash = CalculateFileHash(sourceFilePath);
                
                if (File.Exists(Path.Combine(targetDirectory, sourceFile.Name)))
                {
                    // Файл існує в директорії-цілі
                    logger.Log($"Файл вже існує (дублікат): {sourceFile.Name}");
                }
                else if (fileHashes.ContainsKey(fileHash))
                {
                    logger.Log($"Файл є дублікатом, тому його не скопійовано: {sourceFile.Name}");
                }
                else
                {
                    string targetFilePath = Path.Combine(targetDirectory, sourceFile.Name);
                    File.Copy(sourceFilePath, targetFilePath);
                    fileHashes.Add(fileHash, sourceFilePath);

                    logger.Log($"Скопійовано файл: {sourceFile.Name}");
                }
            }

            logger.Log("Операція завершена.");
        }

        static string CalculateFileHash(string filePath)
        {
            using (FileStream stream = File.OpenRead(filePath))
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] hash = sha.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }

        
        //public void CopyFilesWithoutDuplicates(string sourceDirectory, string targetDirectory, Logger logger)
        //{
        //    if (!Directory.Exists(sourceDirectory) || !Directory.Exists(targetDirectory))
        //    {
        //        logger.Log("Директорії-джерело або-і ціль не існують.");
        //        return;
        //    }

        //    DirectoryInfo sourceInfo = new DirectoryInfo(sourceDirectory);
        //    DirectoryInfo targetInfo = new DirectoryInfo(targetDirectory);

        //    foreach (FileInfo sourceFile in sourceInfo.GetFiles())
        //    {
        //        string targetFilePath = Path.Combine(targetDirectory, sourceFile.Name);

        //        if (!File.Exists(targetFilePath))
        //        {
        //            // Файл не існує в директорії-цілі, копіюємо його туди
        //            sourceFile.CopyTo(targetFilePath);
        //            logger.Log($"Скопійовано файл: {sourceFile.Name}");
        //        }
        //        else
        //        {
        //            // Файл вже існує в директорії-цілі, можливо, це дублікат
        //            logger.Log($"Файл вже існує: {sourceFile.Name}");
        //        }
        //    }

        //    logger.Log("Операція завершена.");
        //}
    }
    public class Logger
    {
        private string logFilePath;

        public Logger(string filePath)
        {
            logFilePath = filePath;
        }

        public void Log(string message)
        {
            string logMessage = $"{DateTime.Now}: {message}";
            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
            Console.WriteLine(logMessage);
        }
    }
}
