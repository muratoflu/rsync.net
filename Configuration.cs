/**
 *  Copyright (C) 2006 Alex Pedenko
 * 
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace NetSync
{
    /// <summary>
    /// 
    /// </summary>
    public class Configuration
    {
        private string _confFile;

        public List<Module> Modules;
        public string LogFile;
        public string Port;
        public string Address;

        /// <summary>
        /// Creates new instance and sets confFile to system.directory + cFile
        /// </summary>
        /// <param name="cFile"></param>
        public Configuration(string cFile)
        {
            _confFile = Path.Combine(Environment.SystemDirectory, Path.GetFileName(cFile));
        }

        /// <summary>
        /// Finds number of module using its name
        /// </summary>
        /// <param name="nameModule"></param>
        /// <returns></returns>
        public int GetNumberModule(string nameModule)
        {
            lock (this)
            {
                for (var i = 0; i < Modules.Count; i++)
                {
                    if (Modules[i].Name == nameModule)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Get Module by its number from list od Modules
        /// </summary>
        /// <param name="numberModule"></param>
        /// <returns></returns>
        public Module GetModule(int numberModule)
        {
            lock (this)
            {
                if (numberModule < 0 || numberModule > Modules.Count)
                {
                    return null;
                }
                return Modules[numberModule];
            }
        }

        /// <summary>
        /// Finds name of module using its number
        /// </summary>
        /// <param name="numberModule"></param>
        /// <returns></returns>
        public string GetModuleName(int numberModule)
        {
            lock (this)
            {
                return GetModule(numberModule).Name;
            }
        }

        /// <summary>
        /// Checks whether Module, located at given number, is ReadOnly
        /// </summary>
        /// <param name="numberModule"></param>
        /// <returns></returns>
        public bool ModuleIsReadOnly(int numberModule)
        {
            lock (this)
            {
                return GetModule(numberModule).ReadOnly; //@todo GetModule may return null and thus nullref exception
            }
        }

        /// <summary>
        /// Checks whether Module, located at given number, is WriteOnly
        /// </summary>
        /// <param name="numberModule"></param>
        /// <returns></returns>
        public bool ModuleIsWriteOnly(int numberModule)
        {
            lock (this)
            {
                return GetModule(numberModule).WriteOnly; //@todo GetModule may return null and thus nullref exception
            }
        }

        /// <summary>
        /// Gets string, which contains allowed hosts for Module, located at given number
        /// </summary>
        /// <param name="numberModule"></param>
        /// <returns></returns>
        public string GetHostsAllow(int numberModule)
        {
            lock (this)
            {
                return GetModule(numberModule).HostsAllow; //@todo GetModule may return null and thus nullref exception
            }
        }

        /// <summary>
        /// Gets string, which contains denied hosts for Module, located at given number
        /// </summary>
        /// <param name="numberModule"></param>
        /// <returns></returns>
        public string GetHostsDeny(int numberModule)
        {
            lock (this)
            {
                return GetModule(numberModule).HostsDeny; //@todo GetModule may return null and thus nullref exception
            }
        }

        /// <summary>
        /// Gets string of allowed users for Module, located at given number
        /// </summary>
        /// <param name="numberModule"></param>
        /// <returns></returns>
        public string GetAuthUsers(int numberModule)
        {
            lock (this)
            {
                return GetModule(numberModule).AuthUsers; //@todo GetModule may return null and thus nullref exception
            }
        }

        /// <summary>
        /// Get name of secret file for Module, located at given number
        /// </summary>
        /// <param name="numberModule"></param>
        /// <returns></returns>
        public string GetSecretsFile(int numberModule)
        {
            lock (this)
            {
                return GetModule(numberModule).SecretsFile; //@todo GetModule may return null and thus nullref exception
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public bool LoadParm(Options options)
        {
            lock (this)
            {
                // TODO: path length
                if (!File.Exists(_confFile))
                {
                    WinRsync.Exit("Can't find .conf file: " + _confFile, null);
                    return false;
                }
                try
                {
                    using (var cf = new StreamReader(_confFile))
                    {
                        Module mod = null;

                        if (Modules == null)
                        {
                            Modules = new List<Module>();
                        }

                        lock (cf)
                        {
                            while (true)
                            {
                                var line = cf.ReadLine();
                                if (line == null)
                                {
                                    break;
                                }
                                line = line.Trim();
                                if (!line.IsBlank() && !line.StartsWith(";") && !line.StartsWith("#"))
                                {
                                    if (line.StartsWith("[") && line.StartsWith("]"))
                                    {
                                        line = line.TrimStart('[').TrimEnd(']');
                                        var numberModule = -1;
                                        if ((numberModule = GetNumberModule(line)) >= 0)
                                        {
                                            mod = GetModule(numberModule);
                                        }
                                        else
                                        {
                                            mod = new Module(line);
                                            Modules.Add(mod);
                                        }
                                    }
                                    else
                                    {
                                        if (mod != null)
                                        {
                                            var parm = line.Split('=');
                                            if (parm.Length > 2)
                                            {
                                                continue;
                                            }
                                            parm[0] = parm[0].Trim().ToLower();
                                            parm[1] = parm[1].Trim();
                                            switch (parm[0])
                                            {
                                                case "path":
                                                    mod.Path = parm[1].Replace(@"\", "/");
                                                    break;
                                                case "comment":
                                                    mod.Comment = parm[1];
                                                    break;
                                                case "read only":
                                                    mod.ReadOnly = (parm[1].CompareTo("false") == 0) ? false : true;
                                                    break;
                                                case "write only":
                                                    mod.WriteOnly = (parm[1].CompareTo("true") == 0) ? true : false;
                                                    break;
                                                case "hosts allow":
                                                    mod.HostsAllow = parm[1];
                                                    break;
                                                case "hosts deny":
                                                    mod.HostsDeny = parm[1];
                                                    break;
                                                case "auth users":
                                                    mod.AuthUsers = parm[1];
                                                    break;
                                                case "secrets file":
                                                    mod.SecretsFile = Path.GetFileName(parm[1]);
                                                    break;
                                                default:
                                                    continue;
                                            }
                                        }
                                        else
                                        {
                                            var parm = line.Split('=');
                                            if (parm.Length > 2)
                                            {
                                                continue;
                                            }
                                            parm[0] = parm[0].Trim();
                                            parm[1] = parm[1].Trim();
                                            switch (parm[0])
                                            {
                                                case "log file":
                                                    var logFile = parm[1];
                                                    try
                                                    {
                                                        options.LogFile = new FileStream(logFile, FileMode.OpenOrCreate | FileMode.Append, FileAccess.Write);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        Log.Write(e.Message);
                                                    }
                                                    break;
                                                case "port":
                                                    Port = parm[1];
                                                    options.RsyncPort = Convert.ToInt32(Port);
                                                    break;
                                                case "address":
                                                    options.BindAddress = Address = parm[1];
                                                    break;
                                                default:
                                                    continue;
                                            }
                                        }
                                    }
                                }
                            }
                            cf.Close();
                        }
                    }
                }
                catch
                {
                    WinRsync.Exit("failed to open: " + _confFile, null);
                    return false;
                }
            }
            return true;
        }
    }

    public class Module
    {
        public string Name;
        public string Path = String.Empty;
        public string Comment = String.Empty;
        public bool ReadOnly = true;
        public bool WriteOnly;
        public string HostsAllow = String.Empty;
        public string HostsDeny = String.Empty;
        public string AuthUsers = String.Empty;
        public string SecretsFile = String.Empty;

        public Module(string name)
        {
            Name = name;
        }
    }
}
