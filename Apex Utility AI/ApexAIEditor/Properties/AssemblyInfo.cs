using System.Reflection;
using System.Runtime.InteropServices;
using Apex.Editor.Versioning;
#if UNITY_5 || UNITY_2017
[assembly: UnityEngine.UnityAPICompatibilityVersion("5.2.3f1")]
#endif
[assembly: Apex.ApexRelevantAssembly]
[assembly: AssemblyTitle("ApexAIEditor")]
[assembly: AssemblyCompany("Apex Software")]
[assembly: AssemblyProduct("ApexAIEditor")]
[assembly: AssemblyCopyright("Copyright ©  2015")]

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("6e5b0da5-3362-4add-beb5-1c609e590d95")]
#if PersonalEdition
[assembly: ApexProduct("Apex Utility AI", "Personal Edition", "1.0.7.2", ProductType.Product, requiresLicense = false)]
#elif PerpetualEdition
[assembly: ApexProduct("Apex Utility AI", "Perpetual Edition", "1.0.7.2", ProductType.Product, requiresLicense = false)]
#else
[assembly: ApexProduct("Apex Utility AI", "1.0.7.2", ProductType.Product, requiresLicense = false)]
#endif
[assembly: AssemblyVersion("1.0.7.2")]
[assembly: AssemblyFileVersion("1.0.7.2")]
