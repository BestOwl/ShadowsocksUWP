#pragma once
#include "lwip\init.h"
#include "lwip\tcp.h"
#include "lwip\udp.h"
#include "pch.h"
#include "TcpSocket.h"

namespace WFM = Windows::Foundation::Metadata;

namespace Wintun2socks {
	public delegate void PacketPopedHandler(Platform::Object^ sender, const Platform::Array<uint8, 1>^ e);
	public delegate void DnsPacketPopedHandler(Platform::Object^ sender, const Platform::Array<uint8, 1>^ e, u32_t addr, uint16 port);
	ref class Wintun;
	public interface class IWintun {
		event PacketPopedHandler^ PacketPoped;
		event DnsPacketPopedHandler^ DnsPacketPoped;
		void Init();
		uint8 PushDnsPayload(u32_t addr, uint16 port, const Platform::Array<uint8, 1>^ packet);
		uint8 PushPacket(const Platform::Array<uint8, 1u>^ packet);
	};
	public ref class Wintun sealed: [WFM::DefaultAttribute] IWintun
	{
	private:
		static Wintun^ m_instance;
		static netif* m_interface;
		static tcp_pcb* m_listenPCB;
		static udp_pcb* m_dnsPCB;
		static err_t(__stdcall Wintun::outputPCB) (struct netif *netif, struct pbuf *p,
			const ip4_addr_t *ipaddr);
		static err_t(__stdcall Wintun::recvUdp) (void *arg, struct udp_pcb *pcb, struct pbuf *p,
			const ip_addr_t *addr, u16_t port);

	public:
		static property Wintun^ Instance { Wintun^ get(); };
		virtual void Init();
		virtual uint8 PushPacket(const Platform::Array<uint8, 1u>^ packet);
		virtual uint8 PushDnsPayload(u32_t addr, uint16 port, const Platform::Array<uint8, 1>^ packet);
		virtual event PacketPopedHandler^ PacketPoped;
		virtual event DnsPacketPopedHandler^ DnsPacketPoped;
	};
}
