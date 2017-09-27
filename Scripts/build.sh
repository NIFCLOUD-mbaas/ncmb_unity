# Set project path
project_path=$(pwd)/ncmb_unity
# Log file path
log_file=$(pwd)/Scripts/unity_build.log

error_code=0

echo "* Building project for Mac OS."
/Applications/Unity/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -nographics \
  -silent-crashes \
  -logFile "$log_file" \
  -projectPath "$project_path" \
  -quit
if [ $? = 0 ] ; then
  echo "* Building Mac OS completed successfully."
  error_code=0
else
  echo "* Building Mac OS failed. Exited with $?."
  error_code=1
fi
# Show log
#cat $log_file 

test_result_file=$(pwd)/Scripts/test_runner_result.xml
echo "* Execute Test Runner"
/Applications/Unity/Unity.app/Contents/MacOS/Unity \
-runTests \
-projectPath "$project_path" \
-testResults "$test_result_file" \
-testPlatform editmode

echo '* Test Runner result'
cat $test_result_file 

echo "* Finishing with code $error_code"
exit $error_code


