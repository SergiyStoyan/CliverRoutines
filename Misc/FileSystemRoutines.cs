﻿//********************************************************************************************
//Author: Sergiy Stoyan
//        s.y.stoyan@gmail.com, sergiy.stoyan@outlook.com, stoyan@cliversoft.com
//        http://www.cliversoft.com
//********************************************************************************************
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace Cliver
{
    public class FileSystemRoutines
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="PATHs">dirs listes like environmentVariable PATH</param>
        /// <param name="PATHEXTs">extensions listed like environmentVariable PATHEXT</param>
        /// <returns></returns>
        public static string FindFullCommandLinePath(string fileName, string PATHs, string PATHEXTs)
        {
            var paths = new[] { Environment.CurrentDirectory }.Concat(PATHs.Split(';'));
            var extensions = new[] { string.Empty }.Concat(PATHEXTs.Split(';').Where(e => e.StartsWith(".")));
            var combinations = paths.SelectMany(x => extensions, (path, extension) => Path.Combine(path, fileName + extension));
            return combinations.FirstOrDefault(File.Exists);
        }

        public static bool IsCaseSensitive
        {
            get
            {
                if (_isCaseSensitive == null)
                    _isCaseSensitive = isCaseSensitive();
                return (bool)_isCaseSensitive;
            }
        }
        static bool? _isCaseSensitive = null;
        static bool isCaseSensitive()
        {
            var tmp = Path.GetTempPath();
            return !Directory.Exists(tmp.ToUpper()) || !Directory.Exists(tmp.ToLower());
        }

        static public IEnumerable<string> GetFiles(string directory, bool includeSubfolders = true)
        {
            var fs = Directory.EnumerateFiles(directory);
            if (includeSubfolders)
                foreach (string d in Directory.EnumerateDirectories(directory))
                    fs = fs.Concat(GetFiles(d));
            return fs;
        }

        /// <summary>
        /// Create if does not exists.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="unique"></param>
        /// <returns></returns>
        public static string CreateDirectory(string directory, bool unique = false)
        {
            DirectoryInfo di = new DirectoryInfo(directory);
            if (!di.Exists)
                di.Create();
            else if (unique)
            {
                int i = 0;
                do
                {
                    di = new DirectoryInfo(directory + "_" + (++i));
                }
                while (di.Exists);
                di.Create();
            }
            return di.FullName;
        }

        public static void CopyDirectory(string directory1, string directory2, bool overwrite = false)
        {
            if (!Directory.Exists(directory2))
                Directory.CreateDirectory(directory2);
            foreach (string file in Directory.GetFiles(directory1))
                File.Copy(file, directory2 + Path.DirectorySeparatorChar + PathRoutines.GetFileName(file), overwrite);
            foreach (string d in Directory.GetDirectories(directory1))
                CopyDirectory(d, directory2 + Path.DirectorySeparatorChar + PathRoutines.GetDirName(d), overwrite);
        }

        public static void ClearDirectory(string directory, bool recursive = true, bool throwException = true)
        {
            try
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    return;
                }

                foreach (string file in Directory.GetFiles(directory))
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
                if (recursive)
                    foreach (string d in Directory.GetDirectories(directory))
                        DeleteDirectory(d, recursive);
            }
            catch (Exception e)
            {
                if (throwException)
                    throw;
            }
        }

        public static void DeleteDirectory(string directory, bool recursive = true)
        {
            foreach (string file in Directory.GetFiles(directory))
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }
            if (recursive)
                foreach (string d in Directory.GetDirectories(directory))
                    DeleteDirectory(d, recursive);
            Directory.Delete(directory, false);
        }

        public static bool DeleteDirectorySteadfastly(string directory, bool recursive = true)
        {
            bool error = false;
            foreach (string file in Directory.GetFiles(directory))
            {
                try
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
                catch
                {
                    error = true;
                }
            }
            if (recursive)
                foreach (string d in Directory.GetDirectories(directory))
                    DeleteDirectorySteadfastly(d, recursive);
            try
            {
                Directory.Delete(directory, false);
            }
            catch
            {
                error = true;
            }
            return !error;
        }

        /// <summary>
        /// Creates the dir if it is missing.
        /// (!)It throws an exception when the destination file exists and !overwrite.
        /// </summary>
        /// <param name="file1"></param>
        /// <param name="file2"></param>
        /// <param name="overwriteElseException"></param>
        public static void CopyFile(string file1, string file2, bool overwriteElseException = false)
        {
            CreateDirectory(PathRoutines.GetFileDir(file2), false);
            File.Copy(file1, file2, overwriteElseException);//(!)it throws an exception when the destination file exists and !overwrite
        }

        public static void MoveFile(string file1, string file2, bool overwriteElseException = true)
        {
            CreateDirectory(PathRoutines.GetFileDir(file2), false);
            if (File.Exists(file2))
            {
                if (!overwriteElseException)
                    throw new System.Exception("File " + file2 + " already exists.");
                File.Delete(file2);
            }
            File.Move(file1, file2);
        }

        public static void DeleteFile(string file, bool exceptionIfDirectoryDoesNotExist = false)
        {
            try
            {
                File.Delete(file);
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                if (exceptionIfDirectoryDoesNotExist)
                    throw;
            }
        }

        //public static void Copy(string path1, string path2, bool overwrite = false)
        //{
        //    if (Directory.Exists(path1))
        //    {
        //        CreateDirectory(path2, false);
        //        foreach (string f in Directory.GetFiles(path1))
        //        {
        //            string f2 = PathRoutines.GetPathMirroredInDir(f, path1, path2);
        //            File.Copy(f, f2, overwrite);
        //        }
        //        foreach (string d in Directory.GetDirectories(path1))
        //        {
        //            string d2 = PathRoutines.GetPathMirroredInDir(d, path1, path2);
        //            Copy(d, d2, overwrite);
        //        }
        //    }
        //    else
        //    {
        //        string f2 = PathRoutines.GetPathMirroredInDir(f, path1, path2);
        //        File.Copy(f, f2, overwrite);
        //    }
        //}

        public static bool IsFileLocked(string file)
        {
            try
            {
                using (Stream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None))
                    return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
