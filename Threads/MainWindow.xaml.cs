using PropertyChanged;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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

namespace ThreadsL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [AddINotifyPropertyChangedInterface]

    public partial class MainWindow : Window
    {
        [DependsOn(nameof(GenerateRange), nameof(ButtonGenerateRange_Click))]

        public ObservableCollection<long> PrimeNumbers { get; set; } = new ObservableCollection<long>();
        public ObservableCollection<long> FibonacciNumbers { get; set; } = new ObservableCollection<long>();
        LongListToCommaSeparatedStringConverter converter = new LongListToCommaSeparatedStringConverter();
        Thread threadPrimeNumbers;
        Thread threadFibonacci;
        public MainWindow()
        {
            InitializeComponent();
            Binding binding = new Binding("PrimeNumbers");
            binding.Converter = converter;
            binding.StringFormat = "{0}, ";

            myTextBlock.SetBinding(TextBlock.TextProperty, binding);

            Binding binding1 = new Binding("FibonacciNumbers");
            binding1.Converter = converter;
            binding1.StringFormat = "{0}, ";


            myTextBlockFibonacci.SetBinding(TextBlock.TextProperty, binding1);
            DataContext = this;
        }

        private void ButtonGenerateRange_Click(object sender, RoutedEventArgs e)
        {
            if (threadPrimeNumbers != null)
                return;
            string? s_start = TextBoxLeftRange.Text;
            string? s_end = TextBoxRightRange.Text;
            long? start = null;
            long? end = null;
            long tmp1;
            if (!string.IsNullOrEmpty(s_start) && long.TryParse(s_start, out tmp1))
                start = tmp1;
            if (!string.IsNullOrEmpty(s_end) && long.TryParse(s_end, out tmp1))
                end = tmp1;

            if (start == null)
                start = 2;
            RangeTwoNumbers range = new RangeTwoNumbers(collection: PrimeNumbers, start: start, end: end);


            threadPrimeNumbers = new Thread(GenerateRange);
            threadPrimeNumbers.Start(range);

        }

        private void GenerateRange(object obj)
        {
            long i = 0;
            long? start = null;
            long? end = null;
            try
            {
                if (obj is RangeTwoNumbers range)
                {
                    if (range.Start != null)
                        start = range.Start;
                    if (range.End != null)
                        end = range.End;
                    if (start != null && end != null && end < start)
                    {
                        long? tmp = end;
                        end = start;
                        start = tmp;
                    }

                    for (i = start.Value; (end != null && i <= end) || (end == null && true); i++)
                    {
                        PrimeNumbers.Add(i);
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            Binding binding = new Binding("PrimeNumbers");
                            binding.Converter = converter;
                            binding.StringFormat = "{0}, ";

                            myTextBlock.SetBinding(TextBlock.TextProperty, binding);
                        }));
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (ThreadAbortException e)
            {
                Thread.ResetAbort();
                for (; (end != null && i <= end) || (end == null && true); i++)
                {
                    PrimeNumbers.Add(i);
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        Binding binding = new Binding("PrimeNumbers");
                        binding.Converter = converter;
                        binding.StringFormat = "{0}, ";

                        myTextBlock.SetBinding(TextBlock.TextProperty, binding);
                    }));
                    Thread.Sleep(1000);
                }
            }
            finally
            {

            }

        }

        private void StopThreadRangeButton_Click(object sender, RoutedEventArgs e)
        {
            threadPrimeNumbers.Suspend();
        }

        private void PauseThreadRangeButton_Click(object sender, RoutedEventArgs e)
        {
            //if (threadPrimeNumbers != null && threadPrimeNumbers.ThreadState == ThreadState.Running) 
            threadPrimeNumbers.Abort();
        }

        private void ResumeThreadRangeButton_Click(object sender, RoutedEventArgs e)
        {
            //if (threadPrimeNumbers != null && threadPrimeNumbers.ThreadState == ThreadState.Aborted) 
            threadPrimeNumbers.Resume();
        }

        private void ResetThreadRangeButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void GenerateFibonacci()
        {
            try
            {
                if (FibonacciNumbers.Count < 2)
                {
                    FibonacciNumbers.Add(0);
                    FibonacciNumbers.Add(1);
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Binding binding1 = new Binding("FibonacciNumbers");
                    binding1.Converter = converter;
                    binding1.StringFormat = "{0}, ";


                    myTextBlockFibonacci.SetBinding(TextBlock.TextProperty, binding1);
                });
                for (int i = 2; true; i++)
                {
                    long nextFibonacci = FibonacciNumbers[i - 1] + FibonacciNumbers[i - 2];
                    FibonacciNumbers.Add(nextFibonacci);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Binding binding1 = new Binding("FibonacciNumbers");
                        binding1.Converter = converter;
                        binding1.StringFormat = "{0}, ";


                        myTextBlockFibonacci.SetBinding(TextBlock.TextProperty, binding1);
                    });
                    Thread.Sleep(1000);
                }

            }
            catch (ThreadAbortException e)
            {
                Thread.ResetAbort();
                for (int i = FibonacciNumbers.Count; true; i++)
                {
                    long nextFibonacci = FibonacciNumbers[i - 1] + FibonacciNumbers[i - 2];
                    FibonacciNumbers.Add(nextFibonacci);
                }
            }
            finally
            {

            }

        }
        private void ButtonGenerateFibonacci_Click(object sender, RoutedEventArgs e)
        {
            if (threadFibonacci != null)
                return;
            threadFibonacci = new Thread(GenerateFibonacci);
            threadFibonacci.Start();
        }

        private void StopThreadFibonacciButton_Click(object sender, RoutedEventArgs e)
        {
            threadFibonacci.Suspend();
        }

        private void PauseThreadFibonacciButton_Click(object sender, RoutedEventArgs e)
        {
            threadFibonacci.Abort();
        }

        private void ResumeThreadFibonacciButton_Click(object sender, RoutedEventArgs e)
        {
            threadFibonacci.Resume();
        }

        private void ResetThreadFibonacciButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
    struct RangeTwoNumbers
    {
        public long? Start { get; set; }
        public long? End { get; set; }
        public ObservableCollection<long> collection { get; set; }
        public RangeTwoNumbers(ObservableCollection<long> collection, long? start = null, long? end = null)
        {
            this.collection = collection ?? new ObservableCollection<long>();
            Start = start;
            End = end;
        }
    }
    [AddINotifyPropertyChangedInterface]

    public class LongListToCommaSeparatedStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Collections.IEnumerable enumerable)
            {
                return string.Join(", ", enumerable.OfType<long>());
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
