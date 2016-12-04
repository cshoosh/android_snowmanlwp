#define DEBUG

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

enum ApplicationType
{ 
    AndroidApplication = 0, 
    AndroidLiveWallpaper = 1
}

public class AJUnity2Eclipse : EditorWindow 
{	
	string projectDirectory = Directory.GetCurrentDirectory();
    string unityDirectory = EditorApplication.applicationPath.Replace("Unity.exe", "").Replace("Unity.app", "");
	ApplicationType applicationType = ApplicationType.AndroidLiveWallpaper;
	string outputRootDirectory = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "android-eclipse";
	
    [MenuItem ("File/Eclipse export ..", false)]
    static void Init () 
	{
		#if DEBUG
			Debug.Log(string.Format("Show Window"));
		#endif
        AJUnity2Eclipse window = (AJUnity2Eclipse)EditorWindow.GetWindow (typeof (AJUnity2Eclipse));
		window.Show();
    }
	
	void OnGUI () 
	{
		#if DEBUG
			Debug.Log(string.Format("Create Window GUI"));
		#endif
		
		bool canBuild = true;
		string buttonText = "Create Eclipse project";
		string successMessage = "";
		
		GUIStyle warnStyle = new GUIStyle();
		warnStyle.fontStyle = FontStyle.Bold;
		warnStyle.normal.textColor = new Color(238.0f/255.0f, 71.0f/255.0f, 63.0f/255.0f);
		warnStyle.padding = new RectOffset(6, 6, 6, 6);
		warnStyle.wordWrap = true;
		
		GUIStyle warnStyleNoBold = new GUIStyle();
		warnStyleNoBold.normal.textColor = new Color(238.0f/255.0f, 71.0f/255.0f, 63.0f/255.0f);
		warnStyleNoBold.padding = new RectOffset(6, 6, 6, 6);
		warnStyleNoBold.wordWrap = true;
		
		GUIStyle successStyle = new GUIStyle();
		successStyle.fontStyle = FontStyle.Bold;
		successStyle.wordWrap = true;
		successStyle.normal.textColor = new Color(47.0f/255.0f, 142.0f/255.0f, 62.0f/255.0f);
		successStyle.padding = new RectOffset(6, 6, 6, 6);
		
		string pathStagingArea = projectDirectory + Path.DirectorySeparatorChar + "Temp" + Path.DirectorySeparatorChar + "StagingArea";
		if ( !Directory.Exists(pathStagingArea + Path.DirectorySeparatorChar + "assets") )
		{
			canBuild = false;
			GUILayout.Label("Please go to File -> Build settings and make an Android-build first!", warnStyle);
		}
		else if ( Directory.Exists(outputRootDirectory) )
		{
			buttonText = "Update Eclipse project";
			GUILayout.Label("Existing Eclipse project found! The project will be updated with your latest Unity3d scripts and assets. Your modifications to the Eclipse project will be preserved!", successStyle);
			successMessage = "Your Eclipse project has been updated!";
		}
		else
		{
			GUILayout.Label("Ready to create Eclipse Android project.", successStyle);
			successMessage = "Eclipse export was successfull!\nOpen Eclipse, go to\n\nFile -> Import -> General -> Existing project into workspace\n\nand navigate to the newly created output directory. Two projects will show up. Import both of them, leaving the 'copy into workspace'-option unchecked.\n\nYou are now ready to build the Android project in Eclipse!";
		}
		
        GUILayout.Label ("Path configuration", EditorStyles.boldLabel);
		
        projectDirectory = EditorGUILayout.TextField("Project directory", projectDirectory);
		unityDirectory = EditorGUILayout.TextField("Unity directory", unityDirectory);
		
		GUILayout.Label ("Output", EditorStyles.boldLabel);
		
		outputRootDirectory = EditorGUILayout.TextField("Output directory", outputRootDirectory);
		
		bool lwpSupport = Application.unityVersion.StartsWith("3.4");
		
		GUILayout.Label ("Project type", EditorStyles.boldLabel);
		
		if ( Directory.Exists(outputRootDirectory + Path.DirectorySeparatorChar + PlayerSettings.productName) )
			GUI.enabled = false;
		
		if ( !lwpSupport )
		{
			GUILayout.Label("Android Live Wallpaper projects are only supported in Unity3d version 3.4.", warnStyleNoBold);
			GUI.enabled = false;
			applicationType = ApplicationType.AndroidApplication;
		}
		applicationType = (ApplicationType)EditorGUILayout.EnumPopup("Application type", applicationType);
		
		GUI.enabled = canBuild;
        
		EditorGUILayout.Space();

		if (GUILayout.Button(buttonText))
		{
			// Step 1 : check if StagingArea exists
			if ( !Directory.Exists(pathStagingArea) )
			{
				Debug.LogError("StagingArea not found at: '"+pathStagingArea+"'. Did you make an Android build?");
				return;
			}
			
			#if DEBUG
				Debug.Log(string.Format("Found temporary staging area at: '{0}'", pathStagingArea));
			#endif
			
			if ( Directory.Exists(outputRootDirectory) )
			{
				string existingAssetsFolder = outputRootDirectory + Path.DirectorySeparatorChar + PlayerSettings.productName + Path.DirectorySeparatorChar + "assets";
				string updatedAssetsFolder = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "Temp" + Path.DirectorySeparatorChar + "StagingArea" + Path.DirectorySeparatorChar + "assets";
				
				#if DEBUG
					Debug.Log(string.Format("Updating existing assets folder at: '{0}' with '{1}'", existingAssetsFolder, updatedAssetsFolder));
				#endif
				
				DeleteDirectory(existingAssetsFolder);
				Directory.Move(updatedAssetsFolder, existingAssetsFolder);
			}
			else
			{
				#if DEBUG
					Debug.Log(string.Format("Creating output directory: '{0}'", outputRootDirectory));
				#endif
				Directory.CreateDirectory(outputRootDirectory);
				
				// Step 2 : move StagingArea to outputRootDirectory and rename it
				string eclipseLibraryProjectPath = outputRootDirectory + Path.DirectorySeparatorChar + PlayerSettings.productName + "Library";
				#if DEBUG
					Debug.Log(string.Format("Moving staging area from '{0}' to '{1}'", pathStagingArea, eclipseLibraryProjectPath));
				#endif
				MoveDirectory(pathStagingArea, eclipseLibraryProjectPath);
				
				// Step 3 : determine Android SDK version
				int minSdkVersion = -1;
				int targetSdkVersion = -1;
				FindAndroidSdkVersions(eclipseLibraryProjectPath + Path.DirectorySeparatorChar + "AndroidManifest.xml", ref minSdkVersion, ref targetSdkVersion);
				
				#if DEBUG
					Debug.Log(string.Format("Android minSdkVersion={0}, targetSdkVersion={1}", minSdkVersion, targetSdkVersion));
				#endif
				
				// Step 4 : extract zip
				string ZipName;
				string FolderName;
				if ( applicationType == ApplicationType.AndroidLiveWallpaper )
				{
					ZipName = "Data.2.zip";
					FolderName = "LiveWallpaper";
				}
				else
				{
					ZipName = "Data.1.zip";
					FolderName = "StandAlone";
				}
				#if DEBUG
					Debug.Log(string.Format("Extracting {0} to '{1}'", ZipName, outputRootDirectory));
				#endif
				ExtractZip("Assets/AJUnity2Eclipse/Resources/" + ZipName, outputRootDirectory);
				
				// Step 5 : rename outputRootDirectory/YourProjectName				
				string eclipseProjectPath = outputRootDirectory + Path.DirectorySeparatorChar + PlayerSettings.productName;
				string extractedPath = outputRootDirectory + Path.DirectorySeparatorChar + FolderName;

				#if DEBUG
					Debug.Log(string.Format("Moving eclipse project directory from '{0}' to '{1}'", extractedPath, eclipseProjectPath));
				#endif
				
				MoveDirectory(extractedPath, eclipseProjectPath);
				
				// Step 6 : create project.properies
				string pathProjectProperties = eclipseLibraryProjectPath + Path.DirectorySeparatorChar + "project.properties";
				
				#if DEBUG
					Debug.Log(string.Format("Writing library project project.properties to '{0}'", pathProjectProperties));
				#endif
				
				File.WriteAllText(pathProjectProperties, string.Format("target=android-{0}\nandroid.library=true", targetSdkVersion));
				
				// Step 7 : move Assets to eclipse project
				string oldAssetsLocation = eclipseLibraryProjectPath + Path.DirectorySeparatorChar + "assets";
				string newAssetsLocation = eclipseProjectPath + Path.DirectorySeparatorChar + "assets";
				
				#if DEBUG
					Debug.Log(string.Format("Moving assets from '{0}' to '{1}'", oldAssetsLocation, newAssetsLocation));
				#endif
				
				MoveDirectory(oldAssetsLocation, newAssetsLocation);
				
				// Step 8 : copy classes.jar
				string classesJarPathComponentsMac = string.Join(string.Format("{0}", Path.DirectorySeparatorChar), new string[] {unityDirectory,"Unity.app","Contents","PlaybackEngines","AndroidPlayer","bin","classes.jar"});
				string classesJarPathComponentsWin = string.Join(string.Format("{0}", Path.DirectorySeparatorChar),new string[] {unityDirectory,"Data","PlaybackEngines","AndroidPlayer","bin","classes.jar"});
				
				string oldClassesJarLocation = "";
				if ( File.Exists(classesJarPathComponentsWin) )
				{
					#if DEBUG
						Debug.Log(string.Format("Platform seems to be Windows"));
					#endif
					oldClassesJarLocation = classesJarPathComponentsWin;
				}
				else if ( File.Exists(classesJarPathComponentsMac) )
				{
					#if DEBUG
						Debug.Log(string.Format("Platform seems to be MacOS"));
					#endif
					oldClassesJarLocation = classesJarPathComponentsMac;
				}
				else
				{
					Debug.LogError("classes.jar not found!");
				}
					
				string newClassesJarLocation = eclipseProjectPath + Path.DirectorySeparatorChar + "libs" + Path.DirectorySeparatorChar + "classes.jar";
				
				#if DEBUG
					Debug.Log(string.Format("Copying classes.jar from '{0}' to '{1}'", oldClassesJarLocation, newClassesJarLocation));
				#endif
				
				CopyFile(oldClassesJarLocation, newClassesJarLocation);
				
				// Step 9 : create package structure
				string bundleIdentifier = readXmlAttribute(eclipseLibraryProjectPath + Path.DirectorySeparatorChar + "AndroidManifest.xml", "//manifest", "package");

				#if DEBUG
					Debug.Log(string.Format("BundleIdentifier is: '{0}'", bundleIdentifier));
				#endif
				
				string eclipseProjectPackagePath = eclipseProjectPath + Path.DirectorySeparatorChar + "src" + Path.DirectorySeparatorChar + bundleIdentifier.Replace(".", string.Format("{0}", Path.DirectorySeparatorChar));
				
				#if DEBUG
					Debug.Log(string.Format("Creating package path: '{0}'", eclipseProjectPackagePath));
				#endif
				
				Directory.CreateDirectory(eclipseProjectPackagePath);
				
				// Step 10 : move main class YourProjectName.java to new location
				string oldMainClassLocation = eclipseProjectPath + Path.DirectorySeparatorChar + "YourProjectName.java";
				string newMainClassLocation = eclipseProjectPackagePath + Path.DirectorySeparatorChar + PlayerSettings.productName + ".java";
				
				#if DEBUG
					Debug.Log(string.Format("Writing main class to '{0}'", newMainClassLocation));
				#endif
				
				string mainClassContents = File.ReadAllText(oldMainClassLocation);
				mainClassContents = mainClassContents.Replace("{ProductName}", PlayerSettings.productName);
				mainClassContents = mainClassContents.Replace("{PackageName}", bundleIdentifier);
				File.WriteAllText(newMainClassLocation, mainClassContents);
				
				// Step 11 : delete YourProjectName.java
				
				#if DEBUG
					Debug.Log(string.Format("Deleting main class template '{0}'", oldMainClassLocation));
				#endif
				
				File.Delete(oldMainClassLocation);
				
				// Step 12 : move library project .classpath to correct location
				string oldLibraryProjectClasspathLocation = eclipseProjectPath + Path.DirectorySeparatorChar + "library.classpath";
				string newLibraryProjectClasspathLocation = eclipseLibraryProjectPath + Path.DirectorySeparatorChar + ".classpath";
				
				#if DEBUG
					Debug.Log(string.Format("Moving library project .classpath from '{0}' to '{1}'", oldLibraryProjectClasspathLocation, newLibraryProjectClasspathLocation));
				#endif
				
				File.Move(oldLibraryProjectClasspathLocation, newLibraryProjectClasspathLocation);
				
				// Step 13 : move library project .project to correct location
				string oldLibraryProjectDotProjectLocation = eclipseProjectPath + Path.DirectorySeparatorChar + "library.project";
				string newLibraryProjectDotProjectLocation = eclipseLibraryProjectPath + Path.DirectorySeparatorChar + ".project";
				
				#if DEBUG
					Debug.Log(string.Format("Moving library project .classpath from '{0}' to '{1}'", oldLibraryProjectDotProjectLocation, newLibraryProjectDotProjectLocation));
				#endif
				
				File.Move(oldLibraryProjectDotProjectLocation, newLibraryProjectDotProjectLocation);
				
				// Step 14 : update AndroidManifest.xml (minSdkVersion, targetSdkVersion, package, name)
				string androidManifest = eclipseProjectPath + Path.DirectorySeparatorChar + "AndroidManifest.xml";
				
				#if DEBUG
					Debug.Log(string.Format("Updating AndroidManifest.xml '{0}'", androidManifest));
				#endif
				
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.Load(androidManifest);
				XmlNode node;
				
				node = xmlDoc.SelectSingleNode("//manifest");
				node.Attributes["package"].Value = bundleIdentifier;
				
				node = xmlDoc.SelectSingleNode("//manifest/uses-sdk");
				node.Attributes["android:minSdkVersion"].Value = minSdkVersion.ToString();
				node.Attributes["android:targetSdkVersion"].Value = targetSdkVersion.ToString();
				
				if ( applicationType == ApplicationType.AndroidLiveWallpaper )
					node = xmlDoc.SelectSingleNode("//manifest/application/service");
				else
					node = xmlDoc.SelectSingleNode("//manifest/application/activity");
				
				node.Attributes["android:name"].Value = "." + PlayerSettings.productName;
				
				xmlDoc.Save(androidManifest);
				
				
				// Step 15 : update .classpath
				string classpathFile = eclipseProjectPath + Path.DirectorySeparatorChar + ".classpath";
				string ajJarLocation = eclipseProjectPath + Path.DirectorySeparatorChar + "libs" + Path.DirectorySeparatorChar + "AJEclipseIntegration.jar";
				
				#if DEBUG
					Debug.Log(string.Format("Updating .classpath '{0}'", classpathFile));
				#endif
				
				XmlDocument classpathDoc = new XmlDocument();
				classpathDoc.Load(classpathFile);
				XmlNode classpathNode = classpathDoc.SelectSingleNode("//classpath");
				
				XmlAttribute kindLibAttribute1 = classpathDoc.CreateAttribute("kind");
				kindLibAttribute1.Value = "lib";
				
				XmlAttribute kindLibAttribute2 = classpathDoc.CreateAttribute("kind");
				kindLibAttribute2.Value = "lib";
				
				XmlNode classesJarNode = classpathDoc.CreateNode(XmlNodeType.Element, "classpathentry", null);
				XmlAttribute classesJarAttribute = classpathDoc.CreateAttribute("path");
				classesJarAttribute.Value = newClassesJarLocation;
				classesJarNode.Attributes.Append(classesJarAttribute);
				classesJarNode.Attributes.Append(kindLibAttribute1);
				classpathNode.AppendChild(classesJarNode);
				
				if ( applicationType == ApplicationType.AndroidLiveWallpaper )
				{
					XmlNode ajJarNode = classpathDoc.CreateNode(XmlNodeType.Element, "classpathentry", null);
					XmlAttribute ajJarAttribute = classpathDoc.CreateAttribute("path");
					ajJarAttribute.Value = ajJarLocation;
					ajJarNode.Attributes.Append(ajJarAttribute);
					ajJarNode.Attributes.Append(kindLibAttribute2);
					classpathNode.AppendChild(ajJarNode);
				}
				
				classpathDoc.Save(classpathFile);
				
				// Step 16 : update project .project
				string projectFile = eclipseProjectPath + Path.DirectorySeparatorChar + ".project";
				
				#if DEBUG
					Debug.Log(string.Format("Updating .project '{0}'", projectFile));
				#endif
				
				XmlDocument projectXmlDoc = new XmlDocument();
				projectXmlDoc.Load(projectFile);
				XmlNode projectNameNode = projectXmlDoc.SelectSingleNode("//projectDescription/name");
				projectNameNode.InnerText = PlayerSettings.productName;
				projectXmlDoc.Save(projectFile);

				// Step 17 : update library project .project
				string libraryProjectFile = eclipseLibraryProjectPath + Path.DirectorySeparatorChar + ".project";
				
				#if DEBUG
					Debug.Log(string.Format("Updating library .project '{0}'", libraryProjectFile));
				#endif
				
				XmlDocument libraryProjectXmlDoc = new XmlDocument();
				libraryProjectXmlDoc.Load(libraryProjectFile);
				XmlNode libraryProjectNameNode = libraryProjectXmlDoc.SelectSingleNode("//projectDescription/name");
				libraryProjectNameNode.InnerText = PlayerSettings.productName + "Library";
				libraryProjectXmlDoc.Save(libraryProjectFile);
				
				// Step 18 : write project.properties
				string propertiesFile = eclipseProjectPath + Path.DirectorySeparatorChar + "project.properties";
				string eclipseLibraryProjectRelativePath = ".." + Path.DirectorySeparatorChar + PlayerSettings.productName + "Library";
				
				#if DEBUG
					Debug.Log(string.Format("Writing project project.properties to '{0}'", propertiesFile));
				#endif
				
				File.WriteAllText(propertiesFile, string.Format("target=android-{0}\nandroid.library.reference.1={1}", targetSdkVersion, eclipseLibraryProjectRelativePath));
				
				// Step 19 : update strings.xml
				string stringsXmlFile = eclipseProjectPath + Path.DirectorySeparatorChar + "res" + Path.DirectorySeparatorChar + "values" + Path.DirectorySeparatorChar + "strings.xml";
				
				#if DEBUG
					Debug.Log(string.Format("Writing strings.xml to '{0}'", stringsXmlFile));
				#endif
				
				XmlDocument stringsXmlDoc = new XmlDocument();
				stringsXmlDoc.Load(stringsXmlFile);
				XmlNode stringsXmlNode = stringsXmlDoc.SelectSingleNode("//resources/string");
				stringsXmlNode.InnerText = PlayerSettings.productName;
				stringsXmlDoc.Save(stringsXmlFile);
			}
			
			Debug.Log("Eclipse export was successfull!");
			UnityEditor.EditorUtility.DisplayDialog("Eclipse export", successMessage, "OK");
		}
    }
	
