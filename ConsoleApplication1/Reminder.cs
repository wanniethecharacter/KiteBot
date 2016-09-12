using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Newtonsoft.Json;

namespace KiteBot
{
    static class Reminder
    {
        public static string RootDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent?.Parent?.FullName;
        public static string ReminderPath => RootDirectory + "/Content/ReminderList.json";

        private static readonly Regex Regex = new Regex(@"(![Rr]eminder)\s+(?<digits>\d+)\s+(?<unit>\w+)(?:\s+(?<reason>[\w\d\s':/`\\\.,!?]+))?");

        private static Timer _reminderTimer;
        private static LinkedList<ReminderEvent> _reminderList;

        static Reminder()
        {
            _reminderList = File.Exists(ReminderPath) ? 
                JsonConvert.DeserializeObject<LinkedList<ReminderEvent>>(File.ReadAllText(ReminderPath)) :
                new LinkedList<ReminderEvent>();

            _reminderTimer = new Timer();
            _reminderTimer.Elapsed += CheckReminders;
            _reminderTimer.AutoReset = false;

            List<ReminderEvent> deleteBuffer = new List<ReminderEvent>(_reminderList.Count);
            foreach (var reminder in _reminderList)
            {
                if (_reminderList.First.Value.RequestedTime <= DateTime.Now)
                {
                    deleteBuffer.Add(reminder);
                }
            }
            deleteList(deleteBuffer);
            if (_reminderList.Count != 0)
            {
                SetTimer(_reminderList.First.Value.RequestedTime);
            }
        }

        public static string AddNewEvent(Message message)
        {
            Match matches = Regex.Match(message.Text);
            if (matches.Success)
            {
                var milliseconds = 0;
                switch (matches.Groups["unit"].Value.ToLower()[0])
                {
                    case 's':
                        milliseconds = int.Parse(matches.Groups["digits"].Value) * 1000;
                        break;
                    case 'm':
                        milliseconds = int.Parse(matches.Groups["digits"].Value) * 1000 * 60;
                        break;
                    case 'h':
                        milliseconds = int.Parse(matches.Groups["digits"].Value) * 1000 * 60 * 60;
                        break;
                    case 'd':
                        milliseconds = int.Parse(matches.Groups["digits"].Value) * 1000 * 60 * 60 * 24;
                        break;
                    default:
                        return "Couldn't find any supported time units, please use [seconds|minutes|hour|days]";
                }

                ReminderEvent reminderEvent = new ReminderEvent
                {
                    RequestedTime = DateTime.Now.AddMilliseconds(milliseconds),
                    UserId = message.User.Id,
                    Reason = matches.Groups["reason"].Success ? matches.Groups["reason"].Value : "No specified reason"
                };

                if (_reminderList.Count == 0)
                {
                    _reminderList.AddFirst(reminderEvent);
                    SetTimer(reminderEvent.RequestedTime);
                }
                else
                {
                    var laternode = _reminderList.EnumerateNodes().FirstOrDefault(x => x.Value.RequestedTime.CompareTo(reminderEvent.RequestedTime) > 0);
                    if (laternode == null)
                    {
                        _reminderList.AddLast(reminderEvent);
                    }
                    else
                    {
                        _reminderList.AddBefore(laternode, reminderEvent);
                    }
                }
                Save();
                return $"Reminder set for {reminderEvent.RequestedTime.ToUniversalTime().ToString("g",new CultureInfo("en-US"))} UTC with reason: {reminderEvent.Reason}";

            }
            return "Couldn't parse your command, please use the format \"!Reminder [number] [seconds|minutes|hour|days] [optional: reason for reminder]\"";
        }

        private static void SetTimer(DateTime newTimer)
        {
            TimeSpan interval = newTimer - DateTime.Now;
            _reminderTimer.Interval = interval.TotalMilliseconds;
            _reminderTimer.Enabled = true;
        }

        private static async Task CheckReminders(ElapsedEventArgs elapsedEventArgs)
        {
            List<ReminderEvent> deleteBuffer = new List<ReminderEvent>(_reminderList.Count);

            foreach (ReminderEvent reminder in _reminderList)
            {
                if (reminder.RequestedTime.CompareTo(DateTime.Now) <= 0)
                {
                    var channel = Program.Client.Servers.First().GetUser(reminder.UserId).PrivateChannel ??
                                  await Program.Client.Servers.First().GetUser(reminder.UserId).CreatePMChannel();
                    await channel.SendMessage($"Reminder: {reminder.Reason}");

                    deleteBuffer.Add(reminder);
                    if (_reminderList.Count == 0) break;
                }
            }
            deleteList(deleteBuffer);
            deleteBuffer = null;
            if (_reminderList.Count != 0)
            {
                SetTimer(_reminderList.First.Value.RequestedTime);
            }
            Save();
        }

        private static void deleteList(List<ReminderEvent> deleteBuffer)
        {
            foreach (var lapsedEvent in deleteBuffer)
            {
                _reminderList.Remove(lapsedEvent);
            }
        }

        private static async void CheckReminders(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            await CheckReminders(elapsedEventArgs);
        }

        private static void Save()
        {
            File.WriteAllText(ReminderPath,JsonConvert.SerializeObject(_reminderList));
        }

        struct ReminderEvent
        {
            public DateTime RequestedTime { get; set; }
            public ulong UserId { get; set; }
            public string Reason { get; set; }
        }
    }
    public static class LinkedListExtensions
    {
        public static IEnumerable<LinkedListNode<T>> EnumerateNodes<T>(this LinkedList<T> list)
        {
            var node = list.First;
            while (node != null)
            {
                yield return node;
                node = node.Next;
            }
        }
    }
}
