# Shadowsocks client for UWP
>If you want to keep a secret, you must also hide it from yourself.

NOTE: Currently only supports the SOCKS5 protocol, support for the Shadowsocks protocol will be available later.
 
### Geting startd
###### Requirements
 * Windows 10 Version 1703 or higher
 * Visual Studio 2017 (higher than 15.7) with UWP and C++ Development workload
###### Build
 1. clone this repo
 2. Initialize submodules `git submodules update`
 3. Open `CMakeLists.txt` with Visual Studio in project root
 4. Open generated soulution file `CMakeBuilds/build/${ConfigurationName}/Shadowsocks.sln`
 5. Build ShadowsocksUWP