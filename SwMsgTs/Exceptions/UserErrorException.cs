using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msg.SwMsgTs.Exceptions {
    public class UserErrorException : Exception {
        public UserErrorException(string error) : base(error) {
        }
    }
}
