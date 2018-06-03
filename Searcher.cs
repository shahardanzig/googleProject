using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace WpfApplication1
{
    public class Searcher
    {
        public Dictionary<string, int> indexerDic;
        public Dictionary<string,KeyValuePair<double[],string>> docsInfo;
        public Dictionary<string, KeyValuePair<int[], string>> DicDoc;
        public string path;
        public bool withM;
        public string[] qterms;
        public double QuerySQsum;
        public int monthA;
        public int monthB;
        public int avgdl;

        

        public Searcher()
        {
            docsInfo = new Dictionary<string, KeyValuePair<double[], string>>();
            DicDoc = new Dictionary<string, KeyValuePair<int[], string>>();
            QuerySQsum = 0;
            monthA = 1;
            monthB = 12;
            withM = false;
        }

        public List<string> breakQuery(string query)
        {
            query = query.Trim().ToLower();
            string[] Qterms = query.Split(' ');
            List<string> listQt = new List<string>();
            listQt.AddRange(Qterms);
            QuerySQsum = Qterms.Length;
            return listQt;
            
            
        }




        public void initQuery(List<string> Qterms)
        {

            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatch = Stopwatch.StartNew();
            if (monthA == 1 && monthB == 12)
            {
                withM = false;
            }
            else
            {
                withM = true;
            }
            Qterms.Sort();
            List<string> Perfile = new List<string>();
            int tmp = indexerDic[indexerDic.Keys.First()];
            foreach (string str in Qterms)
            {
                if (indexerDic.ContainsKey(str))
                {
                    if (tmp != indexerDic[str])
                    {
                        if(Perfile.Count!=0)
                        {
                            Search(Perfile, tmp);
                        }
                        tmp = indexerDic[str];
                        Perfile = new List<string>();
                        Perfile.Add(str);
                    }
                    else
                    {
                        Perfile.Add(str);
                    }
                }
            }
            Search(Perfile, tmp);

            double st = stopwatch.ElapsedMilliseconds;
        }


        public void Search(List<string> query, int FileNum)
        {

            string tmpQ = "";
            string s = path + @"\new" + FileNum + ".txt";
            StreamReader file = new StreamReader(s);
            string line;
            bool found = false;
            while ((line = file.ReadLine()) != null)
            {
                if (query.Count == 0)
                    break;

                found = false;

                for (int i = 0; i < query.Count; i++)
                {
                    if (line == query[i])
                    {
                        found = true;
                        tmpQ = query[i];
                        query.RemoveAt(i);
                        break;
                    }
                }
                if (found)
                {
                    line = file.ReadLine();
                    bool two = false;

                    double idf2 = 0;
                    for (int i = 0; i < line.Length; i++)
                    {
                        if (line[i] == '~' && two)
                        {
                            int thirdtilda = line.IndexOf('~', i + 1);
                            idf2 = Double.Parse(line.Substring(i + 1, thirdtilda - i - 1));
                            break;
                        }
                        if (line[i] == '~')
                            two = true;
                        
                    }
                    while ((line = file.ReadLine()) != "&")
                    {
                        int firsttilda = line.IndexOf('~');
                        int secondtilda = line.IndexOf('~', firsttilda + 1);
                        int thirdtilda = line.IndexOf('~', secondtilda + 1);
                        string docName = line.Substring(secondtilda + 1, line.IndexOf('|') - secondtilda - 1);
                        double rankA = 1;
                        string TI = DicDoc[docName].Value.Trim().ToLower();
                        
                        if (TI.Contains(tmpQ))
                        {
                            rankA = 7;
                        }
                        
                        if (docsInfo.ContainsKey(docName))
                        {
                            double tf = rankA*Double.Parse(line.Substring(firsttilda+1, secondtilda - firsttilda - 1)) * 100;
                            double bm25 = (idf2 * ((tf * (2 + 1)) / (tf + 2 * (1 - 0 + 0 * (DicDoc[docName].Key[2] / avgdl))) + 1));
                            docsInfo[docName].Key[0]++;
                            docsInfo[docName].Key[1] += bm25;
                        }
                        else
                        {
                            double tf = rankA * Double.Parse(line.Substring(firsttilda + 1, secondtilda - firsttilda - 1)) * 1;
                            if (withM == false)
                            {
                                double bm25 = (idf2 * ((tf * (2 + 1)) / (tf + 2 * (1 - 0 + 0 * (DicDoc[docName].Key[2] / avgdl))) + 1));
                                docsInfo.Add(docName, new KeyValuePair<double[], string>(new double[] { 1, bm25 }, DicDoc[docName].Value));
                            }
                            else
                            {
                                double bm25 = (idf2 * ((tf * (2 + 1)) / (tf + 2 * (1 - 0 + 0 * (DicDoc[docName].Key[2] / avgdl))) + 1));
                                int mon = DicDoc[docName].Key[3];
                                
                                if (mon <= monthB && mon >= monthA)
                                {
                                    docsInfo.Add(docName, new KeyValuePair<double[], string>(new double[] { 1, bm25 }, DicDoc[docName].Value));
                                }
                            }
                        }
                    }
                }
                else
                {
                    while ((line = file.ReadLine()) != "&") ;
                }
            }
        }
    }
}
