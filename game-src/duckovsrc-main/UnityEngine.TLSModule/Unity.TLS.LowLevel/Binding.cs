using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.Bindings;

namespace Unity.TLS.LowLevel;

[NativeHeader("External/unitytls/builds/CSharp/BindingsUnity/TLSAgent.gen.bindings.h")]
internal static class Binding
{
	public struct unitytls_errorstate
	{
		public uint magic;

		public uint code;

		public ulong reserved;
	}

	public struct unitytls_dataRef
	{
		public unsafe byte* dataPtr;

		public UIntPtr dataLen;
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public unsafe delegate void unitytls_client_on_data_callback(IntPtr arg0, byte* arg1, UIntPtr arg2, uint arg3);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public unsafe delegate int unitytls_client_data_send_callback(IntPtr arg0, byte* arg1, UIntPtr arg2, uint arg3);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public unsafe delegate int unitytls_client_data_receive_callback(IntPtr arg0, byte* arg1, UIntPtr arg2, uint arg3);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public unsafe delegate int unitytls_client_data_receive_timeout_callback(IntPtr arg0, byte* arg1, UIntPtr arg2, uint arg3, uint arg4);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public unsafe delegate void unitytls_client_log_callback(int arg0, byte* arg1, UIntPtr arg2, byte* arg3, byte* arg4, UIntPtr arg5);

	public struct unitytls_client_config
	{
		public unitytls_dataRef caPEM;

		public unitytls_dataRef serverPEM;

		public unitytls_dataRef privateKeyPEM;

		public uint clientAuth;

		public uint transportProtocol;

		public unitytls_dataRef psk;

		public unitytls_dataRef pskIdentity;

		public IntPtr onDataCB;

		public IntPtr dataSendCB;

		public IntPtr dataReceiveCB;

		public IntPtr dataReceiveTimeoutCB;

		public IntPtr transportUserData;

		public IntPtr applicationUserData;

		public int handshakeReturnsOnStep;

		public int handshakeReturnsIfWouldBlock;

		public uint ssl_read_timeout_ms;

		public unsafe byte* hostname;

		public uint tracelevel;

		public IntPtr logCallback;

		public uint ssl_handshake_timeout_min;

		public uint ssl_handshake_timeout_max;

		public ushort mtu;
	}

	public struct unitytls_client
	{
		public uint role;

		public uint state;

		public uint handshakeState;

		public IntPtr ctx;

		public unsafe unitytls_client_config* config;

		public IntPtr internalCtx;
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public unsafe delegate int unitytls_tlsctx_handshake_on_blocking_callback(unitytls_client* arg0, IntPtr arg1, int arg2);

	public const int UNITYTLS_SUCCESS = 0;

	public const int UNITYTLS_INVALID_ARGUMENT = 1;

	public const int UNITYTLS_INVALID_FORMAT = 2;

	public const int UNITYTLS_INVALID_PASSWORD = 3;

	public const int UNITYTLS_INVALID_STATE = 4;

	public const int UNITYTLS_BUFFER_OVERFLOW = 5;

	public const int UNITYTLS_OUT_OF_MEMORY = 6;

	public const int UNITYTLS_INTERNAL_ERROR = 7;

	public const int UNITYTLS_NOT_SUPPORTED = 8;

	public const int UNITYTLS_ENTROPY_SOURCE_FAILED = 9;

	public const int UNITYTLS_STREAM_CLOSED = 10;

	public const int UNITYTLS_DER_PARSE_ERROR = 11;

	public const int UNITYTLS_KEY_PARSE_ERROR = 12;

	public const int UNITYTLS_SSL_ERROR = 13;

	public const int UNITYTLS_USER_CUSTOM_ERROR_START = 1048576;

	public const int UNITYTLS_USER_WOULD_BLOCK = 1048577;

	public const int UNITYTLS_USER_WOULD_BLOCK_READ = 1048578;

	public const int UNITYTLS_USER_WOULD_BLOCK_WRITE = 1048579;

	public const int UNITYTLS_USER_READ_FAILED = 1048580;

	public const int UNITYTLS_USER_WRITE_FAILED = 1048581;

	public const int UNITYTLS_USER_UNKNOWN_ERROR = 1048582;

	public const int UNITYTLS_SSL_NEEDS_VERIFY = 1048583;

	public const int UNITYTLS_HANDSHAKE_STEP = 1048584;

	public const int UNITYTLS_USER_CUSTOM_ERROR_END = 2097152;

	public const int UNITYTLS_LOGLEVEL_MIN = 0;

	public const int UNITYTLS_LOGLEVEL_FATAL = 0;

	public const int UNITYTLS_LOGLEVEL_ERROR = 1;

	public const int UNITYTLS_LOGLEVEL_WARN = 2;

	public const int UNITYTLS_LOGLEVEL_INFO = 3;

	public const int UNITYTLS_LOGLEVEL_DEBUG = 4;

	public const int UNITYTLS_LOGLEVEL_TRACE = 5;

	public const int UNITYTLS_LOGLEVEL_MAX = 5;

