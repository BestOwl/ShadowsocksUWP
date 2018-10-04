# Shadowsocks client for UWP
[![telegram_group](https://img.shields.io/badge/chat%20on-telegram%20group-blue.svg)](https://telegram.me/ytflow)
>If you want to keep a secret, you must also hide it from yourself.

### Features
NOTE: This project is working in progress, some features are no implemented yet.

- [x] TCP relay
- [ ] UDP relay
- [ ] Stream cipher
  - [x] aes-xxx-cfb
  - [x] aes-xxx-ctr
  - [ ] chacha20-ietf
  - [ ] camellia-xxx-cfb
- [ ] AEAD cipher
- [ ] Profile switching
- [ ] QR code profile sharing
- [ ] PAC configuration
 
### Geting started
###### Requirements
 * Windows 10 Version 1703 or higher
 * Visual Studio 2017 (higher than 15.7) with UWP and C++ Development workload
###### Build
 1. Clone repo and initialize submodules <br/>
 `git clone --recursive https://github.com/BestOwl/ShadowsocksUWP.git`
 2. Open `ShadowsocksUWP.sln`
 3. Build ShadowsocksUWP solution
 
 ### References
 * https://github.com/shadowsocks/shadowsocks-windows/issues/1177
 * https://github.com/shadowsocks/shadowsocks-windows/issues/862
 * https://github.com/YtFlow/YtFlowTunnel
 * https://github.com/YtFlow/Wintun2socks
 * https://github.com/Noisyfox/ShadowsocksUWP
