using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicPlayer
{
    public class GigaFloatList
    {
        List<float>[] List;
        const long LIST_LENGTH = 67108863;
        long count;
        public long Count
        {
            get
            {
                return count;
            }
        }

        public GigaFloatList()
        {
            List = new List<float>[1000];
            List[0] = new List<float>((int)LIST_LENGTH);
            for (int i = 0; i < List.Length; i++)
                List[i] = new List<float>(1);
            count = 0;
        }

        public void Add(float item)
        {
            int ListIndex = 0;
            foreach (List<float> L in List)
            {
                if (L.Count > LIST_LENGTH)
                    ListIndex++;
                else
                    break;
            }

            count++;
            List[ListIndex].Add(item);
        }
        public float Get(long index)
        {
            long ListIndex = index / LIST_LENGTH;
            long Index = index % LIST_LENGTH;
            return List[(int)ListIndex][(int)Index];
        }
        public bool Exist(long index)
        {
            long ListIndex = index / LIST_LENGTH;
            long Index = index % LIST_LENGTH;
            return List[(int)ListIndex].Count > Index;
        }
        public float[] GetRange(long startIndex, long count)
        {
            if (Exist(startIndex) && Exist(startIndex + count))
            {
                List<float> Temp = new List<float>();
                for (long i = startIndex; i < startIndex + count; i++)
                    Temp.Add(Get(i));
                return Temp.ToArray();
            }
            else
                return null;
        }
    }
}
