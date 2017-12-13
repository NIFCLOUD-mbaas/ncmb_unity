# Color
ncolors=$(tput colors)
if test -n "$ncolors" && test $ncolors -ge 8; then
    bold="$(tput bold)"
    standout="$(tput smso)"
    normal="$(tput sgr0)"
    red="$(tput setaf 1)"
    green="$(tput setaf 2)"
fi
# nmcb_unity folder
root=$(pwd)
# Unity command path
unity_command=/Applications/Unity/Unity.app/Contents/MacOS/Unity

# Set project path
project_path=$root/ncmb_unity
# Log file path
log_file=$root/TravisScripts/unity_build.log
# Test Runner result file 
test_result_file=$root/TravisScripts/test_runner_result.xml
# Error code
error_code=0
build_error=1
test_error=1
# Config for retry 
max_retry=2
# Build and Test count
build_count=0
test_count=0


# UNITY BUILD WITH RETRY 
while [[ $build_count -lt $((max_retry+1)) && $build_error != 0 ]]
do
echo "${bold}${green}* Building project for Mac OS.${normal}"
$unity_command \
  -batchmode \
  -nographics \
  -silent-crashes \
  -logFile "$log_file" \
  -projectPath "$project_path" \
  -quit
if [ $? = 0 ] ; then
  echo "${bold}${green}* Building Mac OS completed successfully${normal}" 
  build_error=0
else
  echo "${bold}${red}* Building Mac OS failed with $?${normal}"
  build_error=1
fi

if [ $build_count -lt $((max_retry+1)) ]; then
  build_count=$((build_count+1))
fi

done
echo "* Unity build log"
cat $log_file 

if [ $build_error != 0 ]; then
  echo "______________________________________________________________________"
  echo "${bold}${red}x Building Mac OS completed failed. Retry: $((build_count-1))${normal}"
  echo "${bold}${red}x Please fix MacOS Building before running Test Runner${normal}"
  exit $build_error
fi

# TEST RUNNER WITH RETRY 
while [[ $test_count -lt $((max_retry+1)) && $test_error != 0 ]]
do
echo "${bold}${green}* Execute Test Runner${normal}"
$unity_command \
-batchmode \
-nographics \
-runTests \
-projectPath "$project_path" \
-testResults "$test_result_file" \
-testPlatform playmode

failed=$(echo 'cat //test-run/@failed' | xmllint --shell $test_result_file | awk -F\" 'NR % 2 == 0 { print $2 }')

if [ -n "${failed}" ] && [ $failed -gt 0 ]; then
  test_error=2
else 
  test_error=0
fi

if [ $test_count -lt $((max_retry+1)) ]; then
  test_count=$((test_count+1))
fi
done

echo '${bold}${green}* Test Runner result${normal}'
cat $test_result_file 

total=$(echo 'cat //test-run/@total' | xmllint --shell $test_result_file | awk -F\" 'NR % 2 == 0 { print $2 }')
passed=$(echo 'cat //test-run/@passed' | xmllint --shell $test_result_file | awk -F\" 'NR % 2 == 0 { print $2 }')
failed=$(echo 'cat //test-run/@failed' | xmllint --shell $test_result_file | awk -F\" 'NR % 2 == 0 { print $2 }')

if [[ -z "${total}" ]]; then
  test_error=3
fi

echo -e "\n\n${bold}${green}o Building Mac OS completed successfully.${normal} Retry: $((build_count-1))"
case "$test_error" in
0)  echo "${bold}${green}o Test Runner completed successfully [ Total:$total  Passed:$passed Failed:$failed ].${normal} Retry: $((test_count-1))"
    ;;
2)  echo "${bold}${red}x Test Runner completed failed [ Total:$total  Passed:$passed Failed:$failed ].${normal} Retry: $((test_count-1))"
    ;;
3)  echo "${bold}${red}x Test Runner completed failed. Can not read xml result file${normal}"
    ;;
esac
exit $test_error
