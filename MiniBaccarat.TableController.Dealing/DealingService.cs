using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.TableController.Dealing
{
    [Access(Name = "dealing", IsPublic = false)]
    public class DealingService
    {
        [Access(Name = "cards")]
        public async Task DealingCards(RequestContext ctx)
        {
            //System.Diagnostics.Debugger.Break();

            string cardCount = ctx.Data.ToString();
            int count = 0;
            if (!Int32.TryParse(cardCount, out count) || count <= 0 || count >= 5)
            {
                await ctx.Session.Send("Invalid param");
                return;
            }

            List<string> cards = new List<string>();
            for (int i = 0; i < count; i++) cards.Add(FakeInput.GenCard());

            string result = String.Join(",", cards.ToArray());

            await ctx.Session.Send(result);
        }
    }
}
