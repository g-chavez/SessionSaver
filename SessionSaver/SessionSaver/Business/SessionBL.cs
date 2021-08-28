using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SessionSaver.Model;

namespace SessionSaver.Business
{
    public class SessionBL
    {
        public const string SuccessMessage = "Session saved";
        private const string DirectoryNotFoundMessage = "Destination directory not found. Please try a different directory.";
        private const string FileNotFoundMessage = "Destination file not found. Please try a different file name.";
        private const string PathTooLongMessage = "Destination path is too long. Please try a shorter path.";
        private const string NotSupportedMessage = "Action not supported. Please try other path or opening the application as administrator.";
        private const string SecurityMessage = "Security issue occured. Please try other path or opening the application as administrator.";
        private const string UnauthorizedAccessMessage = "Unauthorized access. Please try other path or opening the application as administrator.";
        private const string ExceptionMessage = "The session couldn't be saved. Please try again with other directory.";

        public string SaveSession(string filePath, List<Application> applications)
        {
            //string path = "C:\\Users\\*\\Desktop\\Folder\\";
            //string fileName = GetFileName();
            //string filePath = path + fileName;

            List<string> commandList = new List<string>();
            commandList.Add("@echo off");
            commandList.AddRange(
                applications.Select(
                    x => x.CommandLine
                ).ToList()
                );

            string result = SaveSessionFile(filePath, commandList);
            return result;
        }

        public string SaveSessionFile(string filePath, List<string> commandList)
        {
            try
            {
                File.WriteAllLines(     //.AppendAllLines(
                filePath,
                commandList,
                System.Text.Encoding.ASCII
                );

                return SuccessMessage + " at: " + DateTime.Now.ToString();
            }
            catch (DirectoryNotFoundException)
            {
                return DirectoryNotFoundMessage;
            }
            catch (FileNotFoundException)
            {
                return FileNotFoundMessage;
            }
            catch (PathTooLongException)
            {
                return PathTooLongMessage;
            }
            catch (NotSupportedException)
            {
                return NotSupportedMessage;
            }
            catch (System.Security.SecurityException)
            {
                return SecurityMessage;
            }
            catch (UnauthorizedAccessException)
            {
                return UnauthorizedAccessMessage;
            }
            catch (Exception)
            {
                return ExceptionMessage;
            }
        }

        public string GetFileName()
        {
            DateTime CurrentDate = DateTime.Now;
            string Day = CurrentDate.Day.ToString().PadLeft(2, '0');
            string Month = CurrentDate.Month.ToString().PadLeft(2, '0');

            return Day + "-" + Month + "-" + CurrentDate.Year.ToString() + ".bat";
        }
    }
}
