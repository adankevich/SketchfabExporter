using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swpublished;
using SolidWorks.Interop.swconst;
using SolidWorksTools;
using SolidWorksTools.File;
using Microsoft.Win32;
using System.Windows.Forms;
using System.IO;

namespace SketchfabExporterforSW
{
    /// <summary>
    /// 
    /// </summary>
    [Guid("98EBB85E-9C91-47BE-B5A2-F177609F7A80"), ComVisible(true)]
    [SwAddin(
        Description = "Upload your 3D models to Sketchfab!",
        Title = "Sketchfab Exporter Beta",
        LoadAtStartup = true
        )]
    public class SwAddin : ISwAddin
    {
        #region Fields

        BitmapHandler bitmapHandler = new BitmapHandler();

        /// <summary>
        /// Contains path to registry key that contains all SW command manager records. {0} parameter represents SW version (2009,2010,2011)
        /// </summary>
        const string SW_UI_REGISTRY_STORE = @"Software\SolidWorks\SolidWorks {0}\User Interface\";
        /// <summary>
        /// Contains list of comma separated keys that hold SW Command manager tab and toolbar records 
        /// </summary>
        const string SW_UI_REGISTRY_STORE_KEYS = @"CommandManager\AssyContext\,CommandManager\DrwContext\,CommandManager\EditPartContext\,CommandManager\PartContext\,Custom API Toolbars\";

        #endregion

        #region Properties

        internal static ISldWorks SwApp
        {
            get;
            private set;
        }

        internal int Cookie
        {
            get;
            private set;
        }

        internal ICommandManager CmdMgr
        {
            get;
            private set;
        }

        internal Guid AddinGuid
        {
            get
            {
                return this.GetType().GUID;
            }
        }

        #endregion

        #region SolidWorks Registration
        [ComRegisterFunctionAttribute]
        public static void RegisterFunction(Type t)
        {
            #region Get Custom Attribute: SwAddinAttribute
            SwAddinAttribute SWattr = null;
            Type type = typeof(SwAddin);

            foreach (System.Attribute attr in type.GetCustomAttributes(false))
            {
                if (attr is SwAddinAttribute)
                {
                    SWattr = attr as SwAddinAttribute;
                    break;
                }
            }

            #endregion

            try
            {
                Microsoft.Win32.RegistryKey hklm = Microsoft.Win32.Registry.LocalMachine;
                Microsoft.Win32.RegistryKey hkcu = Microsoft.Win32.Registry.CurrentUser;

                string keyname = "SOFTWARE\\SolidWorks\\Addins\\{" + t.GUID.ToString() + "}";
                Microsoft.Win32.RegistryKey addinkey = hklm.CreateSubKey(keyname);
                addinkey.SetValue(null, 0);

                //TODO can I change these Desciption= Rapid Sheet Metal - Rapid Quote Plug in    Tittle= Rapid Sheet Metal 2012 
                addinkey.SetValue("Description", SWattr.Description);
                addinkey.SetValue("Title", SWattr.Title);
#if DEBUG
                keyname = "Software\\SolidWorks\\AddInsStartup\\{" + t.GUID.ToString() + "}";
                addinkey = hkcu.CreateSubKey(keyname);
                addinkey.SetValue(null, Convert.ToInt32(SWattr.LoadAtStartup), Microsoft.Win32.RegistryValueKind.DWord);
#endif
            }
            catch (System.NullReferenceException nl)
            {
                Console.WriteLine("There was a problem registering this dll: SWattr is null. \n\"" + nl.Message + "\"");
                System.Windows.Forms.MessageBox.Show("There was a problem registering this dll: SWattr is null.\n\"" + nl.Message + "\"");
            }

            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);

