using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.DataAccessServer.TransData
{
    [Access(Name = "transaction-data", IsPublic = false)]
    public class TransactionDataService
    {
        [Access(Name = "create-debit")]
        public async Task CreateDebit(RequestContext ctx)
        {
        }

        [Access(Name = "update-debit")]
        public async Task UpdateDebit(RequestContext ctx)
        {
        }

        [Access(Name = "create-credit")]
        public async Task CreateCredit(RequestContext ctx)
        {
        }

        [Access(Name = "update-credit")]
        public async Task UpdateCredit(RequestContext ctx)
        {
        }
    }
}
