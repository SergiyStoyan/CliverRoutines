//********************************************************************************************
//Author: Sergey Stoyan
//        sergey.stoyan@gmail.com
//        sergey.stoyan@hotmail.com
//        stoyan@cliversoft.com
//        http://www.cliversoft.com
//********************************************************************************************

using System;
using System.IO;

namespace Cliver
{
    /// <summary>
    /// Instances of this class are to be stored in CommonApplicationData folder.
    /// CliverWinRoutines lib contains AppSettings adapted for Windows.
    /// </summary>
    public class AppSettings : Settings
    {
        /*//version with static __StorageDir
        /// <summary>
        /// (!)A Settings derivative or some of its ancestors must define this public static getter to specify the storage directory.
        /// </summary>
        new public static string __StorageDir { get; private set; } = Log.AppCompanyCommonDataDir + Path.DirectorySeparatorChar + Config.CONFIG_FOLDER_NAME; 
        */

        /// <summary>
        /// Storage folder for this Settings derivative.
        /// Each Settings derived class must have it defined. 
        /// Despite of the fact it is not static, actually it is instance independent as only the initial value is used.
        /// (It is not static because C# does not support static polymorphism.)
        /// </summary>
        sealed public override string __StorageDir { get; protected set; } = StorageDir;
        /// <summary>
        /// Storage folder for this Settings derivative.
        /// Each Settings derived class must have it defined. 
        /// </summary>
        public static readonly string StorageDir = Log.AppCompanyCommonDataDir + Path.DirectorySeparatorChar + Config.CONFIG_FOLDER_NAME;
    }

    /// <summary>
    /// Instances of this class are to be stored in LocalApplicationData folder.
    /// </summary>
    public class UserSettings : Settings
    {
        /*//version with static __StorageDir
        /// <summary>
        /// (!)A Settings derivative or some of its ancestors must define this public static getter to specify the storage directory.
        /// </summary>
        new public static string __StorageDir { get; private set; } = Log.AppCompanyUserDataDir + Path.DirectorySeparatorChar + Config.CONFIG_FOLDER_NAME;
        */

        /// <summary>
        /// Storage folder for this Settings derivative.
        /// Each Settings derived class must have it defined. 
        /// Despite of the fact it is not static, actually it is instance independent as only the initial value is used.
        /// (It is not static because C# does not support static polymorphism.)
        /// </summary>
        sealed public override string __StorageDir { get; protected set; } = StorageDir;
        /// <summary>
        /// Storage folder for this Settings derivative.
        /// Each Settings derived class must have it defined. 
        /// </summary>
        public static readonly string StorageDir = Log.AppCompanyUserDataDir + Path.DirectorySeparatorChar + Config.CONFIG_FOLDER_NAME;
    }
}