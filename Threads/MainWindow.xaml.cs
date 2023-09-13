using PropertyChanged;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
        CancellationTokenSource cts_prime = new CancellationTokenSource();
        CancellationToken token_prime;
        ManualResetEvent resetEvent_prime = new ManualResetEvent(true);
        bool isGeneratingPrimes = false;
        CancellationTokenSource cts_fibonacci = new CancellationTokenSource();
        CancellationToken token_fibonacci;
        ManualResetEvent resetEvent_fibonacci = new ManualResetEvent(true);
        bool isGeneratingFibonacci = false;

        public MainWindow()
        {
            token_prime = cts_prime.Token;
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
            StartNewRange();
        }
        private void StartNewRange()
        {
            if (isGeneratingPrimes == true)
                return;
            isGeneratingPrimes = true;
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
            cts_prime = new();
            token_prime = cts_prime.Token;
            RangeTwoNumbers range = new RangeTwoNumbers(collection: PrimeNumbers, token: token_prime, resetEvent_prime, start: start, end: end);


            threadPrimeNumbers = new Thread(GenerateRange);
            threadPrimeNumbers.Start(range);
        }
        private void GenerateRange(object obj)
        {
            CancellationToken token = ((RangeTwoNumbers)obj).token;
            try
            {
                EventWaitHandle ev = ((RangeTwoNumbers)obj).ev;
                long? start = null;
                long? end = null;

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

                    for (long i = start.Value; (end != null && i <= end) || (end == null && true); i++)
                    {
                        //wait
                        ev.WaitOne();
                        PrimeNumbers.Add(i);
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            Binding binding = new Binding("PrimeNumbers");
                            binding.Converter = converter;
                            binding.StringFormat = "{0}, ";

                            myTextBlock.SetBinding(TextBlock.TextProperty, binding);
                        }));
                        token.ThrowIfCancellationRequested();
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (OperationCanceledException e)
            {
                MessageBox.Show("Block try catch");
            }
            finally
            {
                resetEvent_prime.Set();     // set to signaled state
                MessageBox.Show("Block finaly");
                isGeneratingPrimes = false;

            }

        }

        private void StopThreadRangeButton_Click(object sender, RoutedEventArgs e)
        {
            cts_prime.Cancel();
        }

        private void PauseThreadRangeButton_Click(object sender, RoutedEventArgs e)
        {
            resetEvent_prime.Reset();
        }

        private void ResumeThreadRangeButton_Click(object sender, RoutedEventArgs e)
        {
            resetEvent_prime.Set();
        }

        private void ResetThreadRangeButton_Click(object sender, RoutedEventArgs e)
        {
            cts_prime.Cancel();
            StartNewRange();
        }
        private void GenerateFibonacci(object obj)
        {
            CancellationToken token = ((CancellationToken)obj);
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
                    resetEvent_fibonacci.WaitOne();
                    long nextFibonacci = FibonacciNumbers[i - 1] + FibonacciNumbers[i - 2];
                    FibonacciNumbers.Add(nextFibonacci);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Binding binding1 = new Binding("FibonacciNumbers");
                        binding1.Converter = converter;
                        binding1.StringFormat = "{0}, ";


                        myTextBlockFibonacci.SetBinding(TextBlock.TextProperty, binding1);
                    });
                    token.ThrowIfCancellationRequested();
                    Thread.Sleep(1000);
                }
            }
            catch (OperationCanceledException e)
            {
                MessageBox.Show("Block try catch");
            }
            finally
            {
                resetEvent_fibonacci.Set();
                MessageBox.Show("Block finaly");
                isGeneratingFibonacci = false;
            }
        }

        private void ButtonGenerateFibonacci_Click(object sender, RoutedEventArgs e)
        {
            StartFibonacciNumbers();
        }
        private void StartFibonacciNumbers()
        {
            if (isGeneratingFibonacci == true)
                return;
            isGeneratingFibonacci = true;
            cts_fibonacci = new();
            token_fibonacci = cts_fibonacci.Token;
            threadFibonacci = new Thread(GenerateFibonacci);
            threadFibonacci.Start(token_fibonacci);

        }
        private void StopThreadFibonacciButton_Click(object sender, RoutedEventArgs e)
        {
            cts_fibonacci.Cancel();
        }

        private void PauseThreadFibonacciButton_Click(object sender, RoutedEventArgs e)
        {
            resetEvent_fibonacci.Reset();
        }

        private void ResumeThreadFibonacciButton_Click(object sender, RoutedEventArgs e)
        {
            resetEvent_fibonacci.Set();
        }

        private void ResetThreadFibonacciButton_Click(object sender, RoutedEventArgs e)
        {
            cts_fibonacci.Cancel();
            StartFibonacciNumbers();
        }
    }
    struct RangeTwoNumbers
    {
        public long? Start { get; set; }
        public long? End { get; set; }
        public ObservableCollection<long> collection { get; set; }
        public CancellationToken token { get; set; }
        public EventWaitHandle ev { get; set; }
        public RangeTwoNumbers(ObservableCollection<long> collection, CancellationToken token, EventWaitHandle ev, long? start = null, long? end = null)
        {
            this.collection = collection ?? new ObservableCollection<long>();
            Start = start;
            End = end;
            this.token = token;
            this.ev = ev;
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
