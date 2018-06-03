using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace assigment1
{
    public class testIndexer
    {
        
        Dictionary<string, List<string[]>> reveseIndex;
        int acc = 0;
        public string path { set; get; }
        int filenum;
        
        Dictionary<string, List<string>> AllLetter;
        public int DocCounter;
        public Dictionary<string, int> indexerDic;
        public int count1 = 0;
        public int avgdl;
        public Dictionary<string, bool> stopBigDic;
        public Dictionary<string, KeyValuePair<string, int>> Occurences;
        public int filesize, indx;
        public Dictionary<string,KeyValuePair<int[],string>> UniqueDic;
        public Dictionary<string, double[]> vectorDic;
        public testIndexer()
        {
            reveseIndex = new Dictionary<string, List<string[]>>();
            filenum = 0;
            AllLetter = new Dictionary<string, List<string>>();
            
            DocCounter = 0;
            path = "";
            indexerDic = new Dictionary<string, int>();
            stopBigDic = new Dictionary<string, bool>();
            Occurences = new Dictionary<string, KeyValuePair<string, int>>();
            vectorDic = new Dictionary<string, double[]>();
            UniqueDic = new Dictionary<string, KeyValuePair<int[], string>>();
        }


        public void UnionDic(Dictionary<string, List<string[]>> dico)
        {

            foreach (var item in dico.Keys)
            {
                if (reveseIndex.ContainsKey(item))
                {
                    int sum = Int32.Parse(reveseIndex[item][reveseIndex[item].Count - 1][2]) + Int32.Parse(dico[item][dico[item].Count - 1][2]);

                    reveseIndex[item].AddRange(dico[item]);
                    reveseIndex[item][reveseIndex[item].Count - 1][2] = sum.ToString();
                    continue;
                }
                else
                {
                    reveseIndex.Add(item, new List<string[]>());
                    reveseIndex[item].AddRange(dico[item]);
                }
            }
            acc++;
            if (acc > 9 || indx == ((filesize - 1)))
            {
                PrePosting();
                acc = 0;
            }
        }

        private void PrePosting()
        {
            filenum++;
            var l = reveseIndex.OrderBy(key => key.Key);
            var dic = l.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
            using (var stream2 = File.OpenWrite(path + @"\file" + filenum + ".txt"))
            {
                using (var writer = new StreamWriter(stream2))
                {
                    foreach (var key in dic.Keys)
                    {
                        writer.WriteLine(key);
                        for (int i = 0; i < dic[key].Count; i++)
                        {
                            writer.Write((dic[key][i][3]) + "~" + Double.Parse(dic[key][i][3]) / (double)UniqueDic[dic[key][i][0]].Key[1]+ "~" + dic[key][i][0] + "|");
                            writer.WriteLine((dic[key][i][1]));
                        }
                        writer.WriteLine("&");
                    }
                }
            }
            reveseIndex = new Dictionary<string, List<string[]>>();

        }


        public void Dump(string tpath)
        {
            using (StreamWriter file = new StreamWriter(tpath + ".txt"))
            {
                foreach (string s in AllLetter.Keys)
                {
                    
                    indexerDic.Add(s, filenum);
                    count1++;

                    file.WriteLine(s);
                    file.Write(AllLetter[s].Count + "~");

                    double idf = Math.Log10(DocCounter / AllLetter[s].Count);
                    double idf2 = Math.Log10((DocCounter - AllLetter[s].Count + 0.5) / (AllLetter[s].Count + 0.5));
                    
                    double sum = 0;
                    int firsttilda = 0;
                    int secondtilda = 0;
                    double tf = 0;
                    string doc;
                    for (int i = 0; i < AllLetter[s].Count; i++)
                    {
                        firsttilda = AllLetter[s][i].IndexOf('~');
                        secondtilda = AllLetter[s][i].IndexOf('~',firsttilda+1);
                        sum += Int32.Parse(AllLetter[s][i].Substring(0, firsttilda));
                        tf = Double.Parse(AllLetter[s][i].Substring(firsttilda + 1, secondtilda - firsttilda - 1));
                        doc = AllLetter[s][i].Substring(secondtilda + 1, AllLetter[s][i].IndexOf('|') - secondtilda - 1);
                        if (vectorDic.ContainsKey(doc))
                        {
                            vectorDic[doc][0] += tf*idf;
                            vectorDic[doc][1] += Math.Pow(tf * idf,2);
                          
                        }
                        else
                        {
                            vectorDic.Add(doc, new double[] { tf * idf, Math.Pow(tf * idf, 2)});
                        }
                    }

                    file.Write(sum + "~");
                    file.Write(idf2 + "~");
                    file.WriteLine(idf);
                    int place = 0;
                    
                    while (place < AllLetter[s].Count)
                    {
                        file.WriteLine(AllLetter[s][place]);
                        place++;
                    }
                    file.WriteLine("&");
                }
            }
            AllLetter = new Dictionary<string, List<string>>();
        }

        public void Union()
        {
            filenum = 1;
            List<int> index = new List<int>();
            double tmpfs = (double)filesize;
            double tmpDub = (double)(tmpfs / 10.0);
            int Ubound = (int)Math.Ceiling(tmpDub);
            for (int i = 0; i < Ubound; i++)
            {
                index.Add(i);
            }
            StreamReader[] filePointers = new StreamReader[(Ubound)];
            string[] terms = new string[(Ubound)];
            List<string>[] Data = new List<string>[(Ubound)];
            string line1 = "";
            int k = 0;
            while (k < index.Count)
            {
                filePointers[index[k]] = new StreamReader(path + @"\file" + (k + 1) + ".txt");
                if ((line1 = filePointers[index[k]].ReadLine()) != null)
                {
                    terms[index[k]] = line1;
                    Data[index[k]] = new List<string>();
                    while ((line1 = filePointers[index[k]].ReadLine()) != "&")
                    {
                        Data[index[k]].Add(line1);
                    }
                    k++;

                }
                else
                {
                    index.Remove(k);
                }
            }
            while (index.Count > 0)
            {
                int j = 0;
                int Min = index[j];
                while (j < index.Count - 1)
                {
                    int check = String.Compare(terms[Min], terms[index[j + 1]]);
                    if (check > 0)
                    {
                        Min = index[j + 1];
                    }
                    j++;
                }
                if (!indexerDic.ContainsKey(terms[Min]))
                {
                    if (terms[Min][0] != ' ' && !stopBigDic.ContainsKey(terms[Min]))
                    {
                        if (AllLetter.ContainsKey(terms[Min]))
                            AllLetter[terms[Min]].AddRange(Data[Min]);
                        else
                        {
                            if (AllLetter.Count == 25000)
                            {
                                Dump(path + @"\new" + filenum);
                                filenum++;
                                AllLetter.Add(terms[Min], Data[Min]);
                            }
                            else
                                AllLetter.Add(terms[Min], Data[Min]);
                        }
                    }
                    
                }
                
                string line2;
                if ((line2 = filePointers[Min].ReadLine()) != null)
                {
                    terms[Min] = line2;
                    Data[Min] = new List<string>();
                    while ((line2 = filePointers[Min].ReadLine()) != "&")
                    {
                        Data[Min].Add(line2);
                    }

                }
                else
                {
                    index.Remove(Min);
                }
            }
            if (AllLetter.Count > 0)
            {
                Dump(path + @"\new" + filenum);
            }
        }

        public void returnIndex(string patht)
        {
            indexerDic = new Dictionary<string, int>();
            int typefileend = 0;
            string[] Files = System.IO.Directory.GetFiles(patht);
            for (int i = 0; i < Files.Length; i++)
            {
                if (Files[i].Contains("new"))
                {
                    typefileend++;
                    string line = "";
                    string fPath = path + @"\new" + typefileend +".txt";
                    StreamReader read = new StreamReader(fPath);
                    while ((line = read.ReadLine()) != null)
                    {
                        indexerDic.Add(line, typefileend);
                        while ((line = read.ReadLine()) != "&");
                    }
                }
            }
        }
    }
}
