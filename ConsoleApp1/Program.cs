using System.Text;
using System.Text.RegularExpressions;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            string text = "Це приклад питального речення? Так, це він. А це звичайне речення.";
            string pattern = @"(?<=[.!?])";
            string[] sentences = Regex.Split(text, pattern);
            foreach (string sentence in sentences)
            {
                if (sentence.Trim().EndsWith("?"))
                {
                    Console.WriteLine($"Питання: {sentence}");
                }
            }
        }
        static bool IsQuestion(string sentence)
        {
            // Визначаємо, чи закінчується речення знаком питання
            return sentence.Trim().EndsWith("?");
        }
    }
}