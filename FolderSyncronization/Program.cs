using FolderSyncronization_Veeam;
using System.Diagnostics;
using System.Timers;
using Timer = System.Timers.Timer;

class Program
{
    public static string _sourcePath = string.Empty;
    public static string _replicaPath = string.Empty;
    public static Timer _timer = new();
    public static int _interval;
    public static string _logFilePath = string.Empty;
    public static object _lock = new();
    public static void Main(string[] args)
    {
        try
        {
            //get source, destination path and interval frequency from user
            Console.WriteLine("Enter the source directory");
            _sourcePath = Console.ReadLine();
            _sourcePath = string.IsNullOrEmpty(_sourcePath) ? throw new ArgumentNullException("Source path is empty") : _sourcePath;
            Console.WriteLine("Enter the replica directory");
            _replicaPath = Console.ReadLine();
            _replicaPath = string.IsNullOrEmpty(_replicaPath) ? throw new ArgumentNullException("Replica path is empty") : _replicaPath;
            if (_replicaPath.Equals(_sourcePath))
            {
                Console.WriteLine("Source and replica's path should be different");
                throw new Exception("Source and replica's path should be different");
            }
            Console.WriteLine("Your desired interval for syncing folders? (mins)");
            _interval = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Enter the logs path");
            _logFilePath = Console.ReadLine();
            _logFilePath = string.IsNullOrEmpty(_logFilePath) ? throw new ArgumentNullException("Logs path is empty") : _logFilePath;
            SetTimer(_interval);
        }
        catch(ArgumentNullException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch(Exception e)
        {
            Log("Error: " + e.Message);
            Console.WriteLine(e.Message);
        }
        
    }

    /// <summary>
    /// Set timer to trigger events periodically
    /// </summary>
    /// <param name="interval"></param>
    public static void SetTimer(int interval)
    {
        _timer.Interval = interval * 60000;
        _timer.Elapsed += OnTimedEvent;
        _timer.Enabled = true;
        _timer.AutoReset = true;
        Log("Event triggered at " + DateTime.UtcNow + " press enter to exit");
        Console.ReadLine();
    }

    /// <summary>
    /// Trigger syncronization for folders and it's directories
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="args"></param>
    public static void OnTimedEvent(object origin, ElapsedEventArgs args)
    {
        Log("Syncronization Started at " + args.SignalTime.ToUniversalTime());
        ThreadPool.QueueUserWorkItem(state => FolderSyncronization.SyncronizeFolders(_sourcePath, _replicaPath)); 
    }

    /// <summary>
    /// Log events and errors if any in specified log path
    /// </summary>
    /// <param name="message"></param>
    public static void Log(string message)
    {
        lock(_lock)
        {
            try
            {
                if (!File.Exists(_logFilePath))
                {
                    using (StreamWriter sw = File.CreateText(_logFilePath))
                    {
                        sw.WriteLine(message);
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(_logFilePath))
                    {
                        sw.WriteLine(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }
    }
}