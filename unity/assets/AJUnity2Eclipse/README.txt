===================== INTRO =====================

AJUnity2Eclipse is a plugin for Unity3d that makes it easy to convert your Unity project to an Eclipse Android project. It does so by extending the Unity Editor, providing an optimal workflow. New in version 2.0: it is now also possible to create android live wallpapers from your Unity3d project (Unity3d v3.4 only).

==================== FEATURES ===================

* Create Eclipse Android projects for your Unity3d projects
* NEW: create Android Live Wallpapers from your Unity3d projects!

================== INSTALLATION =================

AJUnity2Eclipse was tested on Unity3d v3.4 and v3.5. You can install it directly from the Unity Asset Store or by dragging the AJUnity2Eclipse.unitypackage file from file system to the Unity project window. Then simply double-click AJUnity2Eclipse in your package explorer to unpack the package.

====================== USAGE ====================

This is a short guide that briefly describes the steps it takes to use the plugin. You can find more detailed instructions in our blog: 

http://blog.appjigger.com/unity-editor-plugin-for-creating-eclipse-android-projects/

Setup (this has to be done only once for each new Unity project):

 1) Open your project and make an Android build by opening the "File" -> "Build Settings .." window, choose Android from the available platforms and click "Build". Save the APK anywhere - you won't need it anymore.

 2) Go to "File" -> "Eclipse export". You can leave all settings unchanged (recommended) and click "Create Eclipse project".

 3) Open Eclipse and choose "File" -> "Import..." -> "Android" -> "Existing Android Code Into Workspace". Pick the directory you specified as output directory in step (2), the default is <Your-Unity-Project-Directory>/android-eclipse. Two projects will appear in the list box - check them both and make sure that the option "Copy projects into workspace" is _NOT_ checked. Then hit "Finish".

 4) That's it! 


Workflow (you do this every time you make changes in your Unity project and want to update the Eclipse project):

 1) Make an Android build, just like in step (1) of the setup

 2) Go to "File" -> "Build Settings ..". Given the correct output directory, the plugin automatically recognizes that you already created the Eclipse projects and asks you, if you want to update it with your latest changes in Unity. You can do this by clicking "Update Eclipse project". Note: this operation preserves any changes you made to the Eclipse project (like adding and modifying Java classes).

 3) That's it!


Troubleshooting:

In some situations you will get build errors in Eclipse (for example if you accidently selected "General" -> "Existing Projects into Workspace"). If this is the case, right click on both projects in your Eclipse package explorer and click "Android Tools" -> "Fix Project Properties"

===================== CONTACT ===================

AJUnity2Eclipse is developed by appjigger studios. If you have any suggestions or want to provide feedback, feel free to contact us or leave a comment in our blog!

http://www.appjigger.com/

http://blog.appjigger.com/unity-editor-plugin-for-creating-eclipse-android-projects/


===================== LICENSE ===================

AJUnity2Eclipse, Copyright 2012 by appjigger studios.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

AJUnity2Eclipse uses a library called SharpZipLib which is licensed under the GPL. The source code can be found on their official website:

http://www.icsharpcode.net/opensource/sharpziplib/


