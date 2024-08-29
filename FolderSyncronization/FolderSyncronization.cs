namespace FolderSyncronization_Veeam
{
    public class FolderSyncronization
    {
        /// <summary>
        /// Syncronize th source directory to the replica directory
        /// </summary>
        /// <param name="sourceDirectory"></param>
        /// <param name="replicaDirectory"></param>
        /// <exception cref="Exception"></exception>
        public static void SyncronizeFolders(string sourceDirectory, string replicaDirectory)
        {
            try
            {
                //check if the directories exist
                if (!Directory.Exists(sourceDirectory))
                {
                    throw new DirectoryNotFoundException($"Source directory not found: {sourceDirectory}");
                }

                if (!Directory.Exists(replicaDirectory))
                {
                    Directory.CreateDirectory(replicaDirectory);
                }

                Console.WriteLine($"Syncronizing {sourceDirectory} to {replicaDirectory}");
                Program.Log("Syncronizing" +sourceDirectory+ "to" +replicaDirectory);

                //copy files from source to replica after validation
                foreach (string files in Directory.GetFiles(sourceDirectory))
                {
                    var sourceFile = Path.GetFileName(files);
                    var replicaFileDir = Path.Combine(replicaDirectory, sourceFile);

                    if (!File.Exists(replicaFileDir) || File.GetLastAccessTimeUtc(replicaFileDir) < File.GetLastAccessTimeUtc(sourceFile))
                    {
                        File.Copy(files, replicaFileDir, true);
                        Console.WriteLine("Copying files from source to replica folder");
                        Program.Log("Copying files from source to replica folder");
                    }

                }

                //Delete the files that are not present in the source directory
                foreach (string files in Directory.GetFiles(replicaDirectory))
                {
                    var replicaFile = Path.GetFileName(files);
                    var sourceFileDir = Path.Combine(sourceDirectory, replicaFile);

                    if (!File.Exists(sourceFileDir))
                    {
                        File.Delete(files);
                        Console.WriteLine("Deleted outdated files from replica folder");
                        Program.Log("Deleting outdated files from replica folder");
                    }
                }

                //Delete outdated directories in the replica
                foreach (string directory in Directory.GetDirectories(replicaDirectory))
                {
                    var replicaFileName = Path.GetDirectoryName(directory);
                    var sourceDir = Path.Combine(sourceDirectory, replicaFileName);

                    if (!File.Exists(sourceDir))
                    {
                        Directory.Delete(directory, true);
                        Console.WriteLine("Deleted outdated directories from replica");
                        Program.Log("Deleted outdated directories from replica");
                    }
                }

                //copy sub folders from source to replica
                string[] sourceSubDirectories = Directory.GetDirectories(sourceDirectory);
                foreach (var sourceSubDir in sourceSubDirectories)
                {
                    string subDirName = Path.GetFileName(sourceSubDir);
                    string destSubDir = Path.Combine(replicaDirectory, subDirName);

                    ThreadPool.QueueUserWorkItem(state => SyncronizeFolders(sourceSubDir, destSubDir));
                }

                Console.WriteLine("Syncronized directories with source");
                Program.Log("Syncronization completed at " + DateTime.UtcNow);

            }
            catch (Exception e)
            {
                Program.Log("Error: " + e.Message);
                Console.WriteLine(e.Message);
            }
        }
    }
}
