//********************************************************************************************
//Author: Sergiy Stoyan
//        s.y.stoyan@gmail.com, sergiy.stoyan@outlook.com, stoyan@cliversoft.com
//        http://www.cliversoft.com
//********************************************************************************************
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;

namespace Cliver
{
    /// <summary>
    /// Miscellaneous useful methods for file path/name construction
    /// </summary>
    public static class PathRoutines
    {
        public static bool ArePathsEqual(string path1, string path2)
        {
            var p1 = GetNormalizedPath(path1, true);
            var p2 = GetNormalizedPath(path2, true);
            return p1 == p2;
        }

        public static bool IsDirWithinDir(string dir1, string dir2)
        {
            var p1 = GetNormalizedPath(dir1, true);
            var p2 = GetNormalizedPath(dir2, true);
            string[] p1s = p1.Split(Path.DirectorySeparatorChar);
            string[] p2s = p2.Split(Path.DirectorySeparatorChar);
            if (p1s.Length < p2s.Length)
                return false;
            for (int i = 0; i < p2s.Length; i++)
                if (p1s[i] != p2s[i])
                    return false;
            return true;
        }

        public static string GetNormalizedPath(string path, bool lowerCaseIfIsCaseInsensitive)
        {
            string p = Path.GetFullPath(new Uri(path).LocalPath).TrimEnd(Path.DirectorySeparatorChar);
            if (lowerCaseIfIsCaseInsensitive && !FileSystemRoutines.IsCaseSensitive)
                return p.ToLowerInvariant();
            return p;
        }

        public static string GetAbsolutePath(string path)
        {
            if (Path.IsPathRooted(path))
                return path;
            return Log.AppDir + Path.DirectorySeparatorChar + path;
        }

        /// <summary>
        /// Convert illegal characters in the path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="webDecode"></param>
        /// <param name="illegalCharSubstitute"></param>
        /// <returns></returns>
        public static string GetLegalizedPath(string path, bool webDecode = false, string illegalCharSubstitute = "")
        {
            if (webDecode)
            {
                path = HttpUtility.HtmlDecode(path);
                path = HttpUtility.UrlDecode(path);
            }
            return Regex.Replace(path, invalidPathChars, illegalCharSubstitute);
        }
        static string invalidPathChars = "[" + Regex.Escape(new string(Path.GetInvalidPathChars())) + "]";

        /// <summary>
        /// Exctract the file name and convert illegal characters in it.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="webDecode"></param>
        /// <param name="illegalCharSubstitute"></param>
        /// <param name="treatFileAsName">DirectorySeparatorChar will be considered an InvalidFileNameChar and replaced</param>
        /// <returns></returns>
        public static string GetLegalizedFileName(string file, bool webDecode = false, string illegalCharSubstitute = "", bool treatFileAsName = false)
        {
            if (webDecode)
            {
                file = HttpUtility.HtmlDecode(file);
                file = HttpUtility.UrlDecode(file);
            }
            if (treatFileAsName)
                file = Regex.Replace(file, Regex.Escape(Path.DirectorySeparatorChar.ToString()), illegalCharSubstitute);
            return Regex.Replace(file.Substring(file.LastIndexOf(Path.DirectorySeparatorChar) + 1), invalidFileNameChars, illegalCharSubstitute);
        }
        static string invalidFileNameChars = "[" + Regex.Escape(new string(Path.GetInvalidFileNameChars())) + "]";

        /// <summary>
        /// Convert illegal characters in the directory and in the file name.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="webDecode"></param>
        /// <param name="illegalCharSubstitute"></param>
        /// <returns></returns>
        public static string GetLegalizedFile(string file, bool webDecode = false, string illegalCharSubstitute = "")
        {
            if (webDecode)
            {
                file = HttpUtility.HtmlDecode(file);
                file = HttpUtility.UrlDecode(file);
            }
            int p = file.LastIndexOf(Path.DirectorySeparatorChar) + 1;
            string path = file.Substring(0, p);
            string fileName = file.Substring(p);
            fileName = Regex.Replace(fileName, Regex.Escape(Path.DirectorySeparatorChar.ToString()), illegalCharSubstitute);
            return Regex.Replace(path, invalidPathChars, illegalCharSubstitute) + Regex.Replace(fileName, invalidFileNameChars, illegalCharSubstitute);
        }

