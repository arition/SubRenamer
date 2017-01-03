using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubRenamer
{
    internal static class Utils
    {
        public static int TestSimilarity(string texta, string textb)
        {
            if (texta.Length > textb.Length)
            {
                var temp = texta;
                texta = textb;
                textb = temp;
            }
            var data = new int[texta.Length, textb.Length];
            for (var i = 0; i < texta.Length; i++)
            {
                if (texta[i] == textb[0])
                {
                    data[i, 0] = 1;
                }
                else
                {
                    data[i, 0] = 0;
                }
            }
            for (var i = 0; i < texta.Length; i++)
            {
                for (var j = 1; j < textb.Length; j++)
                {
                    if ((i + data[i, j - 1]) < texta.Length && texta[i + data[i, j - 1]] == textb[j])
                    {
                        data[i, j] = data[i, j - 1] + 1;
                    }
                }
            }
            var maxScore = 0;
            for (var i = 0; i < texta.Length; i++)
            {
                for (var j = 1; j < textb.Length; j++)
                {
                    maxScore = Math.Max(data[i, j], maxScore);
                }
            }
            var score = Convert.ToSingle(maxScore);
            return Math.Abs(score) < 0.01 ? 0 : Convert.ToInt32((score / texta.Length) * 100);
        }
    }
}
