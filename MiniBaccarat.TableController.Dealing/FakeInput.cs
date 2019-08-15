using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.TableController.Dealing
{
    public class FakeInput
    {
        static List<string> CARD_SUITS = new List<string>
        {
            "S" /* Spade (♠) */,
            "H" /* Heart (♥) */,
            "C" /* Club (♣) */,
            "D" /* Diamond (♦) */,

        };
        static List<string> CARD_VALUES = new List<string>
        {
            "A" /* Ace */,
            "2" /* 2 */,
            "3" /* 3 */,
            "4" /* 4 */,
            "5" /* 5 */,
            "6" /* 6 */,
            "7" /* 7 */,
            "8" /* 8 */,
            "9" /* 9 */,
            "T" /* 10 */,
            "J" /* Jack */,
            "Q" /* Queen */,
            "K" /* King */,

        };

        static CommonRng m_Rng = new CommonRng();

        public static string GenCard()
        {
            return CARD_VALUES[m_Rng.Next(CARD_VALUES.Count)] + CARD_SUITS[m_Rng.Next(CARD_SUITS.Count)];
        }
    }
}
