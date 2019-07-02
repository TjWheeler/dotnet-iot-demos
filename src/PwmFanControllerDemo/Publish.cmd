@echo off
dotnet publish -r linux-arm /p:ShowLinkerSizeComparison=true 
pushd .\bin\Debug\netcoreapp2.2\linux-arm\publish
rem Update the commands with your password and device name. Also make sure the /home/pi/Desktop/pwmfancontrollerdemo folder exists on the Pi.
pscp -pw "pi" -v -r .\*.* pi@raspberrypi:/home/pi/Desktop/pwmfancontrollerdemo
rem use the following command instead to copy only the project files which is much quicker
rem pscp -pw "pi" -v -r .\pwmfancontrollerdemo.* pi@raspberrypi:/home/pi/Desktop/pwmfancontrollerdemo
popd