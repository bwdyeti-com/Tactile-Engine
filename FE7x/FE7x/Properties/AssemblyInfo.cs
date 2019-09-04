using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if __ANDROID__
using Android.App;
#endif

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("FE7x")]
[assembly: AssemblyProduct("FE7x")]
[assembly: AssemblyDescription("Fire Emblem: Immortal Sword")]
[assembly: AssemblyCompany("www.bwdyeti.com")]
[assembly: AssemblyCopyright("Copyright © bwdyeti.com 2017")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type. Only Windows
// assemblies support COM.
[assembly: ComVisible(false)]

// On Windows, the following GUID is for the ID of the typelib if this
// project is exposed to COM. On other platforms, it unique identifies the
// title storage container when deploying this assembly to the device.
[assembly: Guid("5f7740e0-39d9-4173-9aa6-a20a03931896")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("0.6.7.0")]

// Add some common permissions, these can be removed if not needed
#if __ANDROID__
[assembly: UsesPermission(Android.Manifest.Permission.Internet)]
[assembly: UsesPermission(Android.Manifest.Permission.WriteExternalStorage)]
#endif
