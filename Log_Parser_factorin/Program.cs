using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LogParcer_factorin
{
    class Program
    {
        // Константа-regex паттерн, учавствующая в фильтрации строк лога по нужным событиям
        private const string pattern = @"(\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}).+\[ReceiveDltEventsJob\] job (started|finished)";

        static void Main(string[] args)
        {
            Regex regExpression = new Regex(pattern);

            //Парсинг лог-файла, получение необходимых данных и вычисление нужных значений

            List<ParseData> log = new List<ParseData>();

            var path = Path.Combine(Environment.CurrentDirectory + @"\factorin_log.txt");

            using (StreamReader sr = new StreamReader(path))
            {
                string line;
                DateTime startDate = DateTime.MinValue, finishDate = DateTime.MinValue;
                TimeSpan executionTime;

                while ((line = sr.ReadLine()) != null)
                {
                    // Находим все строки в логе, которые соответствуют нашему regex паттерну
                    var match = regExpression.Match(line);

                    if (match.Success)
                    {
                        DateTime logTime = DateTime.ParseExact(match.Groups[1].Value, "MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                        if (match.Groups[2].Value == "started")
                        {
                            startDate = logTime;
                        }
                        else
                        {
                            // Для нахождения разницы (Timespan) времени выполнения из конечной даты вычитаем начальную.
                            // Полученные данные добавляем в коллекцию
                            finishDate = logTime;
                            executionTime = finishDate.Subtract(startDate);

                            log.Add(new ParseData
                            {
                                ExecutionTime = executionTime,
                                StartDate = startDate,
                                FinishDate = finishDate
                            });
                        }
                    }
                }

                //Формирование отчета с необходимой информацией и вывод на консоль
                var items = log.OrderByDescending(x => x.ExecutionTime);
                var DisplayExecutionTimeMax = items.Select(p => p.ExecutionTime.TotalMilliseconds).Max();
                var DisplayExecutionTimenMin = items.Select(p => p.ExecutionTime.TotalMilliseconds).Min();
                var DisplayExecutionTimeAverage = items.Select(p => p.ExecutionTime.TotalMilliseconds).Average();
                var Count = items.Count();

                Console.WriteLine("Time Of Execution task [ReceiveDltEventsJob]");
                Console.WriteLine("Maximum: {0} Millisecond", DisplayExecutionTimeMax);
                Console.WriteLine("Minimum: {0} Millisecond", DisplayExecutionTimenMin);
                Console.WriteLine("Average: {0} Millisecond", DisplayExecutionTimeAverage);
                Console.WriteLine("Count of events: {0}\n ", Count);
                Console.WriteLine("----------------------------------------------------------------------------");
                Console.Write("Time of execution    | Start date                     | Finish date \n");
                Console.WriteLine("----------------------------------------------------------------------------");

                foreach (var item in items)
                {
                    Console.WriteLine(String.Format("{0, -20} | {1, -30} | {2}", item.ExecutionTime.TotalMilliseconds, item.StartDate, item.FinishDate));
                }
            }

            Console.ReadLine();
        }
    }

    /// <summary>
    /// Вспомогательный класс для хранения необходимых свойств
    /// </summary>
    public class ParseData
    {
        public TimeSpan ExecutionTime { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }
    }
}