        /// <summary>
        /// Works for any length path unlike Path.GetFileName()
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetFileName(string file)
        {
            return Regex.Replace(file, @".*" + Regex.Escape(Path.DirectorySeparatorChar.ToString()), "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        public static string GetFileNameWithoutExtension(string file)
        {
            string n = GetFileName(file);
            return Regex.Replace(n, @"\.[^\.]*$", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        public static (string Dir, string FileName, string FileExtension) GetFileParts(string file, bool fileNameEndsAtFirstDotElseLastDot = true)
        {
            Match m;
            if (fileNameEndsAtFirstDotElseLastDot)
                m = Regex.Match(file, @"(.*" + Regex.Escape(Path.DirectorySeparatorChar.ToString()) + @")?([^\.]*)(.*)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            else
                m = Regex.Match(file, @"(.*" + Regex.Escape(Path.DirectorySeparatorChar.ToString()) + @")?(.*)(\.[^\.]*)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (m.Success)
                return (m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value);
            throw new Exception("Could not parse: " + file);
        }

        /// <summary>
        /// Works for any length path unlike Path.GetFileName()
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static string GetDirName(string dir)
        {
            return Regex.Replace(dir.TrimEnd(Path.DirectorySeparatorChar), @".*" + Regex.Escape(Path.DirectorySeparatorChar.ToString()), "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        public static string InsertSuffixBeforeFileExtension(string file, string suffix, bool beforeFirstDotElseLastDot = true)
        {
            Match m;
            if (beforeFirstDotElseLastDot)
                m = Regex.Match(file, @"(.*)(\.[^" + Regex.Escape(Path.DirectorySeparatorChar.ToString()) + "]*)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            else
                m = Regex.Match(file, @"(.*)(\.[^\.]*)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (m.Success)
                return m.Groups[1].Value + suffix + m.Groups[2].Value;
            return file + suffix;
        }

        public static string AddPrefix2FileName(string file, string prefix)
        {
            Match m = Regex.Match(file, @"(.*" + Regex.Escape(Path.DirectorySeparatorChar.ToString()) + ")(.*)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (m.Success)
                return m.Groups[1].Value + prefix + m.Groups[2].Value;
            return prefix + file;
        }

        public static string AddPrefixSuffix2FileName(string file, string prefix, string suffix, bool fileNameEndsAtFirstDotElseLastDot = true)
        {
            Match m;
            if (fileNameEndsAtFirstDotElseLastDot)
                m = Regex.Match(file, @"(.*" + Regex.Escape(Path.DirectorySeparatorChar.ToString()) + @")([^\.]*)(.*)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            else
                m = Regex.Match(file, @"(.*" + Regex.Escape(Path.DirectorySeparatorChar.ToString()) + @")(.*)(\.[^\.]*)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (m.Success)
                return m.Groups[1].Value + prefix + m.Groups[2].Value + suffix + m.Groups[3].Value;
            return prefix + file + suffix;
        }

        /// <summary>
        /// Works for any length path unlike Path.GetFileName()
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetFileExtension(string file)
        {
            return Regex.Replace(file, @".*\.", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        /// <summary>
        /// Get the parent dir of a file or dir.
        /// Works for any length path unlike Path.GetDir().
        /// </summary>
        /// <param name="file"></param>
        /// <param name="removeTrailingSeparator"></param>
        /// <returns></returns>        
        public static string GetFileDir(string file, bool removeTrailingSeparator = true)
        {
            string fd = Regex.Replace(file, @"[^" + Regex.Escape(Path.DirectorySeparatorChar.ToString()) + @"]*$", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (removeTrailingSeparator)
                fd = fd.TrimEnd(Path.DirectorySeparatorChar);
            return fd;
        }

        public static string ReplaceFileExtension(string file, string extension)
        {
            return Regex.Replace(file, @"\.[^\.]+$", "." + extension, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        public static string GetPathMirroredInDir(string path, string rootDir, string mirrorDir)
        {
            string p = GetNormalizedPath(path, false);
            string rd = GetNormalizedPath(rootDir, false);
            string md = GetNormalizedPath(mirrorDir, false);
            return Regex.Replace(p, @"^\s*" + Regex.Escape(rd), md);
        }

        public static string GetRelativePath(string path, string baseDir)
        {
            string p = GetNormalizedPath(path, false);
            string bd = GetNormalizedPath(baseDir, false);
            return Regex.Replace(p, @"^\s*" + Regex.Escape(bd), "");
        }
    }
}