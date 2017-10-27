# This link changes from time to time.
echo 'Downloading from https://netstorage.unity3d.com/unity/cc85bf6a8a04/MacEditorInstaller/Unity-2017.1.2f1.pkg: '
curl -o Unity.pkg https://netstorage.unity3d.com/unity/cc85bf6a8a04/MacEditorInstaller/Unity-2017.1.2f1.pkg
echo 'Installing Unity.pkg'
sudo installer -dumplog -package Unity.pkg -target /