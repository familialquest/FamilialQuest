using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommonTypes
{
    public class Mail
    {
        public string MessageType
        {
            get
            {
                return _messageType;
            }
            set
            {
                if (value != null)
                {
                    _messageType = value;
                }
                else
                {
                    _messageType = string.Empty;
                }
            }
        }
        public string Address
        {
            get
            {
                return _address;
            }
            set
            {
                if (value != null)
                {
                    _address = value;
                }
                else
                {
                    _address = string.Empty;
                }
            }
        }
        public string ConfirmCode
        {
            get
            {
                return _confirmCode;
            }
            set
            {
                if (value != null)
                {
                    _confirmCode = value;
                }
                else
                {
                    _confirmCode = string.Empty;
                }
            }
        }

        private string _messageType;
        private string _address;
        private string _confirmCode;

        public Mail()
        {

        }
    }
}
