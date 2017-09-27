# This link changes from time to time.
echo 'Downloading from http://netstorage.unity3d.com/unity/472613c02cf7/MacEditorInstaller/Unity-2017.1.0f3.pkg: '
curl -o Unity.pkg http://netstorage.unity3d.com/unity/472613c02cf7/MacEditorInstaller/Unity-2017.1.0f3.pkg
echo 'Installing Unity.pkg'
sudo installer -dumplog -package Unity.pkg -target /