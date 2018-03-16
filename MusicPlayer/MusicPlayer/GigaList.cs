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

        public GigaFloatList()
        {
            List = new List<float>[1000];
            List[0] = new List<float>(67108863);
            for (int i = 0; i < List.Length; i++)
                List[i] = new List<float>(1);
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
        public long Count
        {
            get
            {
                long i = 0;
                foreach (List<float> L in List)
                    i += L.Count;
                return i;
            }
        }
    }
}