	public const int UNITYTLS_SSL_HANDSHAKE_HELLO_REQUEST = 0;

	public const int UNITYTLS_SSL_HANDSHAKE_CLIENT_HELLO = 1;

	public const int UNITYTLS_SSL_HANDSHAKE_SERVER_HELLO = 2;

	public const int UNITYTLS_SSL_HANDSHAKE_SERVER_CERTIFICATE = 3;

	public const int UNITYTLS_SSL_HANDSHAKE_SERVER_KEY_EXCHANGE = 4;

	public const int UNITYTLS_SSL_HANDSHAKE_CERTIFICATE_REQUEST = 5;

	public const int UNITYTLS_SSL_HANDSHAKE_SERVER_HELLO_DONE = 6;

	public const int UNITYTLS_SSL_HANDSHAKE_CLIENT_CERTIFICATE = 7;

	public const int UNITYTLS_SSL_HANDSHAKE_CLIENT_KEY_EXCHANGE = 8;

	public const int UNITYTLS_SSL_HANDSHAKE_CERTIFICATE_VERIFY = 9;

	public const int UNITYTLS_SSL_HANDSHAKE_CLIENT_CHANGE_CIPHER_SPEC = 10;

	public const int UNITYTLS_SSL_HANDSHAKE_CLIENT_FINISHED = 11;

	public const int UNITYTLS_SSL_HANDSHAKE_SERVER_CHANGE_CIPHER_SPEC = 12;

	public const int UNITYTLS_SSL_HANDSHAKE_SERVER_FINISHED = 13;

	public const int UNITYTLS_SSL_HANDSHAKE_FLUSH_BUFFERS = 14;

	public const int UNITYTLS_SSL_HANDSHAKE_WRAPUP = 15;

	public const int UNITYTLS_SSL_HANDSHAKE_OVER = 27;

	public const int UNITYTLS_SSL_HANDSHAKE_SERVER_NEW_SESSION_TICKET = 16;

	public const int UNITYTLS_SSL_HANDSHAKE_HELLO_VERIFY_REQUIRED = 17;

	public const int UNITYTLS_SSL_HANDSHAKE_COUNT = 28;

	public const int UNITYTLS_SSL_HANDSHAKE_BEGIN = 0;

	public const int UNITYTLS_SSL_HANDSHAKE_DONE = 27;

	public const int UNITYTLS_SSL_HANDSHAKE_HANDSHAKE_FLUSH_BUFFERS = 14;

	public const int UNITYTLS_SSL_HANDSHAKE_HANDSHAKE_WRAPUP = 15;

	public const int UNITYTLS_SSL_HANDSHAKE_HANDSHAKE_OVER = 27;

	public const int UnityTLSClientAuth_None = 0;

	public const int UnityTLSClientAuth_Optional = 1;

	public const int UnityTLSClientAuth_Required = 2;

	public const int UnityTLSRole_None = 0;

	public const int UnityTLSRole_Server = 1;

	public const int UnityTLSRole_Client = 2;

	public const int UnityTLSTransportProtocol_Stream = 0;

	public const int UnityTLSTransportProtocol_Datagram = 1;

	public const int UnityTLSClientState_None = 0;

	public const int UnityTLSClientState_Init = 1;

	public const int UnityTLSClientState_Handshake = 2;

	public const int UnityTLSClientState_Messaging = 3;

	public const int UnityTLSClientState_Fail = 64;

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public unsafe static extern uint unitytls_client_send_data(unitytls_client* clientInstance, byte* data, UIntPtr dataLen);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public unsafe static extern uint unitytls_client_read_data(unitytls_client* clientInstance, byte* buffer, UIntPtr bufferLen, UIntPtr* bytesRead);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public unsafe static extern void unitytls_client_add_ciphersuite(unitytls_client* clientInstance, uint suite);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public unsafe static extern uint unitytls_client_get_ciphersuite(unitytls_client* clientInstance, int ndx);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public unsafe static extern int unitytls_client_get_ciphersuite_cnt(unitytls_client* clientInstance);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public unsafe static extern void unitytls_client_init_config(unitytls_client_config* config);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public unsafe static extern unitytls_client* unitytls_client_create(uint role, unitytls_client_config* config);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public unsafe static extern void unitytls_client_destroy(unitytls_client* clientInstance);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public unsafe static extern int unitytls_client_init(unitytls_client* clientInstance);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public unsafe static extern uint unitytls_client_handshake(unitytls_client* clientInstance);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public unsafe static extern uint unitytls_client_set_cookie_info(unitytls_client* clientInstance, byte* peerIdDataPtr, int peerIdDataLen);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public unsafe static extern uint unitytls_client_get_handshake_state(unitytls_client* clientInstance);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public unsafe static extern uint unitytls_client_get_errorsState(unitytls_client* clientInstance, ulong* reserved);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public unsafe static extern uint unitytls_client_get_state(unitytls_client* clientInstance);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public unsafe static extern uint unitytls_client_get_role(unitytls_client* clientInstance);
}