	private void FindAndroidSdkVersions(string pathAndroidManifest, ref int minSdkVersion, ref int targetSdkVersion)
	{
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.Load(pathAndroidManifest);
		XmlNode node = xmlDoc.SelectSingleNode("//manifest/uses-sdk");
		minSdkVersion = int.Parse(node.Attributes["android:minSdkVersion"].Value);
		targetSdkVersion = int.Parse(node.Attributes["android:targetSdkVersion"].Value);
	}
	
	private string readXmlAttribute(string filename, string xpath, string attribute)
	{
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.Load(filename);
		XmlNode node = xmlDoc.SelectSingleNode(xpath);
		return node.Attributes[attribute].Value;
	}
	
	private void MoveDirectory(string src, string dst)
	{
		Directory.Move(src, dst);
	}
	
	private void CopyFile(string src, string dst)
	{
		File.Copy(src, dst);
	}
					
	public static void DeleteDirectory(string target_dir)
    {
        string[] files = Directory.GetFiles(target_dir);
        string[] dirs = Directory.GetDirectories(target_dir);

        foreach (string file in files)
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }

        foreach (string dir in dirs)
        {
            DeleteDirectory(dir);
        }

        Directory.Delete(target_dir, false);
    }
	
	private void ExtractZip(string src, string dst)
	{
		using (ZipInputStream s = new ZipInputStream(File.OpenRead(src))) {
		
			ZipEntry theEntry;
			while ((theEntry = s.GetNextEntry()) != null) 
			{	
				string directoryName = dst + Path.DirectorySeparatorChar + Path.GetDirectoryName(theEntry.Name);
				string fileName      = Path.GetFileName(theEntry.Name);
				
				if ( directoryName.Length > 0 ) {
					Directory.CreateDirectory(directoryName);
				}
				
				if (fileName != string.Empty) 
				{
					using (FileStream streamWriter = File.Create(directoryName + Path.DirectorySeparatorChar + fileName)) {
					
						int size = 2048;
						byte[] data = new byte[2048];
						while (true) {
							size = s.Read(data, 0, data.Length);
							if (size > 0) {
								streamWriter.Write(data, 0, size);
							} else {
								break;
							}
						}
					}
				}
			}
		}
	}
}