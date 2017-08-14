/* ========================================================================
 * Copyright (c) 2005-2017 The OPC Foundation, Inc. All rights reserved.
 *
 * OPC Foundation MIT License 1.00
 * 
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 *
 * The complete license agreement can be found here:
 * http://opcfoundation.org/License/MIT/1.00/
 * ======================================================================*/

using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceProcess;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;
using System.Windows.Forms;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

using Opc.Ua;

namespace Opc.Ua.Configuration
{
    /// <summary>
    /// һ�����ڰ�װ�����ã�����opc Ua�ķ�װ��
    /// </summary>
    public class ApplicationInstance
    {
        #region ���췽��
        /// <summary>
        /// ʵ����һ���µ�<see cref="ApplicationInstance"/>����
        /// </summary>
        public ApplicationInstance()
        {

        }

        /// <summary>
        /// ʹ��ָ��������ʵ����һ���µ�<see cref="ApplicationInstance"/>����
        /// </summary>
        /// <param name="applicationConfiguration">ָ����opc uaӦ�ó�������</param>
        public ApplicationInstance(ApplicationConfiguration applicationConfiguration)
        {
            m_applicationConfiguration = applicationConfiguration;
        }
        #endregion

        #region ����������
        /// <summary>
        /// ��ȡ������Ӧ�ó��������
        /// </summary>
        /// <value>The name of the application.</value>
        public string ApplicationName
        {
            get { return m_applicationName; }
            set { m_applicationName = value; }
        }

        /// <summary>
        /// ��ȡ������Ӧ�ó�������ͣ���Ҫ�ַ��������ͻ��ˣ�������ͣ����ҷ���������
        /// </summary>
        /// <value>The type of the application.</value>
        public ApplicationType ApplicationType
        {
            get { return m_applicationType; }
            set { m_applicationType = value; }
        }

        //Gets or sets the name of the config section containing the path to the application configuration file.
        /// <summary>
        /// ��ȡ������һ������Ӧ�ó��������ļ�·���Ĳ������õ�����
        /// </summary>
        /// <value>The name of the config section.</value>
        public string ConfigSectionName
        {
            get { return m_configSectionName; }
            set { m_configSectionName = value; }
        }

        //Gets or sets the type of configuration file.
        /// <summary>
        /// ��ȡ�����������ļ�������
        /// </summary>
        /// <value>The type of configuration file.</value>
        public Type ConfigurationType
        {
            get { return m_configurationType; }
            set { m_configurationType = value; }
        }

        //Gets or sets the installation configuration.
        /// <summary>
        /// ��ȡ�����ð�װ������Ϣ
        /// </summary>
        /// <value>The installation configuration.</value>
        public InstalledApplication InstallConfig
        {
            get { return m_installConfig; }
            set { m_installConfig = value; }
        }

        /// <summary>
        /// ��ȡ��������������
        /// </summary>
        /// <value>The server.</value>
        public ServerBase Server
        {
            get { return m_server; }
        }

        //Gets the application configuration used when the Start() method was called.
        /// <summary>
        /// ��ȡ������Ӧ�ó�������ö��󣬸ö�����Start()��������ʱʹ��
        /// </summary>
        /// <value>The application configuration.</value>
        public ApplicationConfiguration ApplicationConfiguration
        {
            get { return m_applicationConfiguration; }
            set { m_applicationConfiguration = value; }
        }

        // Gets or sets a flag that indicates whether the application will be set up for management with the GDS agent.
        // If true the application will not be visible to the GDS local agent after installation.
        /// <summary>
        /// ��ȡ������һ����־��ָʾ�Ƿ�Ӧ�ó�������Ϊʹ��GDS������й���
        /// </summary>
        /// <value>���Ϊtrue����װ��Ӧ�ó��򽫲�����ʾ��GDS���ش���</value>
        public bool NoGdsAgentAdmin { get; set; }
        #endregion

        #region ����������Ϣ
        /// <summary>
        /// ��һ���ļ����ذ�װ����
        /// </summary>
        /// <param name="filePath">�ļ���·��</param>
        /// <exception cref="ArgumentNullException"></exception>
        public InstalledApplication LoadInstallConfigFromFile(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException("filePath");
            }

            Stream istrm = null;

            try
            {
                istrm = File.Open(filePath, FileMode.Open, FileAccess.Read);
            }
            catch (Exception e)
            {
                throw ServiceResultException.Create(StatusCodes.BadDecodingError, e, "Could not open file: {0}", filePath);
            }

            return LoadInstallConfigFromStream(istrm);
        }

        /// <summary>
        /// ��һ����Ƕ����Դ�ļ��м��س���İ�װ����
        /// </summary>
        /// <param name="resourcePath">��Դ����</param>
        /// <param name="assembly">����</param>
        /// <exception cref="ArgumentNullException"></exception>
        public InstalledApplication LoadInstallConfigFromResource(string resourcePath, Assembly assembly)
        {
            if (resourcePath == null) throw new ArgumentNullException("resourcePath");

            if (assembly == null)
            {
                assembly = Assembly.GetCallingAssembly();
            }

            Stream istrm = assembly.GetManifestResourceStream(resourcePath);

            if (istrm == null)
            {
                throw ServiceResultException.Create(StatusCodes.BadDecodingError, "Could not find resource file: {0}", resourcePath);
            }

            return LoadInstallConfigFromStream(istrm);
        }