                System.Windows.Forms.MessageBox.Show("There was a problem registering the function: \n\"" + e.Message + "\"");
            }
        }

        [ComUnregisterFunctionAttribute]
        public static void UnregisterFunction(Type t)
        {
            try
            {
                Microsoft.Win32.RegistryKey hklm = Microsoft.Win32.Registry.LocalMachine;
                Microsoft.Win32.RegistryKey hkcu = Microsoft.Win32.Registry.CurrentUser;

                string keyname = "SOFTWARE\\SolidWorks\\Addins\\{" + t.GUID.ToString() + "}";
                hklm.DeleteSubKey(keyname);
#if DEBUG
                keyname = "Software\\SolidWorks\\AddInsStartup\\{" + t.GUID.ToString() + "}";
                hkcu.DeleteSubKey(keyname);
#endif
            }
            catch (System.NullReferenceException nl)
            {
                Console.WriteLine("There was a problem unregistering this dll: " + nl.Message);
                System.Windows.Forms.MessageBox.Show("There was a problem unregistering this dll: \n\"" + nl.Message + "\"");
            }
            catch (System.Exception e)
            {
                Console.WriteLine("There was a problem unregistering this dll: " + e.Message);
                System.Windows.Forms.MessageBox.Show("There was a problem unregistering this dll: \n\"" + e.Message + "\"");
            }
        }

        #endregion

        #region ISwAddin Members

        public bool ConnectToSW(object ThisSW, int cookie)
        {
            SwApp = ThisSW as ISldWorks;
            Cookie = cookie;

            //Setup callbacks
            SwApp.SetAddinCallbackInfo(0, this, Cookie);

            CmdMgr = SwApp.GetCommandManager(this.Cookie);

            try
            {
                addUI();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return false;
            }

            return true;
        }

        public bool DisconnectFromSW()
        {
            try
            {
                removeUI();
            }
            catch (Exception e)
            {

            }

            System.Runtime.InteropServices.Marshal.ReleaseComObject(SwApp);
            SwApp = null;
            //The addin _must_ call GC.Collect() here in order to retrieve all managed code pointers 
            GC.Collect();
            GC.WaitForPendingFinalizers();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            return true;
        }

        #endregion

        #region Private

        void addUI()
        {

            string hint = "Uploads the model to Sketchfab";
            string callback = "PublishCmd_Callback";
            string enable_mthd = "Publish_Enable";
            int position = 16; // Before 'Publish to 3DVia.com...'
            int showInDocumentType = (int)swDocumentTypes_e.swDocPART;

            string file_path = bitmapHandler.CreateFileFromResourceBitmap("SketchfabExporterforSW.sketchfab.bmp", this.GetType().Assembly);



            SwApp.AddMenuItem4(
                showInDocumentType,
                Cookie,
                PublishMenuTitle,
                position,
                callback,
                enable_mthd,
                hint,
                file_path);

        }

        void removeUI()
        {
            int showInDocumentType = (int)swDocumentTypes_e.swDocPART;
            string callback = "PublishCmd_Callback";
            SwApp.RemoveMenu(showInDocumentType, PublishMenuTitle, callback);
            if (bitmapHandler != null)
                bitmapHandler.CleanFiles();
        }

        private string PublishMenuTitle
        {
            get
            {
                string file_menu = SwApp.GetLocalizedMenuName((int)swMenuIdentifiers_e.swFileMenu);
                string sketchfab_menu = @"Publish to Sketchfa&b.com";

                return sketchfab_menu + "@" + file_menu;
            }
        }
        public void PublishCmd_Callback()
        {
            try
            {
                string tmp_file_path = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
                File.Delete(tmp_file_path);
                tmp_file_path = Path.ChangeExtension(tmp_file_path, ".stl");

                int errors = 0;
                int warnings = 0;
                bool isOK = SwApp.IActiveDoc2.Extension.SaveAs(
                    tmp_file_path,
                    (int)swSaveAsVersion_e.swSaveAsCurrentVersion,
                    (int)swSaveAsOptions_e.swSaveAsOptions_Silent,
                    null,
                    ref errors,
                    ref warnings);

                File.Delete(tmp_file_path);
            }
            catch
            { }

        }

        string getSWVersion()
        {
            string version = SwApp.RevisionNumber();

            if (version.StartsWith("17"))
                return "2009";
            else if (version.StartsWith("18"))
                return "2010";
            else if (version.StartsWith("19"))
                return "2011";
            else
                throw new Exception("Unsupported SW version");
        }

        #endregion
    }
}
