using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.SingleWalletServer.Service
{
    [Access(Name = "single-wallet", IsPublic = false)]
    public class SingleWalletService
    {
        [Access(Name = "debit-for-placing-bet")]
        public async Task DebitForBetting(RequestContext ctx)
        {
        }

        [Access(Name = "credit-for-settling-bet")]
        public async Task CreditForBetting(RequestContext ctx)
        {
        }
    }
}
