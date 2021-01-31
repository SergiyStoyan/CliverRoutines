//********************************************************************************************
//Author: Sergey Stoyan
//        sergey.stoyan@gmail.com
//        sergey.stoyan@hotmail.com
//        stoyan@cliversoft.com
//        http://www.cliversoft.com
//********************************************************************************************

using System;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace Cliver
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class Log
    {

        static Log()
        {
            /*if (ProgramRoutines.IsWebContext) - !!!crashes on Xamarin!!!
                throw new Exception("Log is disabled in web context.");

            if (ProgramRoutines.IsWebContext)
                ProcessName = System.Web.Compilation.BuildManager.GetGlobalAsaxType().BaseType.Assembly.GetName(false).Name;
            else*/
            /*!!!it was tested on XamarinMAC, NT service
            entryAssembly = Assembly.GetEntryAssembly();
            //!!!when using WCF it happened that GetEntryAssembly() is NULL 
            if (entryAssembly == null)
                entryAssembly = Assembly.GetCallingAssembly();
            ProcessName = entryAssembly.GetName(false).Name;

            AssemblyRoutines.AssemblyInfo ai = new AssemblyRoutines.AssemblyInfo(entryAssembly);
            CompanyName = string.IsNullOrWhiteSpace(ai.Company) ? "CliverSoft" : ai.Company;

            AppDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
            */

            Process p = Process.GetCurrentProcess();
            ProcessName = p.ProcessName;
            AppDir = PathRoutines.GetFileDir(p.MainModule.FileName);
            CompanyName = FileVersionInfo.GetVersionInfo(p.MainModule.FileName)?.CompanyName;

            //!!!No write permission on macOS
            CompanyCommonDataDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + Path.DirectorySeparatorChar + CompanyName;
            //!!!No write permission on macOS
            AppCompanyCommonDataDir = CompanyCommonDataDir + Path.DirectorySeparatorChar + ProcessName;
            CompanyUserDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Path.DirectorySeparatorChar + CompanyName;
            AppCompanyUserDataDir = CompanyUserDataDir + Path.DirectorySeparatorChar + ProcessName;
        }

        /// <summary>
        /// Normalized name of this process.
        /// </summary>
        public static readonly string ProcessName;

        /// <summary>
        /// Company name of the executing file.
        /// </summary>
        public static readonly string CompanyName;
        ///// <summary>
        ///// If altering it, do it at the very beginning.
        ///// </summary>
        //public static string CompanyName
        //{
        //    get
        //    {
        //        if (string.IsNullOrWhiteSpace(companyName))
        //        {
        //            Assembly entryAssembly = Assembly.GetEntryAssembly();
        //            if (entryAssembly == null)//!!!when using WCF it happened that GetEntryAssembly() is NULL 
        //                entryAssembly = Assembly.GetCallingAssembly();
        //            AssemblyRoutines.AssemblyInfo ai = new AssemblyRoutines.AssemblyInfo(entryAssembly);
        //            companyName = string.IsNullOrWhiteSpace(ai.Company) ? "CliverSoft" : ai.Company;
        //        }
        //        return companyName;
        //    }
        //    set
        //    {
        //        companyName = value;
        //    }
        //}
        //static string companyName;

        /// <summary>
        /// User-independent company data directory.
        /// (!)No write permission on macOS
        /// </summary>
        public static readonly string CompanyCommonDataDir;

        /// <summary>
        /// User-independent company-application data directory.
        /// (!)No write permission on macOS
        /// </summary>
        public static readonly string AppCompanyCommonDataDir;

        /// <summary>
        /// User-dependent company data directory.
        /// </summary>
        public static readonly string CompanyUserDataDir;

        /// <summary>
        /// User-dependent company-application data directory.
        /// </summary>
        public static readonly string AppCompanyUserDataDir;

        /// <summary>
        /// Directory where the application binary is located.
        /// </summary>
        public readonly static string AppDir;
    }
}

