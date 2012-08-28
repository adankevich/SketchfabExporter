// editmsi.js <msi-file>
// Performs a post-build fixup of an msi to change all components to be 64bit

// Constant values from Windows Installer
var msiOpenDatabaseModeTransact = 1;

var msiViewModifyInsert         = 1
var msiViewModifyUpdate         = 2
var msiViewModifyAssign         = 3
var msiViewModifyReplace        = 4
var msiViewModifyDelete         = 6

var msidbCustomActionTypeInScript       = 0x00000400;
var msidbCustomActionTypeNoImpersonate  = 0x00000800

//var WshShell = WScript.CreateObject("WScript.Shell");

if (WScript.Arguments.Length != 1)
{
    WScript.StdErr.WriteLine(WScript.ScriptName + " file");
    WScript.Quit(1);
}

var filespec = WScript.Arguments(0);
var installer = WScript.CreateObject("WindowsInstaller.Installer");
var database = installer.OpenDatabase(filespec, msiOpenDatabaseModeTransact);

var sql
var view
var record

try
{
    sql = "SELECT Attributes FROM Component";
    view = database.OpenView(sql);
    view.Execute();
    record = view.Fetch();
    while (record)
    {
        if ( ! ( record.IntegerData (1) & 0x100 ) )
        {
            //WshShell.Popup ( "Before: " + record.IntegerData (1) );
            record.IntegerData (1) = record.IntegerData (1) | 0x100;
            view.Modify ( msiViewModifyReplace, record );
            //WshShell.Popup ( "After: " + record.IntegerData (1) );
        }
        record = view.Fetch();
    }

    view.Close();
    database.Commit();
}
catch(e)
{
    WScript.StdErr.WriteLine(e);
    WScript.Quit(1);
}
