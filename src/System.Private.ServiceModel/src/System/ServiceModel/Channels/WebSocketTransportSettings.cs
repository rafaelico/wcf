// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime;
using System.Threading;

namespace System.ServiceModel.Channels
{
    public sealed class WebSocketTransportSettings : IEquatable<WebSocketTransportSettings>
    {
        public const string ConnectionOpenedAction = "http://schemas.microsoft.com/2011/02/session/onopen";
        public const string BinaryMessageReceivedAction = "http://schemas.microsoft.com/2011/02/websockets/onbinarymessage";
        public const string TextMessageReceivedAction = "http://schemas.microsoft.com/2011/02/websockets/ontextmessage";
        public const string SoapContentTypeHeader = "soap-content-type";
        public const string BinaryEncoderTransferModeHeader = "microsoft-binary-transfer-mode";
        internal const string WebSocketMethod = "WEBSOCKET";
        internal const string SoapSubProtocol = "soap";
        internal const string TransportUsageMethodName = "TransportUsage";

        private WebSocketTransportUsage _transportUsage;
        private bool _createNotificationOnConnection;
        private TimeSpan _keepAliveInterval;
        private string _subProtocol;
        private bool _disablePayloadMasking;
        private int _maxPendingConnections;

        public WebSocketTransportSettings()
        {
            _transportUsage = WebSocketDefaults.TransportUsage;
            _createNotificationOnConnection = WebSocketDefaults.CreateNotificationOnConnection;
            _keepAliveInterval = WebSocketDefaults.DefaultKeepAliveInterval;
            _subProtocol = WebSocketDefaults.SubProtocol;
            _disablePayloadMasking = WebSocketDefaults.DisablePayloadMasking;
            _maxPendingConnections = WebSocketDefaults.DefaultMaxPendingConnections;
        }

        private WebSocketTransportSettings(WebSocketTransportSettings settings)
        {
            Fx.Assert(settings != null, "settings should not be null.");
            this.TransportUsage = settings.TransportUsage;
            this.SubProtocol = settings.SubProtocol;
            this.KeepAliveInterval = settings.KeepAliveInterval;
            this.DisablePayloadMasking = settings.DisablePayloadMasking;
            this.CreateNotificationOnConnection = settings.CreateNotificationOnConnection;
            this.MaxPendingConnections = settings.MaxPendingConnections;
        }

        [DefaultValue(WebSocketDefaults.TransportUsage)]
        public WebSocketTransportUsage TransportUsage
        {
            get
            {
                return _transportUsage;
            }

            set
            {
                WebSocketTransportUsageHelper.Validate(value);
                _transportUsage = value;
            }
        }

        [DefaultValue(WebSocketDefaults.CreateNotificationOnConnection)]
        public bool CreateNotificationOnConnection
        {
            get
            {
                return _createNotificationOnConnection;
            }

            set
            {
                _createNotificationOnConnection = value;
            }
        }

        [DefaultValue(typeof(TimeSpan), WebSocketDefaults.DefaultKeepAliveIntervalString)]
        public TimeSpan KeepAliveInterval
        {
            get
            {
                return _keepAliveInterval;
            }

            set
            {
                if (value < TimeSpan.Zero && value != Timeout.InfiniteTimeSpan)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException(
                                "value",
                                value,
                                SR.SFxTimeoutOutOfRange0));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException(
                                            "value",
                                            value,
                                            SR.SFxTimeoutOutOfRangeTooBig));
                }

                _keepAliveInterval = value;
            }
        }

        [DefaultValue(WebSocketDefaults.SubProtocol)]
        public string SubProtocol
        {
            get
            {
                return _subProtocol;
            }

            set
            {
                if (value != null)
                {
                    if (value == string.Empty)
                    {
                        throw FxTrace.Exception.Argument("value", SR.WebSocketInvalidProtocolEmptySubprotocolString);
                    }

                    if (value.Split(WebSocketHelper.ProtocolSeparators).Length > 1)
                    {
                        throw FxTrace.Exception.Argument("value", SR.Format(SR.WebSocketInvalidProtocolContainsMultipleSubProtocolString, value));
                    }

                    string invalidChar;
                    if (WebSocketHelper.IsSubProtocolInvalid(value, out invalidChar))
                    {
                        throw FxTrace.Exception.Argument("value", SR.Format(SR.WebSocketInvalidProtocolInvalidCharInProtocolString, value, invalidChar));
                    }
                }

                _subProtocol = value;
            }
        }

        [DefaultValue(WebSocketDefaults.DisablePayloadMasking)]
        public bool DisablePayloadMasking
        {
            get
            {
                return _disablePayloadMasking;
            }

            set
            {
                _disablePayloadMasking = value;
            }
        }

        [DefaultValue(WebSocketDefaults.DefaultMaxPendingConnections)]
        public int MaxPendingConnections
        {
            get
            {
                return _maxPendingConnections;
            }

            set
            {
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException(
                        "value",
                        value,
                        SR.ValueMustBePositive));
                }

                _maxPendingConnections = value;
            }
        }

        public bool Equals(WebSocketTransportSettings other)
        {
            if (other == null)
            {
                return false;
            }

            return this.TransportUsage == other.TransportUsage
                && this.CreateNotificationOnConnection == other.CreateNotificationOnConnection
                && this.KeepAliveInterval == other.KeepAliveInterval
                && this.DisablePayloadMasking == other.DisablePayloadMasking
                && StringComparer.OrdinalIgnoreCase.Compare(this.SubProtocol, other.SubProtocol) == 0
                && this.MaxPendingConnections == other.MaxPendingConnections;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return base.Equals(obj);
            }

            WebSocketTransportSettings settings = obj as WebSocketTransportSettings;
            return this.Equals(settings);
        }

        public override int GetHashCode()
        {
            int hashcode = this.TransportUsage.GetHashCode()
                        ^ this.CreateNotificationOnConnection.GetHashCode()
                        ^ this.KeepAliveInterval.GetHashCode()
                        ^ this.DisablePayloadMasking.GetHashCode()
                        ^ this.MaxPendingConnections.GetHashCode();
            if (this.SubProtocol != null)
            {
                hashcode ^= this.SubProtocol.ToLowerInvariant().GetHashCode();
            }

            return hashcode;
        }

        internal WebSocketTransportSettings Clone()
        {
            return new WebSocketTransportSettings(this);
        }
    }
}
