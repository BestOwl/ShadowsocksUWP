#include "pch.h"
#include "TcpSocket.h"
#include <exception>

namespace PC = Platform::Collections;

namespace Wintun2socks {
	PC::UnorderedMap<int, TcpSocket^>^ TcpSocket::m_socketmap = ref new PC::UnorderedMap<int, TcpSocket^>();

	err_t(__stdcall TcpSocket::tcp_recv_func) (void* arg, tcp_pcb *tpcb, pbuf *p, err_t err) {
		TcpSocket^ socket;
		try {
			socket = TcpSocket::m_socketmap->Lookup(*(int*)arg);
			;
		}
		catch (Platform::OutOfBoundsException^) {
			pbuf_free(p);
			tcp_abort(tpcb);
			return ERR_ABRT;
		}
		if (!socket) {
			tcp_abort(tpcb);
			return ERR_ABRT;
		}
		if (p == NULL) {
			auto arr = ref new Platform::Array<uint8, 1>(0);
			socket->DataReceived(socket, arr);
			return ERR_OK;
		}
		auto arr = ref new Platform::Array<uint8, 1>(p->tot_len);
		pbuf_copy_partial(p, arr->begin(), p->tot_len, 0);
		socket->DataReceived(socket, arr);
		tcp_recved(tpcb, p->tot_len);
		pbuf_free(p);
		return ERR_OK;
	}
	err_t(__stdcall TcpSocket::tcp_sent_func) (void* arg, tcp_pcb *tpcb, u16_t len) {
		if (arg == NULL) {
			tcp_abort(tpcb);
			return ERR_ABRT;
		}
		TcpSocket^ socket;
		try {
			socket = TcpSocket::m_socketmap->Lookup(*(int*)arg);
		}
		catch (Platform::OutOfBoundsException^) {
			tcp_abort(tpcb);
			return ERR_ABRT;
		}
		if (!socket) {
			tcp_abort(tpcb);
			return ERR_ABRT;
		}
		socket->DataSent(socket, len);
		return ERR_OK;
	}
	err_t(__stdcall TcpSocket::tcpAcceptFn) (void *arg, struct tcp_pcb *newpcb, err_t err) {
		TcpSocket^ newSocket = ref new TcpSocket(newpcb);
		TcpSocket::EstablishedTcp(newSocket);

		return ERR_OK;
	}
	err_t(__stdcall TcpSocket::tcp_err_func) (void *arg, err_t err) {
		if (arg == NULL) return ERR_OK;
		TcpSocket^ socket;
		try {
			socket = TcpSocket::m_socketmap->Lookup(*(int*)arg);
		}
		catch (Platform::OutOfBoundsException^) {
			return ERR_ABRT;
		}
		socket->SocketError(socket, err);
		return ERR_OK;
	}

	TcpSocket::TcpSocket(tcp_pcb *tpcb)
	{
		m_tcpb = tpcb;
		auto arg = (int*)malloc(sizeof(int));
		*arg = Random::Getone();
		tcp_arg(tpcb, arg);
		tcp_recv(tpcb, (tcp_recv_fn)&tcp_recv_func);
		tcp_sent(tpcb, (tcp_sent_fn)&tcp_sent_func);
		tcp_err(tpcb, (tcp_err_fn)&tcp_err_func);

		RemoteAddr = tpcb->local_ip.addr;
		RemotePort = tpcb->local_port;

		TcpSocket::m_socketmap->Insert(*arg, this);
	}

	uint8 TcpSocket::Send(const Platform::Array<uint8, 1u>^ packet)
	{
		auto ret = tcp_write(m_tcpb, packet->begin(), packet->Length, TCP_WRITE_FLAG_COPY);
		if (ret == ERR_MEM) {
			return ERR_MEM;
		}
		if (ret == ERR_OK) {
			return tcp_output(m_tcpb);
		}
		else {
			return this->Close();
		}
	}
	uint8 TcpSocket::Close()
	{
		if (m_released) return -1;
		m_released = true;
		int* arg = (int*)m_tcpb->callback_arg;
		if (arg == NULL) return ERR_CLSD;
		try {
			TcpSocket::m_socketmap->Remove(*(int*)arg);
			free(arg);
		}
		catch (Platform::OutOfBoundsException^) {
			;
		}
		tcp_arg(m_tcpb, NULL);
		tcp_recv(m_tcpb, NULL);
		tcp_sent(m_tcpb, NULL);
		tcp_err(m_tcpb, NULL);
		return tcp_close(m_tcpb);
	}
	void TcpSocket::Abort()
	{
		if (m_released) return;
		m_released = true;
		int* arg = (int*)m_tcpb->callback_arg;
		if (arg == NULL) return;
		try {
			TcpSocket::m_socketmap->Remove(*(int*)arg);
			free(arg);
		}
		catch (Platform::OutOfBoundsException^) {
			;
		};
		tcp_arg(m_tcpb, NULL);
		tcp_recv(m_tcpb, NULL);
		tcp_sent(m_tcpb, NULL);
		tcp_err(m_tcpb, NULL);
		tcp_abort(m_tcpb);
	}
	TcpSocket::~TcpSocket()
	{
		this->Close();
	}
	unsigned int TcpSocket::ConnectionCount() {
		return m_socketmap->Size;
	}
}
