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
        // Константы, учавствующие в фильтрации лога по необходимомым совпадениям
        private const string eventJobStarted = "[ReceiveDltEventsJob] job started";
        private const string eventJobFinished = "[ReceiveDltEventsJob] job finished";

        /* "The ( ) acts as a capture group. 
        So the matches array has all of matches that C# finds 
        in your string and the sub array has the values of the capture groups inside of those matches" */

        private const string pattern = @"(\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}).+\[ReceiveDltEventsJob\] job (started|finished)";

        static void Main(string[] args)
        {
            //Регулярное выражение для парсинга дата и времени
            Regex logRegex = new Regex(pattern);

            //Парсинг лог-файла
            List<Data> logs = new List<Data>();
            var path = Path.Combine(Environment.CurrentDirectory + @"\factorin_log.txt");
            using (StreamReader sr = new StreamReader(path))
            {
                string line;
                DateTime sed = DateTime.MinValue, finishEventDate;
                TimeSpan timeDuration;

                while ((line = sr.ReadLine()) != null)
                {
                    var match = logRegex.Match(line);
                    if (match.Success)
                    {
                        //DateTime logtime = DateTime.ParseExact(match.Value, "MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                        DateTime logtime = DateTime.ParseExact(match.Groups[1].Value, "MM-dd HH:mm:ss", CultureInfo.InvariantCulture);


                        if (match.Groups[2].Value == "started")
                        {
                            sed = logtime;
                        }
                        else
                        {
                            finishEventDate = logtime;
                            timeDuration = finishEventDate.Subtract(sed);

                            logs.Add(new Data
                            {
                                TimeDuration = timeDuration,
                                OperationDateStarted = sed,
                                OperationDateFinished = finishEventDate

                            });
                        }
                    }
                }

                //part two - calculate time

                var r = logs.OrderByDescending(x => x.TimeDuration);
                var ViewTimeDurationMax = r.Select(p => p.TimeDuration.TotalMilliseconds).Max();
                var ViewTimeDurationMin = r.Select(p => p.TimeDuration.TotalMilliseconds).Min();
                var ViewTimeDurationAverage = r.Select(p => p.TimeDuration.TotalMilliseconds).Average();
                var Count = r.Count();

                Console.WriteLine("Time Of Execution task [ReceiveDltEventsJob]");
                Console.WriteLine("Maximum: {0} Millisecond", ViewTimeDurationMax);
                Console.WriteLine("Minimum: {0} Millisecond", ViewTimeDurationMin);
                Console.WriteLine("Average: {0} Millisecond", ViewTimeDurationAverage);
                Console.WriteLine("Count of events: {0}\n ", Count);
                Console.WriteLine("-----------------------------------------------------------------------");
                Console.Write("Time of execution    | Date started                   | Date finished \n");
                Console.WriteLine("-----------------------------------------------------------------------");

                foreach (var l in r)
                {
                    Console.WriteLine(String.Format("{0, -20} | {1, -30} | {2}", l.TimeDuration.TotalMilliseconds, l.OperationDateStarted, l.OperationDateFinished));
                }
            }

            Console.ReadLine();
        }
    }

    /// <summary>
    /// Class of Data
    /// </summary>
    public class Data
    {
        public TimeSpan TimeDuration { get; set; }
        public DateTime OperationDateStarted { get; set; }
        public DateTime OperationDateFinished { get; set; }
    }
}