        /// <summary>
        /// ʹ�÷����л���һ���������м��س���İ�װ����
        /// </summary>
        /// <param name="istrm">������</param>
        /// <exception cref="SerializationException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public InstalledApplication LoadInstallConfigFromStream(Stream istrm)
        {
            try
            {
                using (XmlTextReader reader = new XmlTextReader(istrm))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(InstalledApplication));
                    return (InstalledApplication)serializer.ReadObject(reader, false);
                }
            }
            catch (Exception e)
            {
                throw ServiceResultException.Create(StatusCodes.BadDecodingError, e, "Could not parse install configuration.");
            }
        }

        /// <summary>
        /// �������ļ��м���Ӧ�ó���İ�װ����
        /// </summary>
        /// <param name="configFile">һ������Ϊ�յ��ļ���������</param>
        public virtual void LoadInstallConfig(string configFile)
        {
            // ���ļ��м�������
            if (!String.IsNullOrEmpty(configFile))
            {
                InstallConfig = LoadInstallConfigFromFile(configFile);
            }

            // �����г����������Դ��Ѱ��InstallConfig.xml�ļ���������ھͼ��ذ�װ���ã���������ھ��˳�
            else if (InstallConfig == null)
            {
                foreach (string resourcePath in Assembly.GetEntryAssembly().GetManifestResourceNames())
                {
                    if (resourcePath.EndsWith("InstallConfig.xml"))
                    {
                        InstallConfig = LoadInstallConfigFromResource(resourcePath, Assembly.GetEntryAssembly());
                        break;
                    }
                }

                if (InstallConfig == null)
                {
                    throw new ServiceResultException(StatusCodes.BadConfigurationError, "Could not load default installation config file.");
                }
            }

            // �����װ�����а���Ӧ�ó�������֣��͸��ǵ���ǰ�����Ӧ�ó�������
            if (String.IsNullOrEmpty(InstallConfig.ApplicationName))
            {
                InstallConfig.ApplicationName = ApplicationName;
            }
            else
            {
                ApplicationName = InstallConfig.ApplicationName;
            }
            
            // ���°�װ�����еĹ̶��ֶΣ������������ͺͿ�ִ��exe���ļ�����
            InstallConfig.ApplicationType = (Opc.Ua.Security.ApplicationType)(int)ApplicationType;
            InstallConfig.ExecutableFile = Application.ExecutablePath;
            //�����װ������������׷��������Ϣ�ͽ���������Ӧ���ڵ�ǰӦ�ó���
            if (InstallConfig.TraceConfiguration != null)
            {
                InstallConfig.TraceConfiguration.ApplySettings();
            }
        }
        #endregion
        
        #region �����ķ���
        /// <summary>
        /// ����������
        /// </summary>
        /// <returns>
        /// ��������Ѿ�����ͷ���True�����򷵻�False
        /// </returns>
        public bool ProcessCommandLine()
        {
            //ɾ����GDS��ع��ܣ��µ�OPCF����������GDS
            // NP Jan-20-2012: removing GDS reference, per new OPCF decree of no GDS.
            NoGdsAgentAdmin = true;
            return ProcessCommandLine(false);
        }

        /// <summary>
        /// ����������
        /// </summary>
        /// <param name="ignoreUnknownArguments">�������Ϊ<c>true</c>��������δ֪����</param>
        /// <returns>
        /// ��������Ѿ�����ͷ���True�����򷵻�False
        /// </returns>
        public bool ProcessCommandLine(bool ignoreUnknownArguments)
        {
            // ����һ��׷�����ö���
            TraceConfiguration config = new TraceConfiguration();
            config.OutputFilePath = "%CommonApplicationData%\\OPC Foundation\\Logs\\Default.InstallLog.txt";
            config.DeleteOnLoad = false;
            config.TraceMasks = 1023;
            config.ApplySettings();

            string[] args =  Environment.GetCommandLineArgs();

            if (args.Length <= 1)
            {
                return false;
            }

            return ProcessCommandLine(ignoreUnknownArguments, args);
        }

        /// <summary>
        /// ����������
        /// </summary>
        /// <returns>��������Ѿ�����ͷ���True�����򷵻�False</returns>
        public bool ProcessCommandLine(bool ignoreUnknownArguments, params string[] args)
        {
            if (args.Length <= 1)
            {
                return false;
            }

            // ���������Ƕ����Ļ�����ֵ���ɡ�:���ָ���
            Dictionary<string,string> argTable = new Dictionary<string, string>();

            for (int ii = 1; ii < args.Length; ii++)
            {
                string arg = args[ii];

                if (String.IsNullOrEmpty(arg))
                {
                    continue;
                }

                int index = args[ii].IndexOf(':');

                if (index != -1 && index > 0 && index < arg.Length-1)
                {
                    argTable[arg.Substring(0, index).ToLower()] = arg.Substring(index+1);
                }
                else
                {
                    argTable[arg.ToLower()] = String.Empty;
                }
            }

            // ��֤������Ϣ
            string error = ValidateArguments(ignoreUnknownArguments, argTable);

            if (!String.IsNullOrEmpty(error))
            {
                throw ServiceResultException.Create(StatusCodes.BadInvalidArgument, error);
            }

            // ���һ��Ĭ�ϵĿ��أ�һ����˵silent=False
            bool silent = !Environment.UserInteractive;

            if (argTable.ContainsKey("/silent"))
            {
                silent = true;
            }

            string configFile = null;

            try
            {
                // �������л�ȡ�����ļ�����Ϣ
                if (argTable.TryGetValue("/configfile", out configFile))
                {
                    configFile = Utils.GetAbsoluteFilePath(configFile, true, true, false);
                }

                // ���ذ�װ������Ϣ
                LoadInstallConfig(configFile);
            }
            catch (Exception e)
            {
                StringBuilder buffer = new StringBuilder();
                buffer.Append("Could not load the install configuration. ");
                buffer.Append(configFile);

                if (!silent)
                {
                    throw ServiceResultException.Create(StatusCodes.BadInvalidArgument, e, buffer.ToString());
                }

                Utils.Trace(e, buffer.ToString());
                return true;
            }
                
            try
            {
                // ��װӦ�ó���
                if (argTable.ContainsKey("/start"))
                {
                    if (ServiceInstaller.StartService(InstallConfig.ApplicationName))
                    {
                        Utils.Trace(Utils.TraceMasks.Information, "Service started '{0}'.", InstallConfig.ApplicationName);
                    }

                    return true;
                }

                // ��װӦ�ó���
                if (argTable.ContainsKey("/install"))
                {
                    Install(silent, argTable);
                    return true;
                }

                // ж��Ӧ�ó���
                if (argTable.ContainsKey("/uninstall"))
                {
                    Uninstall(silent, argTable);
                    return true;
                }

                // ���������ඨ����κβ�����
                return ProcessCommand(silent, argTable);
            }
            catch (Exception e)
            {
                StringBuilder buffer = new StringBuilder();
                buffer.Append("Could not process the command line arguments provided. ");

                if (args != null)
                {
                    for (int ii = 1; ii < args.Length; ii++)
                    {
                        buffer.AppendFormat("{0} ", args[ii]);
                    }
                }

                if (!silent)
                {
                    throw ServiceResultException.Create(StatusCodes.BadInvalidArgument, e, buffer.ToString());
                }

                Utils.Trace(e, buffer.ToString());
                return true;
            }
        }

        /// <summary>
        /// ��OPC UA������Ϊһ��Windows Service����
        /// </summary>
        /// <param name="server">�����������</param>
        public void StartAsService(ServerBase server)
        {
            m_server = server;
            ServiceBase.Run(new WindowsService(server, ConfigSectionName, ApplicationType, ConfigurationType));
        }

        /// <summary>
        /// ����OPC UA����
        /// </summary>
        /// <param name="server">�����������</param>
        public void Start(ServerBase server)
        {
            m_server = server;

            if (m_applicationConfiguration == null)
            {
                LoadApplicationConfiguration(false);
            }

            if (m_applicationConfiguration.SecurityConfiguration != null && m_applicationConfiguration.SecurityConfiguration.AutoAcceptUntrustedCertificates)
            {
                m_applicationConfiguration.CertificateValidator.CertificateValidation += CertificateValidator_CertificateValidation;
            }
            
            server.Start(m_applicationConfiguration);
        }

        /// <summary>
        /// Stops the UA server.
        /// </summary>
        public void Stop()
        {
            m_server.Stop();
        }
        #endregion

        #region WindowsService ��
        /// <summary>
        /// ����OPC UA�����windows ���������֮ǰ�Ľ���
        /// </summary>
        protected class WindowsService : ServiceBase
        {
            #region ������
            /// <summary>
            ///ʵ����һ�� <see cref="WindowsService"/> ����
            /// </summary>
            /// <param name="server">�����������</param>
            /// <param name="configSectionName">Ƭ����������</param>
            /// <param name="applicationType">Ӧ�ó��������</param>
            /// <param name="configurationType">�������������</param>
            public WindowsService(ServerBase server, string configSectionName, ApplicationType applicationType, Type configurationType)
            {
                m_server = server;
                m_configSectionName = configSectionName;
                m_applicationType = applicationType;
                m_configurationType = configurationType;
                EventLog.Source = "UA Application";
            }
            #endregion
            
            #region ��д�ķ���
            /// <summary>
            /// ��һ����̨���߳�����������
            /// </summary>
            protected override void OnStart(string[] args)
            {
                Thread thread = new Thread(OnBackgroundStart);
                thread.Start(null);
            }

            /// <summary>
            /// ֹͣ�������Ա��÷����ս�
            /// </summary>
            protected override void OnStop()
            {
                m_server.Stop();
            }
            #endregion
            
            #region ˽�еķ���
            /// <summary>
            /// һ��ʵ���ں�̨���з���ķ���
            /// </summary>
            private void OnBackgroundStart(object state)
            {
                string filePath = null;
                ApplicationConfiguration configuration = null;

                try
                {
                    filePath = ApplicationConfiguration.GetFilePathFromAppConfig(m_configSectionName);
                    configuration = ApplicationInstance.LoadAppConfig(false, filePath, m_applicationType, m_configurationType, true);
                }
                catch (Exception e)
                {
                    ServiceResult error = ServiceResult.Create(e, StatusCodes.BadConfigurationError, "Could not load UA Service configuration file.\r\nPATH={0}", filePath);
                    this.EventLog.WriteEntry(error.ToLongString(), EventLogEntryType.Error);
                }

                try
                {
                    if (configuration.SecurityConfiguration != null && configuration.SecurityConfiguration.AutoAcceptUntrustedCertificates)                   
                    {
                        configuration.CertificateValidator.CertificateValidation += CertificateValidator_CertificateValidation;
                    }

                    m_server.Start(configuration);
                    // this.EventLog.WriteEntry("SERVICE STARTED! " + this.m_configSectionName, EventLogEntryType.Information);
                }
                catch (Exception e)
                {
                    ServiceResult error = ServiceResult.Create(e, StatusCodes.BadConfigurationError, "Could not start UA Service.");
                    this.EventLog.WriteEntry(error.ToLongString(), EventLogEntryType.Error);
                    Utils.Trace((int)Utils.TraceMasks.Error, error.ToLongString());
                }
            }

            #endregion

            #region Private Fields
            private ServerBase m_server;
            private string m_configSectionName;
            private ApplicationType m_applicationType;
            private Type m_configurationType;
            #endregion
        }
        #endregion

        #region ����������
        /// <summary>
        /// �洢����������
        /// </summary>
        protected class ArgumentDescription
        {
            /// <summary>
            /// ����������
            /// </summary>
            public string Name;

            /// <summary>
            /// ����������
            /// </summary>
            public string Description;

            /// <summary>
            /// ָʾ�����Ƿ���Ҫһ��ֵ
            /// </summary>
            public bool ValueRequired;

            /// <summary>
            /// ָʾ�����Ƿ�����һ��ֵ
            /// </summary>
            public bool ValueAllowed;

            /// <summary>
            /// ʵ����һ�� <see cref="ArgumentDescription"/> ����.
            /// </summary>
            /// <param name="name">��������</param>
            /// <param name="valueRequired">�������Ϊ <c>true</c> ��������Ҫһ��ֵ</param>
            /// <param name="valueAllowed">�������Ϊ <c>true</c> ��������Ҫһ��ֵ</param>
            /// <param name="description">����������</param>
            public ArgumentDescription(
                 string name,
                 bool valueRequired,
                 bool valueAllowed,
                 string description)
            {
                Name = name;
                ValueRequired = valueRequired;
                ValueAllowed = valueAllowed;
                Description = description;
            }
        }

        private static ArgumentDescription[] s_SupportedArguments = new ArgumentDescription[]
        {            
            new ArgumentDescription("/start", false, false, "Starts the application as a service (/start [/silent] [/configFile:<filepath>])."),
            new ArgumentDescription("/install", false, false, "Installs the application (/install [/silent] [/configFile:<filepath>])."),
            new ArgumentDescription("/uninstall", false, false, "Uninstalls the application (/uninstall [/silent] [/configFile:<filepath>])."),
            new ArgumentDescription("/silent", false, false, "Performs operations without prompting user to confirm or displaying errors."),
            new ArgumentDescription("/configFile", true, true, "Specifies the installation configuration file."),
        };
        #endregion

        #region �ܱ����ķ���
        /// <summary>
        /// ��ȡ֧�ֵĲ���������
        /// </summary>
        protected virtual ArgumentDescription[] GetArgumentDescriptions()
        {
            return s_SupportedArguments;
        }

        /// <summary>
        /// ��ȡ�����ı�
        /// </summary>
        protected virtual string GetHelpString(ArgumentDescription[] commands)
        {
            StringBuilder text = new StringBuilder();
            text.Append("These are the supported arguments:\r\n");

            for (int ii = 0; ii < commands.Length; ii++)
            {
                ArgumentDescription command = commands[ii];

                text.Append("\r\n");

                if (command.ValueRequired)
                {
                    text.AppendFormat("{0}:<value> {1}", command.Name, command.Description);
                }
                else if (command.ValueAllowed)
                {
                    text.AppendFormat("{0}[:<value>] {1}", command.Name, command.Description);
                }
                else
                {
                    text.AppendFormat("{0} {1}", command.Name, command.Description);
                }
            }

            text.Append("\r\n");
            return text.ToString();
        }

        /// <summary>
        /// ��֤����
        /// </summary>
        protected virtual string ValidateArguments(bool ignoreUnknownArguments, Dictionary<string, string> args)
        {
            ArgumentDescription[] commands = GetArgumentDescriptions();

            // ����Ƿ�Ҫ�����
            if (args.ContainsKey("/?"))
            {
                return GetHelpString(commands);
            }

            // ��֤����
            StringBuilder error = new StringBuilder();

            foreach (KeyValuePair<string,string> arg in args)
            {
                ArgumentDescription command = null;

                for (int ii = 0; ii < commands.Length; ii++)
                {
                    if (String.Compare(commands[ii].Name, arg.Key, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        command = commands[ii];
                        break;
                    }
                }

                if (command == null)
                {
                    if (!ignoreUnknownArguments)
                    {
                        if (error.Length > 0)
                        {
                            error.Append("\r\n");
                        }

                        error.AppendFormat("Unrecognized argument: {0}", arg.Key);
                    }

                    continue;
                }

                if (command.ValueRequired && String.IsNullOrEmpty(arg.Value))
                {
                    if (error.Length > 0)
                    {
                        error.Append("\r\n");
                    }

                    error.AppendFormat("{0} requires a value to be specified (syntax {0}:<value>).", arg.Key);
                    continue;
                }

                if (!command.ValueAllowed && !String.IsNullOrEmpty(arg.Value))
                {
                    if (error.Length > 0)
                    {
                        error.Append("\r\n");
                    }

                    error.AppendFormat("{0} does not allow a value to be specified.", arg.Key);
                    continue;
                }
            }

            // return any error text.
            return error.ToString();
        }

        /// <summary>
        /// ���ݰ�װ�����е���Ϣ������Ӧ�ó������õ�һЩֵ
        /// </summary>
        /// <param name="configuration">�ȴ����µ�Ӧ�ó�������</param>
        protected virtual void UpdateAppConfigWithInstallConfig(ApplicationConfiguration configuration)
        {
            // ����Ӧ�ó��������
            if (InstallConfig.ApplicationName != null)
            {
                //���֤�鲻Ϊ��
                if (configuration.SecurityConfiguration != null && configuration.SecurityConfiguration.ApplicationCertificate != null)
                {
                    if (configuration.SecurityConfiguration.ApplicationCertificate.SubjectName == configuration.ApplicationName)
                    {
                        configuration.SecurityConfiguration.ApplicationCertificate.SubjectName = InstallConfig.ApplicationName;
                    }
                }

                configuration.ApplicationName = InstallConfig.ApplicationName;
            }

            if (InstallConfig.ApplicationUri != null)
            {
                configuration.ApplicationUri = InstallConfig.ApplicationUri;
            }

            // �����ַ�а�����localhost(���Դ�Сд)�����ñ������������
            if (configuration.ApplicationUri != null)
            {
                int index = configuration.ApplicationUri.IndexOf("localhost", StringComparison.OrdinalIgnoreCase);

                if (index != -1)
                {
                    StringBuilder buffer = new StringBuilder();
                    buffer.Append(configuration.ApplicationUri.Substring(0, index));
                    buffer.Append(System.Net.Dns.GetHostName());
                    buffer.Append(configuration.ApplicationUri.Substring(index+"localhost".Length));
                    configuration.ApplicationUri = buffer.ToString();
                }
            }

            ServerBaseConfiguration serverConfiguration = null ;

            if (configuration.ServerConfiguration != null)
            {
                serverConfiguration = configuration.ServerConfiguration;
            }
            else if (configuration.DiscoveryServerConfiguration != null)
            {
                serverConfiguration = configuration.DiscoveryServerConfiguration;
            }

            //���� ��װ���� �е���Ϣ�����������õ�ַ
            if (serverConfiguration != null)
            {
                if (InstallConfig.BaseAddresses != null && InstallConfig.BaseAddresses.Count > 0)
                {
                    Dictionary<string, string> addresses = new Dictionary<string, string>();
                    serverConfiguration.BaseAddresses.Clear();

                    for (int ii = 0; ii < InstallConfig.BaseAddresses.Count; ii++)
                    {
                        Uri url = Utils.ParseUri(InstallConfig.BaseAddresses[ii]);

                        if (url != null)
                        {
                            if (!addresses.ContainsKey(url.Scheme))
                            {
                                serverConfiguration.BaseAddresses.Add(url.ToString());
                                addresses.Add(url.Scheme, String.Empty);
                            }
                            else
                            {
                                serverConfiguration.AlternateBaseAddresses.Add(url.ToString());
                            }
                        }
                    }
                }

                if (InstallConfig.SecurityProfiles != null && InstallConfig.SecurityProfiles.Count > 0)
                {
                    ServerSecurityPolicyCollection securityPolicies = new ServerSecurityPolicyCollection();

                    for (int ii = 0; ii < InstallConfig.SecurityProfiles.Count; ii++)
                    {
                        for (int jj = 0; jj < serverConfiguration.SecurityPolicies.Count; jj++)
                        {
                            if (serverConfiguration.SecurityPolicies[jj].SecurityPolicyUri == InstallConfig.SecurityProfiles[ii].ProfileUri)
                            {
                                securityPolicies.Add(serverConfiguration.SecurityPolicies[jj]);
                            }
                        }
                    }

                    serverConfiguration.SecurityPolicies = securityPolicies;
                }
            }

            if (InstallConfig.ApplicationCertificate != null)
            {
                configuration.SecurityConfiguration.ApplicationCertificate.StoreType = InstallConfig.ApplicationCertificate.StoreType;
                configuration.SecurityConfiguration.ApplicationCertificate.StorePath = InstallConfig.ApplicationCertificate.StorePath;

                if (String.IsNullOrEmpty(InstallConfig.ApplicationCertificate.SubjectName))
                {
                    configuration.SecurityConfiguration.ApplicationCertificate.SubjectName = InstallConfig.ApplicationCertificate.SubjectName;
                }
            }

            if (InstallConfig.RejectedCertificatesStore != null)
            {
                configuration.SecurityConfiguration.RejectedCertificateStore = Opc.Ua.Security.SecuredApplication.FromCertificateStoreIdentifier(InstallConfig.RejectedCertificatesStore);
            }
            
            if (InstallConfig.IssuerCertificateStore != null)
            {
                configuration.SecurityConfiguration.TrustedIssuerCertificates.StoreType = InstallConfig.IssuerCertificateStore.StoreType;
                configuration.SecurityConfiguration.TrustedIssuerCertificates.StorePath = InstallConfig.IssuerCertificateStore.StorePath;
                configuration.SecurityConfiguration.TrustedIssuerCertificates.ValidationOptions = (CertificateValidationOptions)(int)InstallConfig.IssuerCertificateStore.ValidationOptions;
            }
            
            if (InstallConfig.TrustedCertificateStore != null)
            {
                configuration.SecurityConfiguration.TrustedPeerCertificates.StoreType = InstallConfig.TrustedCertificateStore.StoreType;
                configuration.SecurityConfiguration.TrustedPeerCertificates.StorePath = InstallConfig.TrustedCertificateStore.StorePath;
                configuration.SecurityConfiguration.TrustedPeerCertificates.ValidationOptions = (CertificateValidationOptions)(int)InstallConfig.TrustedCertificateStore.ValidationOptions;
            }

            configuration.CertificateValidator.Update(configuration);
        }

        /// <summary>
        /// ��װ����
        /// </summary>
        /// <param name="silent">�������Ϊ <c>true</c> ������ʾ�κεĶԻ���</param>
        /// <param name="args">�ṩ�������еĶ������</param>
        protected virtual void Install(bool silent, Dictionary<string, string> args)
        {
            //д��׷����־
            Utils.Trace(Utils.TraceMasks.Information, "Installing application.");

            // ���������Ϣ
            string filePath = Utils.GetAbsoluteFilePath(InstallConfig.ConfigurationFile, true, false, false);

            if (filePath == null)
            {
                Utils.Trace("WARNING: Could not load config file specified in the installation configuration: {0}", InstallConfig.ConfigurationFile);
                filePath = ApplicationConfiguration.GetFilePathFromAppConfig(ConfigSectionName);
                InstallConfig.ConfigurationFile = filePath;
            }

            ApplicationConfiguration configuration = LoadAppConfig(silent, filePath, 
                Security.SecuredApplication.FromApplicationType(InstallConfig.ApplicationType), ConfigurationType, false);

            if (configuration == null)
            {
                return;
            }

            // ��������
            UpdateAppConfigWithInstallConfig(configuration);
            ApplicationConfiguration = configuration;
            
            // ���֤��
            X509Certificate2 certificate = configuration.SecurityConfiguration.ApplicationCertificate.Find(true);

            if (certificate != null)
            {
                if (!silent)
                {
                    if (!CheckApplicationInstanceCertificate(configuration, certificate, silent, InstallConfig.MinimumKeySize))
                    {
                        certificate = null;
                    }
                }
            }

            // ����һ���µ�֤��
            if (certificate == null)
            {
                certificate = CreateApplicationInstanceCertificate(configuration, InstallConfig.MinimumKeySize, InstallConfig.LifeTimeInMonths);
            }

            // ȷ��֤���Ǳ����ε�
            AddToTrustedStore(configuration, certificate);
            
            // ����Ƿ��������У���֤����ӵ����ַ���������б���
            if (configuration.ApplicationType == ApplicationType.Server || configuration.ApplicationType == ApplicationType.ClientAndServer)
            {
                try
                {
                    AddToDiscoveryServerTrustList(certificate, null, null, configuration.SecurityConfiguration.TrustedPeerCertificates);
                }
                catch (Exception e)
                {
                    Utils.Trace(e, "Could not add certificate to LDS trust list.");
                }
            }

            // ���÷���ǽ
            if (InstallConfig.ConfigureFirewall)
            {
                ConfigureFirewall(configuration, silent, false);
            }

            // ����HTTP����
            ConfigureHttpAccess(configuration, false);
 
            // ���ÿ�ִ���ļ��ķ��ʣ������ļ���˽Կ
            ConfigureFileAccess(configuration);

            // ���������ļ�
            ConfigUtils.UpdateConfigurationLocation(InstallConfig.ExecutableFile, InstallConfig.ConfigurationFile);

            try
            {
                // ensure the RawData does not get serialized.
                certificate = configuration.SecurityConfiguration.ApplicationCertificate.Certificate;

                configuration.SecurityConfiguration.ApplicationCertificate.Certificate = null;
                configuration.SecurityConfiguration.ApplicationCertificate.SubjectName = certificate.Subject;
                configuration.SecurityConfiguration.ApplicationCertificate.Thumbprint  = certificate.Thumbprint;

                configuration.SaveToFile(configuration.SourceFilePath);
                
                // restore the configuration.
                configuration.SecurityConfiguration.ApplicationCertificate.Certificate = certificate;
            }
            catch (Exception e)
            {
                Utils.Trace(e, "Could not save configuration file. FilePath={0}", configuration.SourceFilePath);
            }

            if (!NoGdsAgentAdmin)
            {
                try
                {
                    // install the GDS agent configuration file
                    string agentPath = Utils.GetAbsoluteDirectoryPath("%CommonApplicationData%\\OPC Foundation\\GDS\\Applications", false, false, true);

                    if (agentPath != null)
                    {
                        Opc.Ua.Security.SecuredApplication export = new Opc.Ua.Security.SecurityConfigurationManager().ReadConfiguration(configuration.SourceFilePath);
                        export.ExecutableFile = InstallConfig.ExecutableFile;

                        DataContractSerializer serializer = new DataContractSerializer(typeof(Opc.Ua.Security.SecuredApplication));

                        using (FileStream ostrm = File.Open(agentPath + "\\" + configuration.ApplicationName + ".xml", FileMode.Create))
                        {
                            serializer.WriteObject(ostrm, export);
                            Utils.Trace(Utils.TraceMasks.Information, "Created GDS agent configuration file.");
                        }
                    }
                }
                catch (Exception e)
                {
                    Utils.Trace(Utils.TraceMasks.Error, "Could not create GDS agent configuration file: {0}", e.Message);
                }
            }

            // ��װ����
            if (InstallConfig.InstallAsService)
            {
                Utils.Trace(Utils.TraceMasks.Information, "Installing service '{0}'.", InstallConfig.ApplicationName);

                OnBeforeInstallService();

                bool start = true;

                bool result = Opc.Ua.Configuration.ServiceInstaller.InstallService(
                    Application.ExecutablePath,
                    InstallConfig.ApplicationName,
                    configuration.ApplicationName,
                    InstallConfig.ServiceDescription,
                    InstallConfig.ServiceStartMode,
                    InstallConfig.ServiceUserName,
                    InstallConfig.ServicePassword,
                    ref start);

                if (!result)
                {
                    throw ServiceResultException.Create(StatusCodes.BadConfigurationError, "Could not install service.");
                }

                Utils.Trace(Utils.TraceMasks.Information, "Service '{0}' installed as {1}.", InstallConfig.ApplicationName, InstallConfig.ServiceStartMode);
            }
        }

        /// <summary>
        /// �ڷ���װ֮ǰ��������
        /// </summary>
        protected virtual void OnBeforeInstallService()
        {
            // can be overridden in child class.
        }

        /// <summary>
        /// ж�ط���ķ���
        /// </summary>
        /// <param name="silent">�������Ϊ <c>true</c> ������ʾ���еĶԻ���</param>
        /// <param name="args">���������ṩ�Ķ������</param>
        protected virtual void Uninstall(bool silent, Dictionary<string, string> args)
        {
            // �������
            string filePath = Utils.GetAbsoluteFilePath(InstallConfig.ConfigurationFile, true, false, false);

            if (filePath == null)
            {
                Utils.Trace("WARNING: Could not load config file specified in the installation configuration: {0}", InstallConfig.ConfigurationFile);
                filePath = ApplicationConfiguration.GetFilePathFromAppConfig(ConfigSectionName);
                InstallConfig.ConfigurationFile = filePath;
            }

            ApplicationConfiguration configuration = LoadAppConfig(silent, filePath, Opc.Ua.Security.SecuredApplication.FromApplicationType(InstallConfig.ApplicationType), ConfigurationType, false);
            ApplicationConfiguration = configuration;

            if (configuration != null)
            {
                // configure the firewall.
                ConfigureFirewall(configuration, false, true);

                // configure HTTP access.
                ConfigureHttpAccess(configuration, true);

                // delete certificate.
                if (InstallConfig.DeleteCertificatesOnUninstall)
                {
                    DeleteApplicationInstanceCertificate(configuration);
                }            
            }

            if (InstallConfig.InstallAsService)
            {
                if (!Opc.Ua.Configuration.ServiceInstaller.UnInstallService(InstallConfig.ApplicationName))
                {
                    Utils.Trace("Service could not be uninstalled.");
                }
            }

            if (!NoGdsAgentAdmin)
            {
                try
                {
                    string agentPath = Utils.GetAbsoluteDirectoryPath("%CommonApplicationData%\\OPC Foundation\\GDS\\Applications", false, false, false);

                    if (agentPath != null)
                    {
                        File.Delete(agentPath + "\\" + configuration.ApplicationName + ".xml");
                    }
                }
                catch (Exception e)
                {
                    Utils.Trace(Utils.TraceMasks.Error, "Could not create GDS agent configuration file: {0}", e.Message);
                }
            }
        }

        /// <summary>
        /// ����������
        /// </summary>
        /// <param name="silent">�������Ϊ <c>true</c> ������ʾ�κεĶԻ���</param>
        /// <param name="args">���������ṩ�Ķ������</param>
        /// <returns>��������ѱ���������True</returns>
        protected virtual bool ProcessCommand(bool silent, Dictionary<string, string> args)
        {
            return false;
        }
        #endregion

        #region ��̬����
        /// <summary>
        /// ����һ������
        /// </summary>
        public static ApplicationConfiguration LoadAppConfig(
            bool silent, 
            string filePath,
            ApplicationType applicationType,
            Type configurationType, 
            bool applyTraceSettings)
        {
            Utils.Trace(Utils.TraceMasks.Information, "Loading application configuration file. {0}", filePath);

            try
            {
                // load the configuration file.
                ApplicationConfiguration configuration = ApplicationConfiguration.Load(
                    new System.IO.FileInfo(filePath), 
                    applicationType,
                    configurationType,
                    applyTraceSettings);

                if (configuration == null)
                {
                    return null;
                }

                return configuration;
            }
            catch (Exception e)
            {
                // warn user.
                if (!silent)
                {
                    ExceptionDlg.Show("Load Application Configuration", e);
                }

                Utils.Trace(e, "Could not load configuration file. {0}", filePath);
                return null;
            }
        }

        /// <summary>
        /// ����Ӧ�ó�������
        /// </summary>
        public ApplicationConfiguration LoadApplicationConfiguration(string filePath, bool silent)
        {
            ApplicationConfiguration configuration = LoadAppConfig(silent, filePath, ApplicationType, ConfigurationType, true);
            m_applicationConfiguration = configuration ?? throw ServiceResultException.Create(StatusCodes.BadConfigurationError, "Could not load configuration file.");

            return configuration;
        }

        /// <summary>
        /// ����Ӧ�ó�������
        /// </summary>
        public ApplicationConfiguration LoadApplicationConfiguration(bool silent)
        {
            string filePath = ApplicationConfiguration.GetFilePathFromAppConfig(ConfigSectionName);
            ApplicationConfiguration configuration = LoadAppConfig(silent, filePath, ApplicationType, ConfigurationType, true);

            m_applicationConfiguration = configuration ?? throw ServiceResultException.Create(StatusCodes.BadConfigurationError, "Could not load configuration file.");

            return configuration;
        }

        /// <summary>
        /// ���һ����Ч��Ӧ�ó���ʵ��֤��
        /// </summary>
        /// <param name="silent">if set to <c>true</c> no dialogs will be displayed.</param>
        /// <param name="minimumKeySize">Minimum size of the key.</param>
        public void CheckApplicationInstanceCertificate(
            bool silent,
            ushort minimumKeySize)
        {
            Utils.Trace(Utils.TraceMasks.Information, "Checking application instance certificate.");

            ApplicationConfiguration configuration = null;

            if (m_applicationConfiguration == null)
            {
                LoadApplicationConfiguration(silent);
            }

            configuration = m_applicationConfiguration;
            bool createNewCertificate = true;

            // find the existing certificate.
            CertificateIdentifier id = configuration.SecurityConfiguration.ApplicationCertificate;

            if (id == null)
            {
                throw ServiceResultException.Create(StatusCodes.BadConfigurationError, "Configuration file does not specify a certificate.");
            }

            X509Certificate2 certificate = id.Find(true);

            // check that it is ok.
            if (certificate != null)
            {
                createNewCertificate = !CheckApplicationInstanceCertificate(configuration, certificate, silent, minimumKeySize);
            }
            else
            {
                // check for missing private key.
                certificate = id.Find(false);

                if (certificate != null)
                {
                    throw ServiceResultException.Create(StatusCodes.BadConfigurationError, "Cannot access certificate private key. Subject={0}", certificate.Subject);
                }

                // check for missing thumbprint.
                if (!String.IsNullOrEmpty(id.Thumbprint))
                {
                    if (!String.IsNullOrEmpty(id.SubjectName))
                    {
                        CertificateIdentifier id2 = new CertificateIdentifier();
                        id2.StoreType = id.StoreType;
                        id2.StorePath = id.StorePath;
                        id2.SubjectName = id.SubjectName;

                        certificate = id2.Find(true);
                    }

                    if (certificate != null)
                    {
                        string message = Utils.Format(
                            "Thumbprint was explicitly specified in the configuration." +
                            "\r\nAnother certificate with the same subject name was found." +
                            "\r\nUse it instead?\r\n" +
                            "\r\nRequested: {0}" +
                            "\r\nFound: {1}",
                            id.SubjectName,
                            certificate.Subject);

                        throw ServiceResultException.Create(StatusCodes.BadConfigurationError, message);
                    }
                    else
                    {
                        string message = Utils.Format("Thumbprint was explicitly specified in the configuration. Cannot generate a new certificate.");
                        throw ServiceResultException.Create(StatusCodes.BadConfigurationError, message);
                    }
                }
                else
                {
                    if (String.IsNullOrEmpty(id.SubjectName))
                    {
                        string message = Utils.Format("Both SubjectName and Thumbprint are not specified in the configuration. Cannot generate a new certificate.");
                        throw ServiceResultException.Create(StatusCodes.BadConfigurationError, message);
                    }
                }
            }
            
            // create a new certificate.
            if (createNewCertificate)
            {
                certificate = CreateApplicationInstanceCertificate(configuration, minimumKeySize, 600);
            }

            // ensure it is trusted.
            else
            {
                AddToTrustedStore(configuration, certificate);
            }

            // add to discovery server.
            if (configuration.ApplicationType == ApplicationType.Server || configuration.ApplicationType == ApplicationType.ClientAndServer)
            {
                try
                {
                    AddToDiscoveryServerTrustList(certificate, null, null, configuration.SecurityConfiguration.TrustedPeerCertificates);
                }
                catch (Exception e)
                {
                    Utils.Trace(e, "Could not add certificate to LDS trust list.");
                }
            }
        }
        #endregion

        #region HTTPS ֧��
        /// <summary>
        /// ��HTTPS֤��ʹ��UA��֤�߼�
        /// </summary>
        /// <param name="validator">The validator.</param>
        public static void SetUaValidationForHttps(CertificateValidator validator)
        {
            m_validator = validator;
            System.Net.ServicePointManager.ServerCertificateValidationCallback = HttpsCertificateValidation;
        }

        /// <summary>
        /// Զ����֤֤��
        /// </summary>
        private static bool HttpsCertificateValidation(
            object sender,
            X509Certificate cert,
            X509Chain chain,
            System.Net.Security.SslPolicyErrors error)
        {
            try
            {
                m_validator.Validate(new X509Certificate2(cert.GetRawCertData()));
                return true;
            }
            catch (Exception e)
            {
                Utils.Trace(e, "Could not verify SSL certificate: {0}", cert.Subject);
                return false;
            }
        }

        private static CertificateValidator m_validator;
        #endregion

        #region ˽�з���
        /// <summary>
        /// ����֤����֤����
        /// </summary>
        private static void CertificateValidator_CertificateValidation(CertificateValidator validator, CertificateValidationEventArgs e)
        {
            try
            {
                if (e.Error != null && e.Error.Code == StatusCodes.BadCertificateUntrusted)
                {
                    e.Accept = true;
                    Utils.Trace((int)Utils.TraceMasks.Security, "Automatically accepted certificate: {0}", e.Certificate.Subject);
                }
            }
            catch (Exception exception)
            {
                Utils.Trace(exception, "Error accepting certificate.");
            }
        }

        /// <summary>
        /// ����Ӧ�ó���ʵ��֤�飨����в����ڣ���
        /// </summary>
        private static bool CheckApplicationInstanceCertificate(
            ApplicationConfiguration configuration,
            X509Certificate2 certificate,
            bool silent,
            ushort minimumKeySize)
        {
            if (certificate == null)
            {
                return false;
            }

            Utils.Trace(Utils.TraceMasks.Information, "Checking application instance certificate. {0}", certificate.Subject);

            // validate certificate.
            configuration.CertificateValidator.Validate(certificate);

            // check key size.
            if (minimumKeySize > certificate.PublicKey.Key.KeySize)
            {
                bool valid = false;

                string message = Utils.Format(
                    "The key size ({0}) in the certificate is less than the minimum provided ({1}). Update certificate?",
                    certificate.PublicKey.Key.KeySize,
                    minimumKeySize);

                if (!silent)
                {
                    if (MessageBox.Show(message, configuration.ApplicationName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    {
                        valid = true;
                    }
                }

                Utils.Trace(message);

                if (!valid)
                {
                    return false;
                }
            }

            // check domains.
            if (configuration.ApplicationType != ApplicationType.Client)
            {
                if (!CheckDomainsInCertificate(configuration, certificate, silent))
                {
                    return false;
                }
            }

            // update uri.
            string applicationUri = Utils.GetApplicationUriFromCertficate(certificate);

            if (String.IsNullOrEmpty(applicationUri))
            {
                bool valid = false;

                string message = "The Application URI is not specified in the certificate. Update certificate?";

                if (!silent)
                {
                    if (MessageBox.Show(message, configuration.ApplicationName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    {
                        valid = true;
                    }
                }

                Utils.Trace(message);

                if (!valid)
                {
                    return false;
                }
            }
            
            // update configuration.
            configuration.ApplicationUri = applicationUri;
            configuration.SecurityConfiguration.ApplicationCertificate.Certificate = certificate;

            return true;
        }

        /// <summary>
        /// ����������ַ�е�����֤���е���ƥ��
        /// </summary>
        private static bool CheckDomainsInCertificate(
            ApplicationConfiguration configuration,
            X509Certificate2 certificate,
            bool silent)
        {
            Utils.Trace(Utils.TraceMasks.Information, "Checking domains in certificate. {0}", certificate.Subject);

            bool valid = true;
            IList<string> serverDomainNames = configuration.GetServerDomainNames();
            IList<string> certificateDomainNames = Utils.GetDomainsFromCertficate(certificate);

            // ��ȡ�����������
            string computerName = System.Net.Dns.GetHostName();

            // get DNS aliases and IP addresses.
            System.Net.IPHostEntry entry = System.Net.Dns.GetHostEntry(computerName);

            for (int ii = 0; ii < serverDomainNames.Count; ii++)
            {
                if (Utils.FindStringIgnoreCase(certificateDomainNames, serverDomainNames[ii]))
                {
                    continue;
                }

                if (String.Compare(serverDomainNames[ii], "localhost", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (Utils.FindStringIgnoreCase(certificateDomainNames, computerName))
                    {
                        continue;
                    }

                    // check for aliases.
                    bool found = false;

                    for (int jj = 0; jj < entry.Aliases.Length; jj++)
                    {
                        if (Utils.FindStringIgnoreCase(certificateDomainNames, entry.Aliases[jj]))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found)
                    {
                        continue;
                    }

                    // check for ip addresses.
                    for (int jj = 0; jj < entry.AddressList.Length; jj++)
                    {
                        if (Utils.FindStringIgnoreCase(certificateDomainNames, entry.AddressList[jj].ToString()))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found)
                    {
                        continue;
                    }
                }

                string message = Utils.Format(
                    "The server is configured to use domain '{0}' which does not appear in the certificate. Update certificate?",
                    serverDomainNames[ii]);

                valid = false;

                if (!silent)
                {
                    if (MessageBox.Show(message, configuration.ApplicationName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    {
                        valid = true;
                        continue;
                    }
                }

                Utils.Trace(message);
                break;
            }

            return valid;
        }

        /// <summary>
        /// ���÷���ǽ
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="silent">if set to <c>true</c> if no dialogs should be displayed.</param>
        /// <param name="remove">if set to <c>true</c> if removing permissions.</param>
        private static void ConfigureFirewall(ApplicationConfiguration configuration, bool silent, bool remove)
        {
            Utils.Trace(Utils.TraceMasks.Information, "Configuring firewall.");

            // check for ports to open/close.
            StringCollection baseAddresses = new StringCollection();

            if (configuration.ServerConfiguration != null)
            {
                baseAddresses = configuration.ServerConfiguration.BaseAddresses;
            }

            if (configuration.DiscoveryServerConfiguration != null)
            {
                baseAddresses = configuration.DiscoveryServerConfiguration.BaseAddresses;
            }

            // remove access.
            if (remove)
            {
                try
                {
                    ConfigUtils.RemoveFirewallAccess(Application.ExecutablePath, baseAddresses);
                }
                catch (Exception e)
                {
                    Utils.Trace(e, "Unexpected error while removing firewall access.");
                }

                return;
            }

            // enable access.
            try
            {
                // check if firewall needs configuration.
                if (!ConfigUtils.CheckFirewallAccess(Application.ExecutablePath, baseAddresses))
                {
                    bool configure = true;

                    if (!silent)
                    {
                        string message = "The firewall has not been configured to allow external access to the server. Configure firewall?";

                        if (MessageBox.Show(message, configuration.ApplicationName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        {
                            configure = false;
                        }
                    }

                    if (configure)
                    {
                        ConfigUtils.SetFirewallAccess(configuration.ApplicationName, Application.ExecutablePath, baseAddresses);
                    }
                }
            }
            catch (Exception e)
            {
                Utils.Trace(e, "Unexpected error while checking or changing the firewall configuration.");
            }
        }

        /// <summary>
        /// ����Ӧ�ó���ʵ��֤��
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="keySize">Size of the key.</param>
        /// <param name="lifetimeInMonths">The lifetime in months.</param>
        /// <returns>The new certificate</returns>
        private static X509Certificate2 CreateApplicationInstanceCertificate(
            ApplicationConfiguration configuration, 
            ushort keySize, 
            ushort lifetimeInMonths)
        {
            Utils.Trace(Utils.TraceMasks.Information, "Creating application instance certificate. KeySize={0}, Lifetime={1}", keySize, lifetimeInMonths);

            // delete existing any existing certificate.
            DeleteApplicationInstanceCertificate(configuration);

            CertificateIdentifier id = configuration.SecurityConfiguration.ApplicationCertificate;

            // get the domains from the configuration file.
            IList<string> serverDomainNames = configuration.GetServerDomainNames();

            if (serverDomainNames.Count == 0)
            {
                serverDomainNames.Add(System.Net.Dns.GetHostName());
            }

            // ensure the certificate store directory exists.
            if (id.StoreType == CertificateStoreType.Directory)
            {
                Utils.GetAbsoluteDirectoryPath(id.StorePath, true, true, true);
            }

            X509Certificate2 certificate = Opc.Ua.CertificateFactory.CreateCertificate(
                id.StoreType,
                id.StorePath,
                configuration.ApplicationUri,
                configuration.ApplicationName,
                null,
                serverDomainNames,
                keySize,
                lifetimeInMonths);

            id.Certificate = certificate;
            AddToTrustedStore(configuration, certificate);

            /*
            if (id.StoreType == CertificateStoreType.Directory)
            {
                DirectoryCertificateStore store = new DirectoryCertificateStore();
                store.Open(id.StorePath);

                List<ApplicationAccessRule> rules = new List<ApplicationAccessRule>();

                ApplicationAccessRule rule = new ApplicationAccessRule();
                rule.RuleType = AccessControlType.Allow;
                rule.Right = ApplicationAccessRight.Run;
                rule.IdentityName = WellKnownSids.NetworkService;
                rules.Add(rule);

                rule = new ApplicationAccessRule();
                rule.RuleType = AccessControlType.Allow;
                rule.Right = ApplicationAccessRight.Run;
                rule.IdentityName = WellKnownSids.LocalService;
                rules.Add(rule);

                rule = new ApplicationAccessRule();
                rule.RuleType = AccessControlType.Allow;
                rule.Right = ApplicationAccessRight.Run;
                rule.IdentityName = WellKnownSids.LocalSystem;
                rules.Add(rule);

                store.SetAccessRules(certificate.Thumbprint, rules, false);
            }
            */

            configuration.CertificateValidator.Update(configuration.SecurityConfiguration);

            Utils.Trace(Utils.TraceMasks.Information, "Certificate created. Thumbprint={0}", certificate.Thumbprint);

            // reload the certificate from disk.
            return configuration.SecurityConfiguration.ApplicationCertificate.LoadPrivateKey(null);
        }

        /// <summary>
        /// ɾ��һ���Ѿ����ڵ�Ӧ�ó���ʵ��֤�顣
        /// </summary>
        /// <param name="configuration">The configuration instance that stores the configurable information for a UA application.</param>
        private static void DeleteApplicationInstanceCertificate(ApplicationConfiguration configuration)
        {
            Utils.Trace(Utils.TraceMasks.Information, "Deleting application instance certificate.");

            // create a default certificate id none specified.
            CertificateIdentifier id = configuration.SecurityConfiguration.ApplicationCertificate;

            if (id == null)
            {
                return;
            }

            // delete private key.
            X509Certificate2 certificate = id.Find();

            // delete trusted peer certificate.
            if (configuration.SecurityConfiguration != null && configuration.SecurityConfiguration.TrustedPeerCertificates != null)
            {
                string thumbprint = id.Thumbprint;

                if (certificate != null)
                {
                    thumbprint = certificate.Thumbprint;
                }

                using (ICertificateStore store = configuration.SecurityConfiguration.TrustedPeerCertificates.OpenStore())
                {
                    store.Delete(thumbprint);
                }
            }

            // delete private key.
            if (certificate != null)
            {
                using (ICertificateStore store = id.OpenStore())
                {
                    store.Delete(certificate.Thumbprint);
                }
            }
        }

        /// <summary>
        /// ��Ӧ�ó���֤����ӵ����ַ����������б�
        /// </summary>
        public static void AddToDiscoveryServerTrustList(
            X509Certificate2 certificate,
            string oldThumbprint,
            IList<X509Certificate2> issuers,
            CertificateStoreIdentifier trustedCertificateStore)
        {
            Utils.Trace(Utils.TraceMasks.Information, "Adding certificate to discovery server trust list.");

            try
            {
                string configurationPath = Utils.GetAbsoluteFilePath(@"%CommonApplicationData%\OPC Foundation\Config\Opc.Ua.DiscoveryServer.Config.xml", true, false, false);

                if (configurationPath == null)
                {
                    throw new ServiceResultException("Could not find the discovery server configuration file. Please confirm that it is installed.");
                }

                Opc.Ua.Security.SecuredApplication ldsConfiguration = new Opc.Ua.Security.SecurityConfigurationManager().ReadConfiguration(configurationPath);
                CertificateStoreIdentifier csid = Opc.Ua.Security.SecuredApplication.FromCertificateStoreIdentifier(ldsConfiguration.TrustedCertificateStore);
                AddApplicationCertificateToStore(csid, certificate, oldThumbprint);

                if (issuers != null && ldsConfiguration.IssuerCertificateStore != null)
                {
                    csid = Opc.Ua.Security.SecuredApplication.FromCertificateStoreIdentifier(ldsConfiguration.IssuerCertificateStore);
                    AddIssuerCertificatesToStore(csid, issuers);
                }

                CertificateIdentifier cid = Opc.Ua.Security.SecuredApplication.FromCertificateIdentifier(ldsConfiguration.ApplicationCertificate);
                X509Certificate2 ldsCertificate = cid.Find(false);

                // add LDS certificate to application trust list.
                if (ldsCertificate != null && trustedCertificateStore != null)
                {
                    AddApplicationCertificateToStore(csid, ldsCertificate, null);
                }
            }
            catch (Exception e)
            {
                Utils.Trace(e, "Could not add certificate to discovery server trust list.");
            }
        }

        /// <summary>
        /// ��Ӧ��֤����ӵ��洢
        /// </summary>
        private static void AddApplicationCertificateToStore(
            CertificateStoreIdentifier csid,
            X509Certificate2 certificate,
            string oldThumbprint)
        {
            ICertificateStore store = csid.OpenStore();

            try
            {
                // ɾ���ɵ�֤��
                if (oldThumbprint != null)
                {
                    store.Delete(oldThumbprint);
                }

                // delete certificates with the same application uri.
                if (store.FindByThumbprint(certificate.Thumbprint) == null)
                {
                    string applicationUri = Utils.GetApplicationUriFromCertficate(certificate);

                    // delete any existing certificates.
                    foreach (X509Certificate2 target in store.Enumerate())
                    {
                        if (Utils.CompareDistinguishedName(target.Subject, certificate.Subject))
                        {
                            if (Utils.GetApplicationUriFromCertficate(target) == applicationUri)
                            {
                                store.Delete(target.Thumbprint);
                            }
                        }
                    }

                    // add new certificate.
                    store.Add(new X509Certificate2(certificate.RawData));
                }
            }
            finally
            {
                store.Close();
            }
        }

        /// <summary>
        /// ��Ӧ��֤����ӵ��洢
        /// </summary>
        private static void AddIssuerCertificatesToStore(CertificateStoreIdentifier csid, IList<X509Certificate2> issuers)
        {
            ICertificateStore store = csid.OpenStore();

            try
            {
                foreach (X509Certificate2 issuer in issuers)
                {
                    if (store.FindByThumbprint(issuer.Thumbprint) == null)
                    {
                        store.Add(issuer);
                    }
                }
            }
            finally
            {
                store.Close();
            }
        }

        /// <summary>
        /// ��֤����ӵ�����֤��洢
        /// </summary>
        /// <param name="configuration">The application's configuration which specifies the location of the TrustedStore.</param>
        /// <param name="certificate">The certificate to register.</param>
        private static void AddToTrustedStore(ApplicationConfiguration configuration, X509Certificate2 certificate)
        {
            string storePath = null;

            if (configuration != null && configuration.SecurityConfiguration != null && configuration.SecurityConfiguration.TrustedPeerCertificates != null)
            {
                storePath = configuration.SecurityConfiguration.TrustedPeerCertificates.StorePath;
            }

            if (String.IsNullOrEmpty(storePath))
            {
                Utils.Trace(Utils.TraceMasks.Information, "WARNING: Trusted peer store not specified.");
                return;
            }

            try
            {
                ICertificateStore store = configuration.SecurityConfiguration.TrustedPeerCertificates.OpenStore();

                if (store == null)
                {
                    Utils.Trace("Could not open trusted peer store. StorePath={0}", storePath);
                    return;
                }

                try
                {
                    // check if it already exists.
                    X509Certificate2 certificate2 = store.FindByThumbprint(certificate.Thumbprint);

                    if (certificate2 != null)
                    {
                        return;
                    } 
                    
                    Utils.Trace(Utils.TraceMasks.Information, "Adding certificate to trusted peer store. StorePath={0}", storePath);

                    List<string> subjectName = Utils.ParseDistinguishedName(certificate.Subject);

                    // check for old certificate.
                    X509Certificate2Collection certificates = store.Enumerate();

                    for (int ii = 0; ii < certificates.Count; ii++)
                    {
                        if (Utils.CompareDistinguishedName(certificates[ii], subjectName))
                        {
                            if (certificates[ii].Thumbprint == certificate.Thumbprint)
                            {
                                return;
                            }

                            store.Delete(certificates[ii].Thumbprint);
                            break;
                        }
                    }

                    // add new certificate.
                    X509Certificate2 publicKey = new X509Certificate2(certificate.GetRawCertData());
                    store.Add(publicKey);
                }
                finally
                {
                    store.Close();
                }
            }
            catch (Exception e)
            {
                Utils.Trace(e, "Could not add certificate to trusted peer store. StorePath={0}", storePath);
            }
        }

        /// <summary>
        /// ���� HTTP ����.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="remove">if set to <c>true</c> then the HTTP access should be removed.</param>
        private void ConfigureHttpAccess(ApplicationConfiguration configuration, bool remove)
        {
            Utils.Trace(Utils.TraceMasks.Information, "Configuring HTTP access.");

            // check for HTTP endpoints which need configuring.
            StringCollection baseAddresses = new StringCollection();

            if (configuration.DiscoveryServerConfiguration != null)
            {
                baseAddresses = configuration.DiscoveryServerConfiguration.BaseAddresses;
            }

            if (configuration.ServerConfiguration != null)
            {
                baseAddresses = configuration.ServerConfiguration.BaseAddresses;
            }

            // configure WCF http access.
            for (int ii = 0; ii < baseAddresses.Count; ii++)
            {
                string url = GetHttpUrlForAccessRule(baseAddresses[ii]);

                if (url != null)
                {
                    SetHttpAccessRules(url, remove);
                }
            }
        }

        /// <summary>
        /// ��ȡ����HTTP���ʹ����HTTP URL��
        /// </summary>
        public static string GetHttpUrlForAccessRule(string baseAddress)
        {
            Uri url = Utils.ParseUri(baseAddress);

            if (url == null)
            {
                return null;
            }
            
            UriBuilder builder = new UriBuilder(url);

            switch (url.Scheme)
            {
                case Utils.UriSchemeHttps:
                {
                    builder.Path = String.Empty;
                    builder.Query = String.Empty;
                    break;
                }

                case Utils.UriSchemeNoSecurityHttp:
                {
                    builder.Scheme = Utils.UriSchemeHttp;
                    builder.Path = String.Empty;
                    builder.Query = String.Empty;
                    break;
                }

                case Utils.UriSchemeHttp:
                {
                    break;
                }

                default:
                {
                    return null;
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// ��ȡ����Ӧ�ó���ķ��ʹ���
        /// </summary>
        private List<ApplicationAccessRule> GetAccessRules()
        {
            List<ApplicationAccessRule> rules = new List<ApplicationAccessRule>();

            // check for rules specified in the installer configuration.
            bool hasAdmin = false;

            if (InstallConfig.AccessRules != null)
            {
                for (int ii = 0; ii < InstallConfig.AccessRules.Count; ii++)
                {
                    ApplicationAccessRule rule = InstallConfig.AccessRules[ii];

                    if (rule.Right == ApplicationAccessRight.Configure && rule.RuleType == AccessControlType.Allow)
                    {
                        hasAdmin = true;
                        break;
                    }
                }

                rules = InstallConfig.AccessRules;
            }

            // provide some default rules.
            if (rules.Count == 0)
            {
                // give user run access.
                ApplicationAccessRule rule = new ApplicationAccessRule();
                rule.RuleType = AccessControlType.Allow;
                rule.Right = ApplicationAccessRight.Run;
                rule.IdentityName = WellKnownSids.Users;
                rules.Add(rule);

                // ensure service can access.
                if (InstallConfig.InstallAsService)
                {
                    rule = new ApplicationAccessRule();
                    rule.RuleType = AccessControlType.Allow;
                    rule.Right = ApplicationAccessRight.Run;
                    rule.IdentityName = WellKnownSids.NetworkService;
                    rules.Add(rule);

                    rule = new ApplicationAccessRule();
                    rule.RuleType = AccessControlType.Allow;
                    rule.Right = ApplicationAccessRight.Run;
                    rule.IdentityName = WellKnownSids.LocalService;
                    rules.Add(rule);
                }               
            }

            // ensure someone can change the configuration later.
            if (!hasAdmin)
            {
                ApplicationAccessRule rule = new ApplicationAccessRule();
                rule.RuleType = AccessControlType.Allow;
                rule.Right = ApplicationAccessRight.Configure;
                rule.IdentityName = WellKnownSids.Administrators;
                rules.Add(rule);
            }

            return rules;
        }

        /// <summary>
        /// ����URL��HTTP���ʹ���
        /// </summary>
        private void SetHttpAccessRules(string url, bool remove)
        {
            try
            {
                List<ApplicationAccessRule> rules = new List<ApplicationAccessRule>();

                if (!remove)
                {
                    rules = GetAccessRules();
                }

                HttpAccessRule.SetAccessRules(new Uri(url), rules, false);
            }
            catch (Exception e)
            {
                Utils.Trace(e, "Unexpected configuring the HTTP access rules.");
            }
        }

        /// <summary>
        /// ���öԿ�ִ���ļ��������ļ���˽Կ�ķ���
        /// </summary>
        private void ConfigureFileAccess(ApplicationConfiguration configuration)
        {
            Utils.Trace(Utils.TraceMasks.Information, "Configuring file access.");

            List<ApplicationAccessRule> rules = GetAccessRules();

            // apply access rules to the excutable file.
            try
            {
                if (InstallConfig.SetExecutableFilePermissions)
                {
                    ApplicationAccessRule.SetAccessRules(InstallConfig.ExecutableFile, rules, true);
                }
            }
            catch (Exception e)
            {
                Utils.Trace(e, "Could not set executable file permissions.");
            }

            // apply access rules to the configuration file.
            try
            {
                if (InstallConfig.SetConfigurationFilePermisions)
                {
                    ApplicationAccessRule.SetAccessRules(configuration.SourceFilePath, rules, true);
                }
            }
            catch (Exception e)
            {
                Utils.Trace(e, "Could not set configuration file permissions.");
            }

            // apply access rules to the private key file.
            try
            {
                X509Certificate2 certificate = configuration.SecurityConfiguration.ApplicationCertificate.Find(true);

                if (certificate != null)
                {
                    ICertificateStore store = configuration.SecurityConfiguration.ApplicationCertificate.OpenStore();
                    store.SetAccessRules(certificate.Thumbprint, rules, true);
                }
            }
            catch (Exception e)
            {
                Utils.Trace(e, "Could not set private key file permissions.");
            }
        }
        #endregion
        
        #region ˽���ֶ�
        private string m_applicationName;
        private ApplicationType m_applicationType;
        private string m_configSectionName;
        private Type m_configurationType;
        private InstalledApplication m_installConfig;
        private ServerBase m_server;
        private ApplicationConfiguration m_applicationConfiguration;
        #endregion
    }
}
