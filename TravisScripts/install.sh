# This link changes from time to time.
echo 'Downloading from https://netstorage.unity3d.com/unity/46dda1414e51/MacEditorInstaller/Unity-2017.2.0f3.pkg: '
curl -o Unity.pkg https://netstorage.unity3d.com/unity/46dda1414e51/MacEditorInstaller/Unity-2017.2.0f3.pkg
echo 'Installing Unity.pkg'
sudo installer -dumplog -package Unity.pkg -target /
