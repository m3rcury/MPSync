using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Configuration;

namespace MPSync_Launcher {
    static class Program {

        private static string pluginFile = string.Empty;
        private static string[] assemblies = null;

        enum MsgBoxIcon {
            INFO,
            WARNING,
            CRITICAL
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Load Plugin Configuration
            LoadPluginConfiguration();
        }
        
        public static void LoadPluginConfiguration() {

            pluginFile = Config.GetFile(Config.Dir.Plugins, "process", Properties.Resources.plugin.ToLower());

            if (!File.Exists(pluginFile)) {
                string message = string.Format("Plugin not found: {0}", pluginFile);
                ShowMessage(message, MsgBoxIcon.INFO);
                return;
            }

            // Flag if plugin has a configuration form
            bool hasConfigForm = false;

            try {
                // Get Plugin Assembly and check if its valid
                Assembly pluginAssembly = Assembly.LoadFrom(pluginFile);

                if (pluginAssembly == null) {
                    string message = string.Format("Invalid Plugin: {0}", pluginFile);
                    ShowMessage(message, MsgBoxIcon.INFO);
                }

                Type[] exportedTypes = pluginAssembly.GetExportedTypes();

                foreach (Type type in exportedTypes) {
                    if (type.IsAbstract) {
                        continue;
                    }

                    if (type.GetInterface("MediaPortal.GUI.Library.ISetupForm") != null) {
                        // Load any optional assemblies to support the plugin
                        if (assemblies != null) {
                            foreach (string assembly in assemblies) {
                                if (File.Exists(assembly)) {
                                    try {
                                        Assembly.LoadFile(assembly);
                                    }
                                    catch (Exception e){
                                        string message = string.Format("Exception in plugin SetupForm loading: \n\n{0} ", e.Message);
                                        ShowMessage(message, MsgBoxIcon.CRITICAL);                                        
                                    }
                                }
                            }
                        }

                        try {                           
                            // Create instance of the current type                           
                            object pluginObject = Activator.CreateInstance(type);
                            ISetupForm pluginForm = pluginObject as ISetupForm;                            

                            // If the plugin has a Configuration form, show it
                            if (pluginForm != null) {
                                if (pluginForm.HasSetup()) {
                                    hasConfigForm = true;
                                    pluginForm.ShowPlugin();
                                    return;
                                }                                   
                            }
                        }
                        catch (Exception setupFormException) {
                            string message = string.Format("Exception in plugin SetupForm loading: \n\n{0} ", setupFormException.Message);
                            ShowMessage(message, MsgBoxIcon.CRITICAL);
                        }
                    }
                }
                
            }
            catch (Exception unknownException) {
                string message = string.Format("Exception in plugin loading: \n\n{0}", unknownException.Message);
                ShowMessage(message, MsgBoxIcon.CRITICAL);
            }

            if (!hasConfigForm) {
                string message = "Plugin does not have a configuration form to show";
                ShowMessage(message, MsgBoxIcon.INFO);
            }

        }

        private static void ShowMessage(string message, MsgBoxIcon icon) {
            MessageBoxIcon MessageBoxIcon = MessageBoxIcon.Information;
            if (icon == MsgBoxIcon.CRITICAL) MessageBoxIcon = MessageBoxIcon.Error;
            MessageBox.Show(message, "Plugin Config Loader", MessageBoxButtons.OK, MessageBoxIcon);
            return;
        }

    }
}
