﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CryptoUtility
{
    /// <summary>
    /// Usage:  string executablePath = FileAssociation.GetExecFileAssociatedToExtension(pathExtension, "open");
    /// </summary>
    public static class FileAssociation
    {
        [Flags]
        public enum AssocF
        {
            Init_NoRemapCLSID = 0x1,
            Init_ByExeName = 0x2,
            Open_ByExeName = 0x2,
            Init_DefaultToStar = 0x4,
            Init_DefaultToFolder = 0x8,
            NoUserSettings = 0x10,
            NoTruncate = 0x20,
            Verify = 0x40,
            RemapRunDll = 0x80,
            NoFixUps = 0x100,
            IgnoreBaseClass = 0x200
        }

        public enum AssocStr
        {
            Command = 1,
            Executable,
            FriendlyDocName,
            FriendlyAppName,
            NoOpen,
            ShellNewValue,
            DDECommand,
            DDEIfExec,
            DDEApplication,
            DDETopic
        }

        [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern uint AssocQueryStringW(AssocF flags, AssocStr str, string pszAssoc, string pszExtra, [Out] StringBuilder pszOut, [In][Out] ref uint pcchOut);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="verb"></param>
        /// <returns>Return null if not found</returns>
        public static string GetExecFileAssociatedToExtension(string ext, string verb = null)
        {
            if (ext[0] != '.')
            {
                ext = "." + ext;
            }
  
            string executablePath = FileExtentionInfo(AssocStr.Executable, ext, verb); // Will only work for 'open' verb
            if (string.IsNullOrEmpty(executablePath))
            {
                executablePath = FileExtentionInfo(AssocStr.Command, ext, verb); // required to find command of any other verb than 'open'

                // Extract only the path
                if (!string.IsNullOrEmpty(executablePath) && executablePath.Length > 1)
                {
                    if (executablePath[0] == '"')
                    {
                        executablePath = executablePath.Split('\"')[1];
                    }
                    else if (executablePath[0] == '\'')
                    {
                        executablePath = executablePath.Split('\'')[1];
                    }
                }
            }

            // Ensure to not return the default OpenWith.exe associated executable in Windows 8 or higher
            if (!string.IsNullOrEmpty(executablePath) && File.Exists(executablePath) &&  !executablePath.ToLower().EndsWith(".dll"))
            {
                if (executablePath.ToLower().EndsWith("openwith.exe"))
                {
                    return null; // 'OpenWith.exe' is th windows 8 or higher default for unknown extensions. I don't want to have it as associted file
                }
                return executablePath;
            }
            return executablePath;
        }

        private static string FileExtentionInfo(AssocStr assocStr, string doctype, string verb)
        {
            AssocF flags = AssocF.Verify | AssocF.NoTruncate;

            const uint E_POINTER = 0x80004003;
            const uint E_NO_ASSOCIATION = 0x80070483;

            var builder = new StringBuilder(512);
            uint size = (uint) builder.Length;

            var result = AssocQueryStringW(flags, assocStr, doctype, verb, builder, ref size);

            if (result == E_POINTER) 
            {
                builder.Length = (int)size;
                result = AssocQueryStringW(flags, assocStr, doctype, verb, builder, ref size);
            }

            if (result == E_NO_ASSOCIATION)
            {
                return null;
            }

            Marshal.ThrowExceptionForHR((int)result);
            return builder.ToString();
        }



    }
}