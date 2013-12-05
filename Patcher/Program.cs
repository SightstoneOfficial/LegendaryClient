using System.IO;

namespace Patcher
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (Directory.Exists("Patch"))
            {
                foreach (string newPath in Directory.GetFiles("Patch", "*.*", SearchOption.AllDirectories))
                    File.Copy(newPath, newPath.Replace("Patch", "."), true);
            }
            System.IO.DirectoryInfo PatchInfo = new DirectoryInfo("Patch");

            foreach (FileInfo file in PatchInfo.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in PatchInfo.GetDirectories())
            {
                dir.Delete(true);
            }
            Directory.Delete("Patch");
            System.Diagnostics.Process.Start("LegendaryClient.exe");
        }
    }
}