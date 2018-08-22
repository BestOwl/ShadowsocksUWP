#pragma once
#include "pch.h"
#include "lwip\init.h"
#include "lwip\tcp.h"
#include "Random.h"
#include <collection.h>

namespace WFM = Windows::Foundation::Metadata;

namespace Wintun2socks {
	ref class TcpSocket;
	public delegate void EstablishedTcpHandler(TcpSocket^ incomingSocket);
	public delegate void DataReceivedHandler(TcpSocket^ sender, const Platform::Array<uint8, 1>^ bytes);
	public delegate void DataSentHandler(TcpSocket^ sender, u16_t length);
	public delegate void SocketErrorHandler(TcpSocket^ sender, signed int err);

	public interface class ITcpSocket {
		event DataReceivedHandler^ DataReceived;
		event DataSentHandler^ DataSent;
		event SocketErrorHandler^ SocketError;
	};
	public ref class TcpSocket sealed: [WFM::DefaultAttribute] ITcpSocket
	{
	private:
		static Platform::Collections::UnorderedMap<int, TcpSocket^>^ TcpSocket::m_socketmap;
		static err_t(__stdcall TcpSocket::tcp_recv_func) (void* arg, tcp_pcb *tpcb, pbuf *p, err_t err);
		static err_t(__stdcall TcpSocket::tcp_sent_func) (void* arg, tcp_pcb *tpcb, u16_t len);
		static err_t(__stdcall TcpSocket::tcp_err_func) (void* arg, err_t err);
		TcpSocket(tcp_pcb* pcb);
		tcp_pcb* m_tcpb;
		bool m_released;
	internal:
		static err_t(__stdcall TcpSocket::tcpAcceptFn) (void *arg, struct tcp_pcb *newpcb, err_t err);
	public:
		property u32_t TcpSocket::RemoteAddr;
		property u16_t TcpSocket::RemotePort;
		uint8 TcpSocket::Send(const Platform::Array<uint8, 1u>^ packet);
		uint8 TcpSocket::Close();
		void TcpSocket::Abort();
		virtual event DataReceivedHandler^ DataReceived;
		virtual event DataSentHandler^ DataSent;
		virtual event SocketErrorHandler^ SocketError;
		static event EstablishedTcpHandler^ EstablishedTcp;
		virtual ~TcpSocket();
		static unsigned int ConnectionCount();
	};
}
