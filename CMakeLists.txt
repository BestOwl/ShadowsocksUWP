cmake_minimum_required(VERSION 3.8)
project(Shadowsocks CSharp)

add_subdirectory(badvpn-winrt)

FILE(TO_NATIVE_PATH "${CMAKE_CURRENT_SOURCE_DIR}" DOS_STYLE_SOURCE_DIR)
include_external_msproject(ShadowsocksUWP ${DOS_STYLE_SOURCE_DIR}\\ShadowsocksUWP\\ShadowsocksUWP.csproj)
include_external_msproject(ShadowsocksBG ${DOS_STYLE_SOURCE_DIR}\\ShadowsocksBG\\ShadowsocksBG.csproj)
